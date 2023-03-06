namespace Beeline.MobileID.RegionDb.Client.Connected_Services.RegionDb;

public class RegionOperatorInfo
{
    public long PhoneNumber { get; set; }
    public string? OperatorName { get; set; }
    public string? FederalOperatorName { get; set; }
    public string? FederalOperatorCode { get; set; }
    public string? FederalRegionCode { get; set; }
    public string? FederalRegionName { get; set; }
    public string? RegionName { get; set; }

    public bool IsBeelineOperatorCode()
    {
        return FederalOperatorCode == "99";
    }
}