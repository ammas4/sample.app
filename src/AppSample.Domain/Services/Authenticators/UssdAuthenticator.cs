using AppSample.Domain.DAL;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Services.AuthenticationChain;

namespace AppSample.Domain.Services.Authenticators;

public interface IUssdAuthenticator : IAuthStarter
{
    /// <summary>
    /// Обрабатывает ответ пользователя на Ussd-запрос
    /// </summary>
    /// <param name="userAnswer"></param>
    /// <param name="msisdn"></param>
    /// <returns></returns>
    Task<bool> ProcessUserAnswerAsync(byte userAnswer, long msisdn);
}

public class UssdAuthenticator : IUssdAuthenticator
{
    readonly IUssdRepository _ussdRepository;
    readonly IAuthenticationChainService _authenticationChainService;
    readonly IUserProfileRepository _userProfileRepository;

    public UssdAuthenticator(
        IUssdRepository ussdRepository,
        IAuthenticationChainService authenticationChainService,
        IUserProfileRepository userProfileRepository
        )
    {
        _ussdRepository = ussdRepository;
        _authenticationChainService = authenticationChainService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<AuthenticatorStartResult> TryStartAsync(AuthSessionBag startBag)
    {
        if (startBag.AuthRequest.MinLoA == AcrValues.LoA3)
            return StartResult(AuthenticatorStartResultType.LoA3NotSupported);

        if (!long.TryParse(startBag.AuthRequest.Msisdn, out var msisdn))
            return StartResult(AuthenticatorStartResultType.MsisdnInvalid);

        var userProfile = await _userProfileRepository.GetUserProfile(msisdn);
        if (userProfile == null) return StartResult(AuthenticatorStartResultType.UserInfoNotFound);
        if (userProfile.IsBlocked) return StartResult(AuthenticatorStartResultType.UserBlocked);

        var (isOk, _) = await _ussdRepository.AskUserForConsistAsync(startBag.AuthRequest.Msisdn);
        return StartResult(isOk ? AuthenticatorStartResultType.Started : AuthenticatorStartResultType.NotSent);
    }

    static AuthenticatorStartResult StartResult(AuthenticatorStartResultType resultType) => new()
    {
        Authenticator = AuthenticatorType.Ussd,
        ResultType = resultType,
        StartedLoA = AcrValues.LoA2
    };

    public async Task<bool> ProcessUserAnswerAsync(byte userAnswer, long msisdn)
    {
        const int userDenyNumber = 2;
        var result = userAnswer == userDenyNumber ? AuthResult.UserDenied : AuthResult.UserAgree;

        return await _authenticationChainService.TryEndAsync(msisdn.ToString(), AuthenticatorType.Ussd, result);
    }
}