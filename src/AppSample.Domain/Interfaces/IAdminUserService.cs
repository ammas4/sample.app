using AppSample.Domain.Models.AdminUsers;

namespace AppSample.Domain.Interfaces;

public interface IAdminUserService
{
    Task<List<AdminUserEntity>> GetAllUsers();
    Task Create(AdminUserEntity item);
    Task<AdminUserEntity> GetById(int id);
    Task Delete(AdminUserEntity item);
    Task<AdminUserEntity> GetByLogin(string login);
}