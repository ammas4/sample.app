using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models
{
    public class ResourceServerSettings : BaseSettings
    {
        public string BaseHostAddress { get; set; }

        public string WarmingInfoEndpoint { get; set; }

        public string AuthInfoEndpoint { get; set; }

        public Credentials DefaultCredentials { get; set; }
    }

    public class Credentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
