using System.ComponentModel;

namespace AppSample.Domain.Models.AdminUsers;

public enum AdminUserRole
{
    [Description("Пользователь")]
    User = 1,
    [Description("Администратор")]
    Admin = 2
}