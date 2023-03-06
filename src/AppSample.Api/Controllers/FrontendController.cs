using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AppSample.Api.Controllers;

/// <summary>
/// Контроллер для отдачи страниц от фронта
/// </summary>
[ApiController]
public class FrontendController : ControllerBase
{
    readonly IWebHostEnvironment _env;
    readonly IMemoryCache _memoryCache;

    public FrontendController(IWebHostEnvironment env, IMemoryCache memoryCache)
    {
        _env = env;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("result/expired")]
    [HttpGet("result/success")]
    [HttpGet("error")]
    [HttpGet("error-code")]
    [HttpGet("confirmation")]
    [HttpGet("reject")]
    [HttpGet("payment/confirm")]
    public async Task<IActionResult> GetContent()
    {
        return GetIndexHtmlContent();
    }

    ContentResult GetIndexHtmlContent()
    {
        var html = _memoryCache.GetOrCreate("FrontendController_content",
            _ => System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "index.html")));
        return Content(html, MediaTypeNames.Text.Html);
    }
}