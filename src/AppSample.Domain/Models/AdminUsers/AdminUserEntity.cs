using AppSample.Domain.DAL.Entities;

namespace AppSample.Domain.Models.AdminUsers;

public class AdminUserEntity
{
    public int Id;
    public string Login;
    public AdminUserRole Role;
    public bool Active;
    public DateTimeOffset CreatedAt;
    public DateTimeOffset UpdatedAt;

    public AdminUserEntity()
    {
    }

    public AdminUserEntity(AdminUserDTO adminDto)
    {
        Id = adminDto.Id;
        Login = adminDto.Login;
        Role = adminDto.Role;
        Active = adminDto.Active;
        CreatedAt = adminDto.CreatedAt;
        UpdatedAt = adminDto.UpdatedAt;
    }

    public AdminUserDTO ToDto()
    {
        return new AdminUserDTO()
        {
            Id = Id,
            Login = Login,
            Role = Role,
            Active = Active,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
        };
    }
}