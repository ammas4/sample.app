using System.Collections.Immutable;
using System.Text.Json;
using AppSample.CoreTools.Redis;
using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using StackExchange.Redis;

namespace AppSample.Domain.Services.AuthenticationChain;

public interface IAuthenticationChainSession
{
    /// <summary>
    /// Пробует создать сессию для msisdn.
    /// У msisdn может быть только одна сессия в момент времени.
    /// Сессия имеет время жизни - таймаут текущего аутентификатора + небольшой оверхед (например, timeoutSmsOtp + 30 сек)
    /// </summary>
    /// <param name="msisdn"></param>
    /// <param name="scopes"></param>
    /// <param name="authRequest"></param>
    /// <param name="spNotificationToken"></param>
    /// <returns>false-если у msisdn уже есть сессия</returns>
    Task<(bool isCreated, AuthSessionDto session)> TryCreateAsync(string msisdn, ImmutableHashSet<string> scopes,
        AuthorizationRequestDto authRequest, string? spNotificationToken);

    Task<(bool isAlive, AuthSessionDto session)> TryGetAsync(string msisdn);

    Task<(bool isAlive, AuthSessionDto session)> UpdateAsync(string msisdn, AuthenticatorEntity authenticator,
        TimeSpan startNextDelay);

    Task<bool> IsAliveAsync(string msisdn);
    Task<bool> TryReleaseAsync(string msisdn);
}

public class AuthenticationChainSession : IAuthenticationChainSession
{
    readonly IRedisService _redisService;

    /// <summary>
    /// Добавочное время жизни сессии
    /// Нужно, чтобы было время на обработку таймаута текущего аутентификатора
    /// </summary>
    readonly TimeSpan _sessionTtlOverhead = TimeSpan.FromSeconds(30);

    public AuthenticationChainSession(IRedisService redisService)
    {
        _redisService = redisService;
    }

    string GetSessionRedisKey(string msisdn) => $"session-key:{msisdn}";

    public async Task<(bool isCreated, AuthSessionDto session)> TryCreateAsync(string msisdn, ImmutableHashSet<string> scopes,
        AuthorizationRequestDto authRequest, string? spNotificationToken)
    {
        if (string.IsNullOrEmpty(msisdn))
            return (false, default);

        var session = new AuthSessionDto(msisdn, scopes, authRequest, spNotificationToken);

        var sessionJson = JsonSerializer.Serialize(session);
        var sessionCreated = await _redisService.SetStringAsync(GetSessionRedisKey(session.Msisdn),
            sessionJson, _sessionTtlOverhead, when: When.NotExists);

        return (sessionCreated, session);
    }

    public async Task<(bool isAlive, AuthSessionDto session)> TryGetAsync(string msisdn)
    {
        var sessionStr = await _redisService.GetStringAsync(GetSessionRedisKey(msisdn));
        if (string.IsNullOrEmpty(sessionStr))
            return (false, default);

        var sessionJson = JsonSerializer.Deserialize<AuthSessionDto>(sessionStr);
        return (true, sessionJson);
    }

    public async Task<(bool isAlive, AuthSessionDto session)> UpdateAsync(string msisdn,
        AuthenticatorEntity authenticator, TimeSpan startNextDelay)
    {
        var (isAlive, session) = await TryGetAsync(msisdn);
        if (!isAlive)
            return (false, default);

        var updatedSession = new AuthSessionDto(session, authenticator);
        var sessionStr = JsonSerializer.Serialize(updatedSession);

        var sessionUpdated = await _redisService.SetStringAsync(GetSessionRedisKey(updatedSession.Msisdn),
            sessionStr, startNextDelay + _sessionTtlOverhead, when: When.Exists);

        return (sessionUpdated, updatedSession);
    }

    public async Task<bool> IsAliveAsync(string msisdn)
    {
        return await _redisService.IsExistsAsync(GetSessionRedisKey(msisdn));
    }

    public async Task<bool> TryReleaseAsync(string msisdn)
    {
        return await _redisService.DeleteAsync(GetSessionRedisKey(msisdn));
    }
}