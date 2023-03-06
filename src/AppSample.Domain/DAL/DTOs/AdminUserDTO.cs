using AppSample.CoreTools.DapperContrib;
using AppSample.Domain.Models.AdminUsers;

namespace AppSample.Domain.DAL.Entities;

public class AdminUserDTO
{
    public int Id { get; init; }
    public string Login { get; init; }
    public AdminUserRole Role { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public bool Active { get; init; }
}