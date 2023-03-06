using AppSample.CoreTools.Extensions;
using AppSample.CoreTools.Helpers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AppSample.Domain.DAL;
using AppSample.Domain.Helpers;
using AppSample.Domain.Models.ServiceProviders;
using MsisdnHelper = AppSample.CoreTools.Helpers.MsisdnHelper;

namespace AppSample.Domain.Services;

public class ConsentSmsService: IConsentSmsService
{
    readonly IdgwSettings _idgwSettings;
    readonly ILogger<ConsentSmsService> _logger;
    readonly ISmsSmppRepository _smsRepository;
    readonly IShortUrlService _shortUrlService;
    readonly ISmppRepository _smppRepository;
    readonly IDeviceAdapterRepository _deviceAdapterRepository;

    public ConsentSmsService(
        IOptions<IdgwSettings> idgwSettings,
        ILogger<ConsentSmsService> logger,
        ISmsSmppRepository smsRepository,
        IShortUrlService shortUrlService, ISmppRepository smppRepository, IDeviceAdapterRepository deviceAdapterRepository)
    {
        _idgwSettings = idgwSettings.Value;
        _logger = logger;
        _smsRepository = smsRepository;
        _shortUrlService = shortUrlService;
        _smppRepository = smppRepository;
        _deviceAdapterRepository = deviceAdapterRepository;
    }

    public async Task<bool> SendAsync(ConsentSmsRequest request, Guid code, bool isPaymentScopeSelected)
    {
        if (MsisdnHelper.IsRussianNumber(request.Msisdn) == false) throw new ArgumentOutOfRangeException(nameof(request.Msisdn));

        var obj = new {request.Msisdn, code};
        _logger.LogInformation($"ConsentSmsService send: {obj}");

        var ctn = MsisdnHelper.GetCtnFromRussianNumber(request.Msisdn);

        var shortCode = StringHelper.ToShortGuid(code);
        var method = isPaymentScopeSelected ? _idgwSettings.ConsentPayMethod : _idgwSettings.ConsentMethod;
        var link = $"{_idgwSettings.BasePath}{method}?code={shortCode}";
        if(isPaymentScopeSelected == false)
            link = await _shortUrlService.MinifyUrlAsync(link, ctn.ToString());

        string text;

        if( request.ServiceProviderScopes?.IsCustomMessageRequired(request.RequestScopes, out var message) == true && message != null )
        {
            text = message.Replace("%ClientName%", request.ServiceProviderName) + ": " + link;
        }
        else if (!string.IsNullOrEmpty(request.Context) && string.IsNullOrEmpty(request.BindingMessage))
        {
            // для scope mc_authz максимальное значение context - 64 символа
            text = (request.IsAuthzScopeSelected ? request.Context.Truncate(64) : request.Context.Truncate(90)) + ": " + link;
        }
        else if (string.IsNullOrEmpty(request.Context) && !string.IsNullOrEmpty(request.BindingMessage))
        {
            text = (request.IsAuthzScopeSelected ? request.BindingMessage.Truncate(32) : request.BindingMessage.Truncate(54)) + ": " + link;
        }
        else if (!string.IsNullOrEmpty(request.Context) && !string.IsNullOrEmpty(request.BindingMessage))
        {
            text = (request.BindingMessage + " " + request.Context).Truncate(89) + ": " + link;
        }
        else
        {
            text = _idgwSettings.ConsentText
                .Replace("%ClientName%", request.ServiceProviderName)
                .Replace("%URL%", link);
        }

        try
        {
            var isSent = await _deviceAdapterRepository.SendSmsAsync(request.Msisdn, text);
            if (isSent) return true;
            
            return await _smsRepository.SendAsync(ctn, text);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Send SMS error");
            return false;
        }
    }

    public async Task<bool> SendOtpMessageAsync(ConsentSmsRequest request, string otpCode)
    {
        if (MsisdnHelper.IsRussianNumber(request.Msisdn) == false) throw new ArgumentOutOfRangeException(nameof(request.Msisdn));

        var obj = new {request.Msisdn, otpCode};
        _logger.LogInformation($"ConsentSmsService otp message: {obj}");

        var ctn = MsisdnHelper.GetCtnFromRussianNumber(request.Msisdn);
        string text;

        if (request.ServiceProviderScopes?.IsCustomMessageRequired(request.RequestScopes, out var message) == true && message != null)
        {
            text = message.Replace("%ClientName%", request.ServiceProviderName) + ": " + otpCode;
        }
        else if (!string.IsNullOrEmpty(request.Context) && string.IsNullOrEmpty(request.BindingMessage))
        {
            // для scope mc_authz максимальное значение context - 64 символа
            text = (request.IsAuthzScopeSelected ? request.Context.Truncate(64) : request.Context.Truncate(90)) + " " + otpCode;
        }
        else if (string.IsNullOrEmpty(request.Context) && !string.IsNullOrEmpty(request.BindingMessage))
        {
            text = (request.IsAuthzScopeSelected ? request.BindingMessage.Truncate(32) : request.BindingMessage.Truncate(54)) + " " + otpCode;
        }
        else if (!string.IsNullOrEmpty(request.Context) && !string.IsNullOrEmpty(request.BindingMessage))
        {
            text = (request.BindingMessage + " " + request.Context).Truncate(89) + " " + otpCode;
        }
        else
        {
            text = $"{_idgwSettings.OtpSmsMessage} {otpCode}";
        }

        try
        {
            var isSent = await _deviceAdapterRepository.SendSmsAsync(request.Msisdn, text);
            if (isSent) return true;
            
            return await _smsRepository.SendAsync(ctn, text);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Send SMS error");
            return false;
        }
    }
}