using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.AdminUsers;

namespace AppSample.Domain.Services;

public class AdminUserService : IAdminUserService
{
    readonly IDbRepository _dbRepository;

    public AdminUserService(IDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<List<AdminUserEntity>> GetAllUsers()
    {
        var allAdminDtos = await _dbRepository.LoadAllAdminUsersAsync();
        var allAdmins = allAdminDtos.Select(x => new AdminUserEntity(x)).OrderBy(x => x.Login).ToList();
        return allAdmins;
    }

    public async Task Create(AdminUserEntity item)
    {
        item.CreatedAt=DateTimeOffset.UtcNow;
        item.UpdatedAt= DateTimeOffset.UtcNow;
        var entity = item.ToDto();
        await _dbRepository.CreateAdminUserAsync(entity);
    }

    public async Task<AdminUserEntity> GetById(int id)
    {
        var adminDto = await _dbRepository.LoadAdminUserByIdAsync(id);
        if (adminDto == null) return null;
        var item = new AdminUserEntity(adminDto);
        return item;
    }

    public async Task<AdminUserEntity> GetByLogin(string login)
    {
        var adminDto = await _dbRepository.LoadAdminUserByLoginAsync(login);
        if (adminDto == null) return null;
        var item = new AdminUserEntity(adminDto);
        return item;
    }

    public async Task Delete(AdminUserEntity item)
    {
        await _dbRepository.DeleteAdminUserAsync(item.Id);
    }
}