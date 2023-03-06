using Beeline.MobileID.CoreTools.Settings;

namespace Beeline.MobileID.RegionDb.Client.Settings;

public class  RegionDbSettings:BaseSettings,IRegionDbSettings
{
    public string Url { get; set; }
}