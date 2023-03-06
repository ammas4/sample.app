using System.Text.Json.Serialization;

namespace AppSample.Domain.Models
{
    public class ResourceServerAuthPostRequest
    {
        [JsonPropertyName("token")]
        public ResourceServerAuthToken Token { get; set; }

        [JsonPropertyName("device_msisdn")]
        public string Msisdn { get; set; }

        [JsonPropertyName("scopes")]
        public ScopeBase[] Scopes { get; set; }

        [JsonPropertyName("jwks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Jwks { get; set; }

        [JsonPropertyName("jwks_uri")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JwksUrl { get; set; }

        [JsonPropertyName("enc_alg")]
        public string EncrAlg { get; set; }

        [JsonPropertyName("enc_method")]
        public string EncrMethod { get; set; }

        [JsonPropertyName("doc_types")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<byte>? DocTypes { get; set; }

        [JsonPropertyName("client_ctn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ClientCtn { get; set; }
    }

}
