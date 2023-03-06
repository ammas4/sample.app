using System.Diagnostics;
using System.Net.Http.Headers;

namespace AppSample.CoreTools.Logging;

public class ApiLoggingHandler : HttpClientHandler
{
    static readonly string[] _textContentTypes = { "html", "text", "xml", "json", "txt", "x-www-form-urlencoded" };

    readonly IRequestResponseLogger _logger;
    readonly string? _apiName;
    readonly Func<HttpContent, Task<string>>? _contentReader;
    readonly Func<HttpContent?, Task<bool>>? _errorChecker;

    public ApiLoggingHandler(IRequestResponseLogger logger,
        string? apiName = null,
        Func<HttpContent, Task<string>>? contentReader = null,
        Func<HttpContent?, Task<bool>>? errorChecker = null)
    {
        _logger = logger;
        _apiName = apiName;
        _contentReader = contentReader;
        _errorChecker = errorChecker;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var logItem = new LogItem()
        {
            Api = _apiName,
        };
        await FillRequestDataAsync(logItem, request);
        var stopWatch = Stopwatch.StartNew();
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopWatch.Stop();
            await FillResponseDataAsync(logItem, response);
            return response;
        }
        catch (Exception exception)
        {
            stopWatch.Stop();
            logItem.Error = (exception is OperationCanceledException) ? "Timeout" : exception.ToString();
            logItem.ErrorMessage = exception.Message;

            throw;
        }
        finally
        {
            logItem.Time = stopWatch.Elapsed;
            _logger.Log(logItem);
        }
    }

    async Task FillRequestDataAsync(LogItem logItem, HttpRequestMessage request)
    {
        logItem.RequestUrl = request.RequestUri?.ToString();
        logItem.RequestMethod = request.Method.Method;
        logItem.RequestHeaders = LogRequestHelper.TransformToDictionary(request.Headers);

        if (request.Content != null)
            logItem.RequestBody = await ReadContentAsStringAsync(request.Content, request.Headers);
    }

    async Task FillResponseDataAsync(LogItem log, HttpResponseMessage response)
    {
        log.ResponseStatusCode = response.StatusCode;
        log.ResponseHeaders = LogRequestHelper.TransformToDictionary(response.Headers);
        if (response.Content != null && response.Content.Headers.ContentLength != 0)
            log.ResponseBody = await ReadContentAsStringAsync(response.Content, response.Headers);
        if (_errorChecker != null) log.WasError = await _errorChecker(response.Content);
    }

    Task<string> ReadContentAsStringAsync(HttpContent content, HttpHeaders headers)
    {
        if (_contentReader != null)
            return _contentReader(content);
        if (content is StringContent || IsTextBasedContentType(content.Headers) || IsTextBasedContentType(headers))
            return content.ReadAsStringAsync();
        else
            return Task.FromResult("binary");
    }

    static bool IsTextBasedContentType(HttpHeaders headers)
    {
        if (headers.TryGetValues("Content-Type", out var contentTypeHeaders) == false)
            return false;

        foreach (var contentTypeHeader in contentTypeHeaders)
        foreach (var textContentType in _textContentTypes)
            if (contentTypeHeader.Contains(textContentType))
                return true;

        return false;
    }
}