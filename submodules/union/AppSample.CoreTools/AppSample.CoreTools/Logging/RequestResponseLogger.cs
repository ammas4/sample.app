using System.Net;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace AppSample.CoreTools.Logging;

/// <summary>
/// Логгер запроса-ответа (request-response)
/// </summary>
public class RequestResponseLogger : IRequestResponseLogger
{
    static Logger _logger = LogManager.GetCurrentClassLogger();

    readonly ILogRequestResponseSettings _settings;
    readonly IHttpLogDataFinder _httpLogDataFinder;

    readonly MaskingStrategiesForSpan.Mask _maskStrategy;
    readonly MaskingStrategiesForBuilder.Mask _maskStrategyForBuilder;

    public RequestResponseLogger(ILogRequestResponseSettings settings, IHttpLogDataFinder httpLogDataFinder)
    {
        _settings = settings;
        _httpLogDataFinder = httpLogDataFinder;

        _maskStrategy =
            MaskingStrategiesForSpan.Strategy(_settings.MaskingType, _settings.PercentageOfMasking);
        _maskStrategyForBuilder = MaskingStrategiesForBuilder.Strategy(_settings.MaskingType, _settings.PercentageOfMasking);
    }

    #region Private

    static bool IsSuccessStatusCode(HttpStatusCode statusCode)
    {
        return ((int)statusCode >= 200) && ((int)statusCode <= 299)
               || statusCode == HttpStatusCode.Moved //301
               || statusCode == HttpStatusCode.Redirect //302
               || statusCode == HttpStatusCode.RedirectMethod; //303
    }

    void AddIndexScopeProperties<T>(IEnumerable<KeyValuePair<string, T>>? headers)
    {
        if (_settings.IndexedScopeHeaders != null)
        {
            foreach (var indexedScopeSetting in _settings.IndexedScopeHeaders)
            {
                if (!string.IsNullOrEmpty(indexedScopeSetting) && headers != null &&
                    headers.Any(d => d.Key == indexedScopeSetting))
                {
                    _logger = _logger.WithProperty(indexedScopeSetting,
                        headers.FirstOrDefault(d => d.Key == indexedScopeSetting).Value);
                }
            }
        }
    }

    string MaskJsonIEnumerable<T>(IEnumerable<T>? source, MaskingStrategiesForSpan.Mask mask, string[] maskValues)
    {
        if (source != null && source.Any())
        {
            return MaskJson(JsonConvert.SerializeObject(source), mask, maskValues);
        }

        return string.Empty;
    }

    string MaskJson(string source, MaskingStrategiesForSpan.Mask mask, string[] maskValues)
    {
        return string.IsNullOrEmpty(source) ? string.Empty : JsonMasker.MaskByPropertyName(source, mask, maskValues);
    }

    string MaskQueryString(string source, MaskingStrategiesForSpan.Mask mask, string[] maskValues)
    {
        return string.IsNullOrEmpty(source) ? source : QueryStringMasker.Mask(source, _settings.MaskValues, _maskStrategy);
    }

    void MaskFields(IDictionary<object, object> fields, MaskingStrategiesForSpan.Mask mask, string[] maskValues)
    {
        var maskFieldKeys = fields.Keys.Where(x => (x is string keyString) && (maskValues?.Contains(keyString) ?? false));
        foreach (var key in maskFieldKeys)
        {
            if (fields[key] is string valueString)
            {
                var chars = valueString.ToCharArray();
                mask(chars);
                fields[key] = new string(chars);
            }
        }
    }

    void InternalLogRequestResponse(LogItem item)
    {
        bool wasError = item.WasError ?? (item.ResponseStatusCode != default && IsSuccessStatusCode(item.ResponseStatusCode) == false);
        var logEventInfo = new LogEventInfo(wasError ? LogLevel.Error : LogLevel.Info, _logger.Name, string.Empty);

        if (item.ContextAdditionalData == null)
        {
            item.ContextAdditionalData = _httpLogDataFinder.GetLogDataFromContext();
        }

        foreach (var pair in item.ContextAdditionalData)
        {
            logEventInfo.Properties[pair.Key] = pair.Value;
        }

        logEventInfo.Properties[nameof(item.Api)] = item.Api;
        logEventInfo.Properties[nameof(item.RequestBody)] = item.RequestBody;
        logEventInfo.Properties[nameof(item.RequestHeaders)] = GetHeadersAsString(item.RequestHeaders, _maskStrategyForBuilder, _settings.MaskValues);
        logEventInfo.Properties[nameof(item.RequestMethod)] = item.RequestMethod;
        logEventInfo.Properties[nameof(item.RequestUrl)] = MaskQueryString(item.RequestUrl, _maskStrategy, _settings.MaskValues);
        logEventInfo.Properties[nameof(item.ResponseBody)] = item.ResponseBody;
        logEventInfo.Properties[nameof(item.ResponseHeaders)] = GetHeadersAsString(item.ResponseHeaders, _maskStrategyForBuilder, _settings.MaskValues);
        logEventInfo.Properties[nameof(item.ResponseStatusCode)] = ((int) item.ResponseStatusCode).ToString();
        logEventInfo.Properties[nameof(item.Time)] = item.Time;
        logEventInfo.Properties[nameof(item.Error)] = item.Error;
        logEventInfo.Properties[nameof(item.ErrorCode)] = item.ErrorCode;
        logEventInfo.Properties[nameof(item.ErrorMessage)] = item.ErrorMessage;

        MaskFields(logEventInfo.Properties, _maskStrategy, _settings.MaskValues);

        AddIndexScopeProperties(item.RequestHeaders);

        _logger.Log(logEventInfo);
    }

    #endregion

    public void Log(LogItem item)
    {
        try
        {
            InternalLogRequestResponse(item);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Logging error");
        }
    }

    string? GetHeadersAsString(Dictionary<string, List<string>>? headers, MaskingStrategiesForBuilder.Mask maskStrategy, string[]? maskValues)
    {
        if (headers == null) return null;
        var stringBuilder = new StringBuilder();
        foreach (KeyValuePair<string, List<string>> keyValuePair in headers)
        {
            if (stringBuilder.Length > 0) stringBuilder.Append("\r\n");
            stringBuilder.Append(keyValuePair.Key);
            stringBuilder.Append(": ");
            var isSensitive = maskValues?.Contains(keyValuePair.Key) ?? false;
            var headerValues = keyValuePair.Value;
            for (int i = 0; i < headerValues.Count; i++)
            {
                if (i > 0)
                    stringBuilder.Append(", ");
                var value = headerValues[i];
                stringBuilder.Append(value);
                if (isSensitive)
                    maskStrategy(stringBuilder, stringBuilder.Length - value.Length, stringBuilder.Length - 1);
            }
        }

        return stringBuilder.ToString();
    }
}