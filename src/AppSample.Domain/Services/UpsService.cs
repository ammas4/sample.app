using System.Text;
using System.Text.Json;
using AppSample.CoreTools.Helpers;
using AppSample.CoreTools.Infrastructure;
using AppSample.CoreTools.Infrastructure.Interfaces;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AppSample.Domain.Models.Ups;
using AppSample.Domain.DAL.DTOs;

namespace AppSample.Domain.Services;

public class UpsService : IUpsService
{
    readonly ICachedConfigService _cachedConfigService;
    readonly UpsSettings _settings;
    IUserProfileRepository _userProfileRepository;
    readonly ILogger<UpsService> _logger;

    public UpsService(IOptions<UpsSettings> settings, ILogger<UpsService> logger,
        ICachedConfigService cachedConfigService, IUserProfileRepository userProfileRepository)
    {
        _settings = settings.Value;
        _logger = logger;
        _cachedConfigService = cachedConfigService;
        _userProfileRepository = userProfileRepository;
    }

    public async Task ReportAboutAuthResult(bool wasSuccess, AuthorizationRequestDto authReqDto)
    {
        if ((_settings.ReportAboutAuthResult ?? false) == false) return; //отключено в настройках

        if (long.TryParse(authReqDto.Msisdn, out long msisdn) == false) return;
        var configState = _cachedConfigService.GetState();
        if (configState.ServiceProvidersById.TryGetValue(authReqDto.ServiceProviderId, out var serviceProvider) == false) return;

        var command = new HistoryCommand
        {
            Msisdn = msisdn,
            AuthStatus = wasSuccess ? AuthStatusEnum.Success : AuthStatusEnum.Error,
            AuthTime = DateTime.UtcNow,
            SpName = serviceProvider.Name
        };

        try
        {
            await _userProfileRepository.SendHistory(command);
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "UPS HTTP request error");
        }
    }
}