using System.Text.Json.Serialization;

namespace AppSample.Domain.Models;

public class Claim
{
    public Claim()
    {
    }

    public Claim(string claimName)
    {
        ClaimName = claimName;
    }

    public Claim(string claimName, bool availableFlag, bool requiredFlag)
    {
        ClaimName = claimName;
        AvailableFlag = availableFlag;
        RequiredFlag = requiredFlag;
    }

    [JsonPropertyName("claim_name")]
    public string ClaimName { get; set; }

    [JsonPropertyName("available_flag")]
    public bool AvailableFlag { get; set; }

    [JsonPropertyName("required_flag")]
    public bool RequiredFlag { get; set; }

    //todo удалить когда обновятся все скоупы СП в бд - MOBID-5369
    [JsonPropertyName("ClaimName")]
    public string ObsoleteClaimName { set => ClaimName = value; }
    [JsonPropertyName("AvailableFlag")]
    public bool ObsoleteAvailableFlag { set => AvailableFlag = value; }
    [JsonPropertyName("RequiredFlag")]
    public bool ObsoleteRequiredFlag { set => RequiredFlag = value; }
}