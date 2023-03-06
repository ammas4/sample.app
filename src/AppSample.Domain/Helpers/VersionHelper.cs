namespace AppSample.Domain.Helpers;

public static class VersionHelper
{
    static readonly string[] SupportedSIVersions = new[] { "mc_si_r2_v1.0" };

    public static bool IsSupportedSIVersion(string version) => SupportedSIVersions.Contains(version);
}