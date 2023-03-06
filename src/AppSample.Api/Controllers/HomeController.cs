using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers;

/// <summary>
/// 
/// </summary>
[AllowAnonymous]
[Route("")]
public class HomeController : Controller
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult Index()
    {
        return Content("AppSample.Api");
    }
}