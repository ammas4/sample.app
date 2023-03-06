namespace AppSample.Domain.Models.ServiceProviders
{
    public class AuthenticatorEntity
    {
        public int Id { get; set; }
        public int ServiceProviderId { get; set; }
        public AuthenticatorType Type { get; set; }
        public int OrderLevel1 { get; set; }
        public int OrderLevel2 { get; set; }
        public TimeSpan NextChainStartDelay { get; set; }
    }
}