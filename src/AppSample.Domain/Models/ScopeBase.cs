using System.Text.Json.Serialization;

namespace AppSample.Domain.Models;

public class ScopeBase
{
    [JsonPropertyName("scope_name")]
    public string ScopeName { get; set; }

    [JsonPropertyName("claims")]
    public List<Claim> Claims { get; set; }

}