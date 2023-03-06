using Beeline.MobileID.RegionDb.Client.Connected_Services.RegionDb;

namespace Beeline.MobileID.RegionDb.Client;

public class FakeRegionDbClient : IRegionDbClient
{
    public async Task<RegionOperatorInfo?> GetInfoByMsisdnAsync(long msisdn)
    {
        if (msisdn < 70_000_000_000 || msisdn > 79_999_999_999)
            throw new ArgumentOutOfRangeException(nameof(msisdn));

        long phoneNumber = msisdn - 70_000_000_000; //номер в формате 10 цифр

        return new RegionOperatorInfo
        {
            FederalOperatorCode = "99",
            FederalOperatorName = "Beeline",
            OperatorName = "ВымпелКом ПАО",
            PhoneNumber = phoneNumber,
            FederalRegionCode = "77",
            FederalRegionName = "г. Москва, Московская область",
            RegionName = null
        };
    }
}