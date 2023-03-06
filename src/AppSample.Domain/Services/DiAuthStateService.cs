using AppSample.CoreTools.Extensions;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Redis;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AppSample.Domain.Services;

public class DiAuthStateService : IDiAuthStateService
{
    const int CodeLength = 6;
    const string CodeAllowedChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly TimeSpan CodeTimeout = TimeSpan.FromMinutes(10);

    /// <summary>
    /// тайм-аут на DI авторизацию
    /// </summary>
    static readonly TimeSpan DiAuthFrontTimeout = TimeSpan.FromSeconds(70);

    /// <summary>
    /// Время хранения состояния в Redis
    /// </summary>
    static readonly TimeSpan StateTimeout = DiAuthFrontTimeout.Add(TimeSpan.FromMinutes(1));

    readonly IRedisService _redisService;
    readonly ILogger<DiAuthStateService> _logger;

    public DiAuthStateService(IRedisService redisService,
        ILogger<DiAuthStateService> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public async Task InitAsync(Guid sessionId)
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime timeOutTime = utcNow.Add(DiAuthFrontTimeout);
        await _redisService.SetStringAsync(RedisTimeoutKey(sessionId), timeOutTime.ToUnixTimeSeconds().ToString(), StateTimeout);
    }

    public async Task ProcessTimeoutAsync(Guid sessionId)
    {
        await _redisService.SetStringAsync(RedisStatusKey(sessionId), CheckConfirmationStatus.Timeout.ToString(), StateTimeout,
            CommandFlags.None, When.NotExists);
    }

    public async Task ProcessRejectAsync(Guid sessionId)
    {
        await _redisService.SetStringAsync(RedisStatusKey(sessionId), CheckConfirmationStatus.Reject.ToString(), StateTimeout,
            CommandFlags.None, When.NotExists);
    }

    public async Task ProcessConfirmAsync(Guid sessionId, AuthenticatorType authenticatorType)
    {
        //пока не сохранили код, проставляем временный статус Wait
        bool success = await _redisService.SetStringAsync(RedisStatusKey(sessionId), CheckConfirmationStatus.Wait.ToString(), StateTimeout,
            CommandFlags.None, When.NotExists);
        if (success)
        {
            string code;
            bool setResult;
            do
            {
                code = StringHelper.GeneratePassword(CodeLength, CodeAllowedChars);
                //сохраняем authId по коду
                setResult = await _redisService.SetStringAsync(RedisAuthIdByCodeKey(code), sessionId.ToString(), CodeTimeout, CommandFlags.None, When.NotExists);
            } while (setResult == false); //повторяем создание кода, если запись в Redis уже была

            //сохраняем созданный код
            await _redisService.SetStringAsync(RedisCodeKey(sessionId), code, StateTimeout);
            await _redisService.SetStringAsync(RedisAuthenticatorType(code), ((int)authenticatorType).ToString(), CodeTimeout);
            //сохраняем статус Ok после сохранения кода,
            //чтобы при получении успешного статуса код всегда был определен
            await _redisService.SetStringAsync(RedisStatusKey(sessionId), CheckConfirmationStatus.OK.ToString(), StateTimeout);
        }
    }

    /// <summary>
    /// Получение статуса попытки авторизации
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<DiStatusInfo> GetStatusAsync(Guid sessionId)
    {
        DateTime utcNow = DateTime.UtcNow;

        var timeOutStr = await _redisService.GetStringAsync(RedisTimeoutKey(sessionId));

        if (string.IsNullOrEmpty(timeOutStr) || long.TryParse(timeOutStr, out var timeOutSeconds) == false) //нет данных о времени тайм-аута по этой сессии
        {
            return new DiStatusInfo()
            {
                Status = CheckConfirmationStatus.Timeout
            };
        }

        var timeOutTime = DateTimeOffset.FromUnixTimeSeconds(timeOutSeconds).DateTime;
        var statusStr = await _redisService.GetStringAsync(RedisStatusKey(sessionId));

        //проверяем, не пришло ли время тайм-аута
        if (string.IsNullOrEmpty(statusStr) && utcNow > timeOutTime)
        {
            statusStr = CheckConfirmationStatus.Timeout.ToString();
            bool writeResult = await _redisService.SetStringAsync(RedisStatusKey(sessionId), statusStr, StateTimeout,
                CommandFlags.None, When.NotExists);
            if (writeResult == false)
            {
                //если запись не прошла из-за того, что значение уже установлено - то считываем это значение
                statusStr = await _redisService.GetStringAsync(RedisStatusKey(sessionId));
            }
        }

        if (string.IsNullOrEmpty(statusStr) || Enum.TryParse(statusStr, out CheckConfirmationStatus status) == false
            || status == CheckConfirmationStatus.Wait)
        {
            return new DiStatusInfo()
            {
                Status = CheckConfirmationStatus.Wait,
                TimerRemaining = (int) Math.Floor((timeOutTime - utcNow).TotalSeconds)
            };
        }

        if (status == CheckConfirmationStatus.OK)
        {
            return new DiStatusInfo()
            {
                Status = status,
                Code = await _redisService.GetStringAsync(RedisCodeKey(sessionId))
            };
        }

        return new DiStatusInfo()
        {
            Status = status
        };
    }

    /// <summary>
    /// Получение идентификатора авторизации по одноразовому коду.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<(Guid?, AuthenticatorType)> GetAuthIdByOneTimeCodeAsync(string code)
    {
        if (!await _redisService.IsExistsAsync(RedisAuthIdByCodeKey(code)))
            return (null, AuthenticatorType.NoValue);
        
        var authIdStr = await _redisService.GetAndDeleteStringAsync(RedisAuthIdByCodeKey(code));
        if (string.IsNullOrEmpty(authIdStr))
            return (null, AuthenticatorType.NoValue);

        if (!Guid.TryParse(authIdStr, out var authId))
            return (null, AuthenticatorType.NoValue);
        
        var authenticatorTypeStr = await _redisService.GetAndDeleteStringAsync(RedisAuthenticatorType(code));
        return Enum.TryParse(authenticatorTypeStr, out AuthenticatorType authenticatorType)
            ? (authId, authenticatorType)
            : (authId, AuthenticatorType.NoValue);
    }

    /// <summary>
    /// Ключ для хранения конечного статуса попытки авторизации ("OK", "Reject", "Timeout")
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    string RedisStatusKey(Guid sessionId) => $"DI-Status:{sessionId}";

    /// <summary>
    /// Ключ для хранения времени истечения тайм-аута попытки авторизации (в формате unix time)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    string RedisTimeoutKey(Guid sessionId) => $"DI-Timeout:{sessionId}";

    /// <summary>
    /// Ключ для хранения созданного кода после успешной авторизации
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    string RedisCodeKey(Guid sessionId) => $"DI-Code:{sessionId}";

    /// <summary>
    /// Ключ для хранения идентификатора попытки авторизации по коду, выданному после успешной авторизации
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    string RedisAuthIdByCodeKey(string code) => $"DI-AuthId:{code}";
    
    string RedisAuthenticatorType(string code) => $"DI-AuthenticatorType:{code}";
}