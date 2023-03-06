using AppSample.Domain.Models.AdminUsers;

namespace AppSample.Admin.Models.AdminUsers;

public class AdminUserViewModel
{
    public int Id { get; set; }
    public string Login { get; set; }
    public AdminUserRole Role;
        


    public AdminUserViewModel(AdminUserEntity item)
    {
        if (item == null)
            return;

        Id = item.Id;
        Login = item.Login;
        Role = item.Role;
    }
}