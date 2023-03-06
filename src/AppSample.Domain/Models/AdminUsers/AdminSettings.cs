using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models.AdminUsers;

public class AdminSettings : BaseSettings
{
    public string Users { get; set; }

    public List<string> UsersAsList => (Users ?? "").Split(',', ';').Where(x => string.IsNullOrEmpty(x) == false).Distinct().ToList();
}