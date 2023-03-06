using System.ComponentModel;

namespace AppSample.Domain.Models
{
    public enum JweAlgorithm
    {
        [Description("RSA1_5")]
        RSA1_5 = 1,
        [Description("RSA-OAEP")]
        RSA_OAEP = 2,
        [Description("RSA-OAEP-256")]
        RSA_OAEP_256 = 3
    }
}
