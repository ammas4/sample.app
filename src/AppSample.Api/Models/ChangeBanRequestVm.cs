using System.Text.Json.Serialization;

namespace AppSample.Api.Models
{
    public class ChangeBanRequestVm
    {
        [JsonPropertyName("msisdn")]
        public string? Msisdn { get; set; }
    }
}
