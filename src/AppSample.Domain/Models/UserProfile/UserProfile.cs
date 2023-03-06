namespace AppSample.Domain.Models.UserProfile;

public class UserProfile
{
    public AppletType[]? Applets { get; init; }
    public bool IsBlocked { get; init; }
    public bool IsPinActive { get; init; }
    public int? Pin { get; init; }

    public bool HasApplet(AppletType applet) => Applets?.Any(a => a == applet) ?? false;
    public bool HasMcApplet => HasApplet(AppletType.Mc10mc);

    public bool HasDstkApplet => HasApplet(AppletType.Dstk10fi) ||
                                 HasApplet(AppletType.Dstk21fio) ||
                                 HasApplet(AppletType.Dstk22fio) ||
                                 HasApplet(AppletType.Dstk311fio);
}