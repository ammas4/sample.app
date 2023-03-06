using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models
{
    public class CacheSettings : BaseSettings
    {
        public TimeSpan OidcCacheTime { get; set; }
        public TimeSpan JwksCacheTime { get; set; }
        public TimeSpan ServiceProdiverJwksCacheTime { get; set; }
    }
}
