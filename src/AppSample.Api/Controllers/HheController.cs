using AppSample.Domain.Services.Authenticators;
using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.CoreTools.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers;

[ApiController]
public class HheController : ControllerBase
{
    public const string HheRequestUrl = "/hhe/request";
    public const string HheEnrichmentUrl = "/hhe/enrichment";

    readonly ISeamlessAuthenticator _seamlessAuthenticator;

    public HheController(ISeamlessAuthenticator seamlessAuthenticator)
    {
        _seamlessAuthenticator = seamlessAuthenticator;
    }

    [HttpGet(HheRequestUrl)]
    public async Task<IActionResult> HheRequest([FromQuery(Name = "interaction_id")] string? interactionId)
    {
        if (Guid.TryParse(interactionId, out Guid id) == false)
            throw new UnifiedException(OAuth2Error.NotFoundEntity);

        var ipAddress = HttpContext.GetRemoteIPAddress(allowForwarded: true);
        string url = await _seamlessAuthenticator.ProcessRequestAsync(id, ipAddress);
        return Redirect(url);
    }

    [HttpGet(HheEnrichmentUrl)]
    public async Task<IActionResult> HheEnrichment([FromQuery(Name = "interaction_id")] string? interactionId)
    {
        if (Guid.TryParse(interactionId, out Guid id) == false)
            throw new UnifiedException(OAuth2Error.NotFoundEntity);

        var xbrToken = Request.Cookies["token"];
        await _seamlessAuthenticator.ProcessResultAsync(id, xbrToken);
        return Content("OK");
    }
}