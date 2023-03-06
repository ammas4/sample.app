using System.ComponentModel.DataAnnotations;
using AppSample.Admin.Helpers;
using AppSample.Domain.Models.AdminUsers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppSample.Admin.Models.AdminUsers;

public class AdminUserAddViewModel
{
    public int? Id { get; set; }

    [Display(Name = @"Логин пользователя (формат 'domain\username')")]
    [Required(ErrorMessage = @"Необходимо заполнить поле 'Логин пользователя'")]
    [MaxLength(250, ErrorMessage = "Длина поля не более 250 символов")]
    public string Login { get; set; }

    [Display(Name = "Роль")]
    [Required(ErrorMessage = "Необходимо указать роль")]
    public AdminUserRole Role { get; set; }

    /*
    [Display(Name = "Состояние")]
    [Required(ErrorMessage = "Необходимо указать состояние")]
    public AdminUserState State { get; set; }
    */

    public IEnumerable<SelectListItem> RolesSelectList => SelectListItemsHelper.Build(AdminUserRole.User, AdminUserRole.Admin);

    public AdminUserAddViewModel()
    {
    }

    public AdminUserAddViewModel(AdminUserEntity item)
    {
        if (item == null)
            return;

        Id = item.Id;
        //State = item.State;
        Login = item.Login;
        Role = item.Role;
    }

    public AdminUserEntity ToItem()
    {
        var item = new AdminUserEntity
        {
            Id = Id ?? 0,
            Login = Login,
            //State = State,
            Role = Role,
        };

        return item;
    }
}