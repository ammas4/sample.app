using AppSample.Admin.Models;
using AppSample.Admin.Models.ServiceProviders;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppSample.Admin.Controllers;

public class ServiceProvidersController : Controller
{
    readonly IServiceProviderService _serviceProviderService;
    readonly IdgwSettings _settings;

    public ServiceProvidersController(IServiceProviderService serviceProviderService, IOptions<IdgwSettings> settings)
    {
        _serviceProviderService = serviceProviderService;
        _settings = settings.Value;
    }

    // GET: Operators
    public async Task<IActionResult> Index(ServiceProviderListViewModel model)
    {
        model.Initialize(await _serviceProviderService.GetAllAsync());

        return View("List", model);
    }

    public async Task<IActionResult> Save(ServiceProviderAddEditViewModel model)
    {
        if (model.Id == default(int))
            throw new InvalidDataException("Неверные данные");

        if (ModelState.IsValid)
        {
            var operatorEntity = model.ToEntity();

            await _serviceProviderService.UpdateAsync(operatorEntity);

            return RedirectToAction("Detail", new { id = model.Id });
        }
        else
        {
            ViewBag.Error = "Ошибка в введенных данных";
            return View("Edit", model);
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await _serviceProviderService.GetByIdAsync(id);
        if (item == null || item.Deleted)
            return NotFound("Сервис-провайдер не найден");

        return View("Details", new ServiceProviderViewModel(item));
    }

    [HttpGet]
    public ActionResult Create()
    {
        var model = new ServiceProviderAddEditViewModel();
        model.Active = true;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceProviderAddEditViewModel model)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                throw new Exception("Ошибка в введенных данных");
            }

            var item = model.ToEntity();

            await _serviceProviderService.CreateAsync(item);

            return RedirectToAction("Index", new { id = item.Id, status = "ok" });
        }
        catch (Exception e)
        {
            ViewBag.Error = e.Message;

            return View("Create", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int tab = 1, string status = "")
    {
        var item = await _serviceProviderService.GetByIdAsync(id);
        var model = new ServiceProviderAddEditViewModel(item)
        {
            SelectedTab = tab is >= 1 and <= 7 ? tab : 1
        };

        ViewBag.IsSuccess = status == "ok";

        return View("Edit", model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceProviderAddEditViewModel model)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                throw new Exception("Ошибка в введенных данных");
            }

            var item = model.ToItem();

            await _serviceProviderService.UpdateAsync(item);

            return RedirectToAction("Edit", new { id, tab = model.SelectedTab, status = "ok" });
        }
        catch (Exception e)
        {
            ViewBag.Error = e.Message;

            return View("Edit", model);
        }
    }

    public async Task<ActionResult> Confirm(int id, string type)
    {
        var item = await _serviceProviderService.GetByIdAsync(id);
        if (item == null || item.Deleted)
            return NotFound("Сервис-провайдер не найден");

        var model = new ServiceProviderViewModel(item);

        switch (type)
        {
            case ActionKey.Delete:
                return View("Confirm/Delete", model);
            case ActionKey.Clone:
                return View("Confirm/Clone", model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Action(string type, int id)
    {
        var item = await _serviceProviderService.GetByIdAsync(id);
        if (item == null || item.Deleted)
            return NotFound($"Оператор с id={id} не найден");

        switch (type)
        {
            case ActionKey.Delete:
                await _serviceProviderService.DeleteAsync(item);
                break;
            case ActionKey.Clone:
                await _serviceProviderService.CloneAsync(item);
                break;
        }

        return RedirectToAction("Index");
    }

    public IActionResult AuthenticatorEntryRow()
    {
        return PartialView("AuthenticatorEdit", new AuthenticatorViewModel() { NextChainStartDelay = TimeSpan.FromSeconds(_settings.DefaultNextChainStartDelaySeconds) });
    }
}