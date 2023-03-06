using System.Text.Json.Serialization;
using AppSample.Domain.Models;

namespace AppSample.Admin.Models.ServiceProviders
{
    public class ScopeViewModel : Scope
    {
        [JsonPropertyName("claims")]
        public List<ClaimViewModel> Claims { get; set; }

        public bool IsRequired { get; set; }
    }

    public class ClaimViewModel : Claim
    {
        public ClaimViewModel(string scopeName, string claimName, bool availableFlag, bool requiredFlag)
        {
            ClaimName = claimName;
            FullClaimName = scopeName + "_" + claimName;
            AvailableFlag = availableFlag;
            RequiredFlag = requiredFlag;
        }
        public ClaimViewModel(string scopeName, string claimName)
        {
            ClaimName = claimName;
            FullClaimName = scopeName + "_" + claimName;
        }

        public ClaimViewModel()
        {
            
        }
        [JsonPropertyName("claim_full_name")]
        public string FullClaimName { get; set; }
    }
}
