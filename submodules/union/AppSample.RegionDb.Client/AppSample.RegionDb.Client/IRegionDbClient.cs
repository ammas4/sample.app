using Beeline.MobileID.RegionDb.Client.Connected_Services.RegionDb;

namespace Beeline.MobileID.RegionDb.Client;

public interface IRegionDbClient
{
    Task<RegionOperatorInfo?> GetInfoByMsisdnAsync(long msisdn);
}