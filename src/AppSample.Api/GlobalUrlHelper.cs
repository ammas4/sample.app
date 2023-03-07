using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using AppSample.Domain.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace AppSample.Api;

/// <summary>
/// Предоставляет адреса эндпоинтов
/// </summary>
public class GlobalUrlHelper : IGlobalUrlHelper
{
    readonly IdgwSettings _idgwSettings;
    readonly IServiceProvider _services;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="idgwSettings"></param>
    /// <param name="services"></param>
    public GlobalUrlHelper(
        IOptions<IdgwSettings> idgwSettings,
        IServiceProvider services
    )
    {
        _idgwSettings = idgwSettings.Value;
        _services = services;
    }

    /// <summary>
    /// Полный путь эндпоинта с разметкой для ussd-сообщния
    /// </summary>
    /// <returns></returns>
    public string GetUssdAskUserTextUrl()
    {
        //return RouteFullUrl(UssdController.AskUserTextRouteName);

        // сделал так, т.к. ActionContext не всегда доступен
        return CombineToFullUrl(_idgwSettings.BasePath, ControllerUrls.AskUserTextPath);
    }

    /// <summary>
    /// Полный адрес эндпоинта для приёма ответа на пуш mc-апплета
    /// </summary>
    /// <returns></returns>
    public string GetMcPushAnswerUrl()
    {
        return CombineToFullUrl(_idgwSettings.BasePath, ControllerUrls.McPushAnswerPath);
    }

    /// <summary>
    /// Полный адрес эндпоинта для приёма ответа на пуш с пин-кодом mc-апплета
    /// </summary>
    /// <returns></returns>
    public string GetMcPushPinAnswerUrl()
    {
        return CombineToFullUrl(_idgwSettings.BasePath, ControllerUrls.McPushPinAnswerPath);
    }

    /// <summary>
    /// Полный адрес эндпоинта для приёма ответа на пуш dstk-апплета
    /// </summary>
    /// <returns></returns>
    public string GetDstkMcPushAnswerUrl()
    {
        return CombineToFullUrl(_idgwSettings.BasePath, ControllerUrls.DstkPushAnswerPath);
    }

    /// <summary>
    /// Полный адрес эндпоинта для приёма ответа на пуш с пин-кодом dstk-апплета
    /// </summary>
    /// <returns></returns>
    public string GetDstkPushPinAnswerUrl()
    {
        return CombineToFullUrl(_idgwSettings.BasePath, ControllerUrls.DstkPushPinAnswerPath);
    }

    /// <summary>
    /// Полный адрес эндпоинта для инициализации определения номера абонета по IP адресу
    /// </summary>
    /// <param name="interactionId"></param>
    /// <returns></returns>
    public string GetHheRequestUrl(Guid interactionId) =>
        CombineToFullUrl(_idgwSettings.BasePath, $"{ControllerUrls.HheRequestUrl}?interaction_id={interactionId}");

    /// <summary>
    /// Полный адрес эндпоинта для завершения определения номера абонета по IP адресу.
    /// Здесь используется отдельная переменная с хостом, тк balance.beeline.ru проставляет
    /// токен для получения msisdn в куки для доменов .beeline.ru. Поэтому в ситуации, когда хост idgw
    /// не является поддоменом beeline.ru, в этом адресе будет использоваться alias-хост из beeline.ru.
    /// </summary>
    public string GetHheEnrichmentUrl(Guid interactionId) =>
        CombineToFullUrl(_idgwSettings.BasePathAliasForXbr, $"{ControllerUrls.HheEnrichmentUrl}?interaction_id={interactionId}");

    /// <summary>
    /// Генерирует полный путь к эндпоинту на основе имени маршрута
    /// Важно: ActionContext не всегда доступен (например из репозитория), что приводит к исключению
    /// </summary>
    /// <param name="routeName"></param>
    /// <returns></returns>
    public string RouteFullUrl(string routeName)
    {
        var urlHelperFactory = _services.GetService<IUrlHelperFactory>();
        var actionContextAccessor = _services.GetService<IActionContextAccessor>();

        var urlHelper = urlHelperFactory!.GetUrlHelper(actionContextAccessor!.ActionContext!);

        return urlHelper.RouteFullUrl(routeName, _idgwSettings.BasePath);
    }

    static string CombineToFullUrl(string host, string url) =>
        AppSample.CoreTools.Helpers.UrlHelper.Combine(host, url);
}

/// <summary>
/// 
/// </summary>
public static class UrlHelperExtensions
{
    /// <summary>
    /// Генерирует полный путь к эндпоинту на основе имени маршрута
    /// </summary>
    /// <param name="urlHelper"></param>
    /// <param name="routeName"></param>
    /// <param name="host"></param>
    /// <returns></returns>
    public static string RouteFullUrl(this IUrlHelper urlHelper, string routeName, string host)
    {
        var url = urlHelper.RouteUrl(routeName);
        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException($"Нет роута с таким именем {routeName}.");

        return AppSample.CoreTools.Helpers.UrlHelper.Combine(host, url);
    }
}