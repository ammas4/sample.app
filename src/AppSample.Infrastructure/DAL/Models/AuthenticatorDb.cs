using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Infrastructure.DAL.Models
{
    public struct AuthenticatorDb
    {
        public int Id { get; init; }
        public int ServiceProviderId { get; init; }
        public int OrderLevel1 { get; init; }
        public int OrderLevel2 { get; init; }
        public AuthenticatorType AuthenticatorType { get; init; }
        public long NextChainStartDelay { get; init; }

        public AuthenticatorDb()
        {
            AuthenticatorType = AuthenticatorType.NoValue;
            Id = 0;
            NextChainStartDelay = 0;
            ServiceProviderId = 0;
            OrderLevel1 = 0;
            OrderLevel2 = 0;
        }

        public AuthenticatorDb(AuthenticatorEntity authenticator)
        {
            Id = authenticator.Id;
            AuthenticatorType = authenticator.Type;
            OrderLevel1 = authenticator.OrderLevel1;
            OrderLevel2 = authenticator.OrderLevel2;
            NextChainStartDelay = authenticator.NextChainStartDelay.Ticks;
            ServiceProviderId = authenticator.ServiceProviderId;
        }
    }
}