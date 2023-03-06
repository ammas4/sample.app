using System.Collections.Specialized;
using System.Web;

namespace AppSample.CoreTools.Helpers;

public class UrlBuilder
{
    readonly UriBuilder _uriBuilder;

    public NameValueCollection Query { get; init; }

    public string? Scheme
    {
        get => _uriBuilder.Scheme;
        set => _uriBuilder.Scheme = value;
    }

    public string? UserName
    {
        get => _uriBuilder.UserName;
        set => _uriBuilder.UserName = value;
    }

    public string? Password
    {
        get => _uriBuilder.Password;
        set => _uriBuilder.Password = value;
    }

    public string? Host
    {
        get => _uriBuilder.Host;
        set => _uriBuilder.Host = value;
    }

    public int Port
    {
        get => _uriBuilder.Port;
        set => _uriBuilder.Port = value;
    }

    public string? Path
    {
        get => _uriBuilder.Path;
        set => _uriBuilder.Path = value;
    }

    public string? Fragment
    {
        get => _uriBuilder.Fragment;
        set => _uriBuilder.Fragment = value;
    }

    public UrlBuilder(string url)
    {
        if (url is null)
            throw new ArgumentNullException(nameof(url));

        _uriBuilder = new UriBuilder(url);
        Query = HttpUtility.ParseQueryString(_uriBuilder.Query);
    }

    public UrlBuilder(Uri uri)
    {
        _uriBuilder = new UriBuilder(uri);
        Query = HttpUtility.ParseQueryString(_uriBuilder.Query);
    }

    public override string ToString()
    {
        var query = Query.ToString();
        _uriBuilder.Query = query;
        var url = _uriBuilder.ToString();
        return url;
    }
}