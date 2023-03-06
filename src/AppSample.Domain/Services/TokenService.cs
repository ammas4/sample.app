using System.Security.Cryptography;
using System.Text;
using AppSample.Domain.Extensions.Models;
using AppSample.Domain.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AppSample.Domain.Services;

public class TokenService : ITokenService
{
    readonly ISubjectService _subjectService;
    readonly IdgwSettings _settings;

    public TokenService(ISubjectService subjectService, IOptions<IdgwSettings> settings)
    {
        _subjectService = subjectService;
        _settings = settings.Value;
    }

    public async Task<CreateTokensResult> CreateTokensAsync(CreateTokensCommand command, AuthenticatorType authenticatorType)
    {
        var accessTokenLifetime = TimeSpan.FromSeconds(_settings.AccessTokenDefaultLifetimeSec);

        var issuedAt = DateTime.UtcNow;

        var subjectId = await _subjectService.GetOrCreateSubject(command.Msisdn, command.ServiceProvider.Id);

        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(_settings.PrivateKey.Sig), out _);

        SigningCredentials signingCredentials = new(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = JwtSignatureValidator.CryptoProviderFactory
        };

        signingCredentials.Key.KeyId = HashHelper.HashStringFromUtf8(_settings.PrivateKey.Sig);

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object>()
            {
                {JwtRegisteredClaimNames.Sub, subjectId},
                {JwtRegisteredClaimNames.Azp, command.ServiceProvider.ClientId},
                {JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()},
            },
            Expires = issuedAt.Add(accessTokenLifetime),
            IssuedAt = issuedAt,
            Issuer = _settings.TokenIssuer,
            Audience = command.ServiceProvider.ClientId,
            SigningCredentials = signingCredentials,
            //TokenType = "at+jwt",
        };
        var tokenHandler = new JsonWebTokenHandler()
        {
            SetDefaultTimesOnTokenCreation = false
        };
        var accessToken = tokenHandler.CreateToken(descriptor);

        var atHash = GetAccessTokenHash(accessToken);

        var idToken = CreateIdToken(command, authenticatorType, signingCredentials, subjectId, issuedAt, atHash);


        CreateTokensResult result = new CreateTokensResult()
        {
            AccessToken = accessToken,
            IdToken = idToken,
            ExpiresIn = Convert.ToInt32(accessTokenLifetime.TotalSeconds),
            TokenType = AuthorizationSchemes.Bearer
        };

        return result;
    }

    static string GetAccessTokenHash(string accessToken)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(accessToken));
        // https://openid.net/specs/openid-connect-core-1_0.html#CodeIDToken
        return Base64UrlEncoder.Encode(hash.Take(hash.Length / 2).ToArray());
    }

    static string GetHashedLoginHint(string msisdn)
    {
        var loginHint = "MSISDN:" + msisdn;
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(loginHint));
        return Convert.ToHexString(hash).ToLower();
    }

    string CreateIdToken(
        CreateTokensCommand command,
        AuthenticatorType authenticatorType,
        SigningCredentials signingCredentials,
        Guid subjectId,
        DateTime issuedAt,
        string atHash)
    {
        var identityTokenLifetime = TimeSpan.FromSeconds(_settings.IdentityTokenDefaultLifetimeSec);

        var amr = GetAuthenticationMethodReference(authenticatorType);
        var hashedLoginHint = GetHashedLoginHint(command.Msisdn);
        var acr = AcrValuesHelper.GetFirstSupportedValue(command.AcrValues);

        var claims = new Dictionary<string, object>()
        {
            {JwtRegisteredClaimNames.Sub, subjectId},
            {JwtRegisteredClaimNames.Azp, command.ServiceProvider.ClientId!},
            {JwtRegisteredClaimNames.Nonce, command.Nonce},
            {JwtRegisteredClaimNames.Acr, acr},
            {JsonWebKeyParameterNames.Kid, signingCredentials.Kid},
            {JwtRegisteredClaimNames.AuthTime, EpochTime.GetIntDate(issuedAt)},
            {JwtRegisteredClaimNames.AtHash, atHash},
            {JwtRegisteredClaimNames.Amr, new[] {amr}},
            {JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()},
            {"hashed_login_hint", hashedLoginHint}
        };

        if (command.Mode == MobileIdMode.SI)
        {
            if (command.ResponseType != MobileConnectResponseTypes.SIPolling)
            {
                claims.Add("recipient", command.NotificationUri);
            }
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Audience = command.ServiceProvider.ClientId,
            Expires = issuedAt.Add(identityTokenLifetime),
            IssuedAt = issuedAt,
            Issuer = _settings.TokenIssuer,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JsonWebTokenHandler()
        {
            SetDefaultTimesOnTokenCreation = false
        };

        return tokenHandler.CreateToken(descriptor);
    }

    static string GetAuthenticationMethodReference(AuthenticatorType authenticatorType) =>
        authenticatorType switch
        {
            AuthenticatorType.SmsWithUrl => "SMS_URL_OK",
            AuthenticatorType.SmsOtp => "OTP_OK",
            AuthenticatorType.Ussd => "USSD_OK",
            AuthenticatorType.Seamless => "SEAM_OK",
            AuthenticatorType.PushMc => "PUSH_OK",
            AuthenticatorType.PushDstk => "PUSH_OK",
            _ => throw new ArgumentException("Invalid argument value", nameof(authenticatorType))
        };
}