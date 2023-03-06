using AppSample.Admin.Helpers;
using AppSample.Admin.Models;
using AppSample.Admin.Models.AdminUsers;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models.AdminUsers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppSample.Admin.Controllers;

[RequiresAuthentication(Roles = new[] {AdminUserRole.Admin})]
public class UsersController : Controller
{
    readonly IAdminUserService _adminUserService;
    readonly AdminSettings _adminSettings;

    public UsersController(IAdminUserService adminUserService, IOptions<AdminSettings> adminSettings)
    {
        _adminUserService = adminUserService;
        _adminSettings = adminSettings.Value;
    }

    public async Task<IActionResult> Index()
    {
        List<AdminUserEntity> users = await _adminUserService.GetAllUsers();

        List<AdminUserViewModel> items = users
            .Select(x =>
            {
                var res = new AdminUserViewModel(x);

                return res;
            })
            .ToList();

        var model = new AdminUserListViewModel()
        {
            Items = items,
            ConfigUsers = _adminSettings.UsersAsList.OrderBy(x => x).ToList()
        };

        return View("List", model);
    }

    [HttpGet]
    public ActionResult Create()
    {
        var model = new AdminUserAddViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(AdminUserAddViewModel model)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                throw new Exception("Ошибка в введенных данных");
            }

            var item = model.ToItem();
            await _adminUserService.Create(item);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewBag.Error = e.Message;
            return View("Create", model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(string type, int id)
    {
        var item = await _adminUserService.GetById(id);
        if (item == null)
            return NotFound($"Пользователь с id={id} не найден");

        switch (type)
        {
            case ActionKey.Delete:
                await _adminUserService.Delete(item);

                break;
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Confirm(int id, string type)
    {
        var item = await _adminUserService.GetById(id);
        if (item == null)
            return NotFound($"Пользователь с id={id} не найден");

        switch (type)
        {
            case ActionKey.Delete:
                var model = new AdminUserAddViewModel(item);
                return View("Confirm/Delete", model);
        }

        return RedirectToAction("Index");
    }
}