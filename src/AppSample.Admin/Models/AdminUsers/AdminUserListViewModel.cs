namespace AppSample.Admin.Models.AdminUsers;

public class AdminUserListViewModel : ListViewModel<AdminUserViewModel>
{
    public List<string> ConfigUsers { get; set; }
}