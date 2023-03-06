using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

namespace AppSample.CoreTools.Logging;

public static class LogRequestHelper
{
    /// <summary>
    /// Body как строка
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<string> PrepareRequestBody(HttpRequest request)
    {
        var bodyAsText = string.Empty;

        request.EnableBuffering();
        if (request.ContentLength > 0)
        {
            using var reader = new StreamReader(request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: Convert.ToInt32(request.ContentLength),
                leaveOpen: true
            );
            bodyAsText = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        if (IsMultipartContentType(request.ContentType))
        {
            _ = MediaTypeHeaderValue.TryParse(request.ContentType, out var h);
            if (h != null)
            {
                try
                {
                    var boundary = GetBoundary(h, int.MaxValue);
                    bodyAsText = Regex.Replace(bodyAsText, @$"{boundary}.*", $"{boundary} replaced by logger",
                        RegexOptions.Multiline | RegexOptions.Singleline);
                }
                catch
                {
                }
            }
        }

        return bodyAsText;
    }

    /// <summary>
    /// Проверка Body на multipart/ content-type
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    static bool IsMultipartContentType(string contentType)
    {
        var result = !string.IsNullOrEmpty(contentType) &&
                     contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    /// The spec says 70 characters is a reasonable limit.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="lengthLimit"></param>
    /// <returns></returns>
    static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
        if (string.IsNullOrWhiteSpace(boundary.Value))
            throw new InvalidDataException("Missing content-type boundary.");

        if (boundary.Length > lengthLimit)
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");

        return boundary.Value;
    }

    public static Dictionary<string, List<string>> TransformToDictionary(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, List<string>>();
        foreach (var header in headers)
        {
            if (result.TryGetValue(header.Key, out var list) == false)
            {
                list = new List<string>();
                result.Add(header.Key, list);
            }

            list.Add(header.Value);
        }

        return result;
    }

    public static Dictionary<string, List<string>> TransformToDictionary(HttpHeaders headers)
    {
        var result = new Dictionary<string, List<string>>();
        foreach (var header in headers)
        {
            if (result.ContainsKey(header.Key))
            {
                result[header.Key].AddRange(header.Value);
                continue;
            }

            result.Add(header.Key, header.Value.ToList());
        }

        return result;
    }

}