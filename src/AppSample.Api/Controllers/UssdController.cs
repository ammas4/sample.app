using AppSample.Api.Models;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AppSample.Domain.Services.AuthenticationChain;
using AppSample.Domain.Services.Authenticators;
using AppSample.Domain.Helpers;

namespace AppSample.Api.Controllers;

public class UssdController : Controller
{
    public const string UserAnswerRouteName = "UssdUserAnswer";
    public const string AskUserTextRouteName = "AskUserText";
    public const string AskUserTextPath = "ussd/askusertext.ru";

    readonly IdgwSettings _idgwSettings;
    readonly IUssdAuthenticator _ussdAuthenticator;
    readonly IAuthenticationChainSession _authenticationChainSession;
    readonly ICachedConfigService _cachedConfigService;
    readonly ILogger<UssdController> _logger;

    public UssdController(
        IOptions<IdgwSettings> idgwSettings,
        IUssdAuthenticator ussdAuthenticator,
        IAuthenticationChainSession authenticationChainSession,
        ICachedConfigService cachedConfigService,
        ILogger<UssdController> logger)
    {
        _idgwSettings = idgwSettings.Value;
        _ussdAuthenticator = ussdAuthenticator;
        _authenticationChainSession = authenticationChainSession;
        _cachedConfigService = cachedConfigService;
        _logger = logger;
    }

    [HttpGet(AskUserTextPath, Name = AskUserTextRouteName)]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> AskUserText(long msisdn)
    {
        var (isAlive, session) = await _authenticationChainSession.TryGetAsync(msisdn.ToString());
        if (!isAlive)
            return NotFound();

        var serviceProviders = _cachedConfigService.GetState().ServiceProvidersById;
        serviceProviders.TryGetValue(session.Bag.AuthRequest.ServiceProviderId, out var serviceProvider);
        string? context;
        string? bindingMessage;

        if(serviceProvider?.Scopes?.IsCustomMessageRequired(session.Bag.Scopes, out var message) == true && message != null)
        {
            context = message.Replace("%ClientName%", serviceProvider.Name);
            bindingMessage = null;
        }
        else
        {
            context = session.Bag.AuthRequest.Context;
            bindingMessage = session.Bag.AuthRequest.BindingMessage;
        }

        return View(new AskUserTextVm
        {
            IdgwSettings = _idgwSettings,
            ServiceProvider = serviceProvider,
            Context = context,
            BindingMessage = bindingMessage
        });
    }

    [HttpGet("ussd/useranswer", Name = UserAnswerRouteName)]
    public async Task<IActionResult> UserAnswer(byte response, long msisdn)
    {
        try
        {
            var isSuccessful = await _ussdAuthenticator.ProcessUserAnswerAsync(response, msisdn);

            if( response == 1 )
                return View(isSuccessful);
        }
        catch( Exception ex )
        {
            _logger.LogError(ex, "Failed ussd confirmation");
        }
        return View(false);
    }
}