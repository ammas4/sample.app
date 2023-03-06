using System.ComponentModel;

namespace AppSample.Domain.Models
{
    public enum JweMethodEncryption
    {
        [Description("A128CBC-HS256")]
        A128CBC_HS256 = 1,
        [Description("A192CBC-HS384")]
        A192CBC_HS384 = 2,
        [Description("A256CBC-HS512")]
        A256CBC_HS512 = 3,
        [Description("A128GCM")]
        A128GCM = 4,
        [Description("A192GCM")]
        A192GCM = 5,
        [Description("A256GCM")]
        A256GCM = 6,
    }
}
