using System.Text.Json.Serialization;

namespace AppSample.Domain.Models
{
    public class Scope : ScopeBase
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("is_loa2_enabled")]
        public bool IsLoa2Enabled { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("is_loa3_enabled")]
        public bool IsLoa3Enabled { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("is_loa4_enabled")]
        public bool IsLoa4Enabled { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        //todo удалить когда обновятся все скоупы СП в бд - MOBID-5369
        [JsonPropertyName("IsEnabled")]
        public bool ObsoleteIsEnabled { set => IsEnabled = value; }
        [JsonPropertyName("IsLoa2Enabled")]
        public bool ObsoleteIsLoa2Enabled { set => IsLoa2Enabled = value; }
        [JsonPropertyName("IsLoa3Enabled")]
        public bool ObsoleteIsLoa3Enabled { set => IsLoa3Enabled = value; }
        [JsonPropertyName("IsLoa4Enabled")]
        public bool ObsoleteIsLoa4Enabled { set => IsLoa4Enabled = value; }
        [JsonPropertyName("ScopeName")]
        public string ObsoleteScopeName { set => ScopeName = value; }
    }
}
