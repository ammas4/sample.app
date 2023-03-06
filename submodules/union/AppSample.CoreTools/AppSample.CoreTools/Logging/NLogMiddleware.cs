using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using AppSample.CoreTools.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace AppSample.CoreTools.Logging;

public class NLogMiddleware
{
    readonly RequestDelegate _next;
    readonly LogRequestResponseSettings _loggingSettings;

    readonly IRequestResponseLogger _logger;
    readonly IHttpContextAccessor _contextAccessor;

    public NLogMiddleware(RequestDelegate next,
        IOptions<LogRequestResponseSettings> loggingSettings,
        IRequestResponseLogger logger,
        IHttpContextAccessor contextAccessor)
    {
        _next = next;
        _logger = logger;
        _contextAccessor = contextAccessor;

        _loggingSettings = loggingSettings.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.ToString();
        if (!AnyStartWith(_loggingSettings.RootPath, path) || AnyStartWith(_loggingSettings.ExcludePath, path))
        {
            await _next(context);
            return;
        }

        LogHelper.SetActivityId();

        var uriString = context.Request.GetDisplayUrl();
        var requestBody = await LogRequestHelper.PrepareRequestBody(context.Request);

        var httpContext = _contextAccessor.HttpContext;

        var logItem = new LogItem
        {
            RequestBody = requestBody,
            RequestHeaders = LogRequestHelper.TransformToDictionary(context.Request.Headers),
            RequestMethod = context.Request.Method,
            RequestUrl = uriString,
            IP = httpContext?.GetRemoteIPAddress()?.ToString()
        };

        var originalBody = context.Response.Body;
        var time = Stopwatch.StartNew();
        try
        {
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await _next(context);
            time.Stop();

            bool skipResponseBodyFromContext = false;
            if (httpContext != null)
            {
                if (httpContext.Items.TryGetValue(SkipResponseBodyContextKey, out var skipResponseBodyValue) && skipResponseBodyValue is bool skipResponseBodyBoolValue)
                    skipResponseBodyFromContext = skipResponseBodyBoolValue;
                if (httpContext.Items.TryGetValue("Ctn", out var ctnValue))
                    logItem.Ctn = ctnValue?.ToString();
            }

            var responseBody = string.Empty;

            if (skipResponseBodyFromContext == false
                && AnyStartWith(_loggingSettings.SkipResponseBody, path) == false
                && ((context.Response.StatusCode >= 200 && context.Response.StatusCode <= 299) || context.Response.StatusCode >= 400))
            {
                memStream.Position = 0;
                responseBody = new StreamReader(memStream).ReadToEnd();
            }

            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);

            logItem.ResponseBody = responseBody;
            logItem.ResponseHeaders = LogRequestHelper.TransformToDictionary(context.Response.Headers);
            logItem.ResponseStatusCode = (HttpStatusCode) context.Response.StatusCode;
        }
        catch (Exception exception)
        {
            time.Stop();
            logItem.Error = exception.ToString();

            throw;
        }
        finally
        {
            logItem.Time = time.Elapsed;
            _logger.Log(logItem);
            context.Response.Body = originalBody;
        }
    }

    static bool AnyStartWith(PathString[]? pathStrings, string path)
    {
        var result = pathStrings?.Any(x => path.StartsWith(x.ToString(), StringComparison.OrdinalIgnoreCase)) ?? false;
        return result;
    }

    /// <summary>
    /// Установка флага в Context.Items о том, что логировать тело ответа не нужно
    /// </summary>
    /// <param name="context"></param>
    public static void SkipResponseBodyLogging(HttpContext context)
    {
        context.Items[SkipResponseBodyContextKey] = true;
    }

    static readonly string SkipResponseBodyContextKey = typeof(NLogMiddleware).FullName + "." + nameof(SkipResponseBodyContextKey);
}