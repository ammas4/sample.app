namespace AppSample.CoreTools.Settings;

public class BaseSettings
{
    public virtual string SectionName => GetType().Name;
}