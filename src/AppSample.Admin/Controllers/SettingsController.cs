using AppSample.Admin.Models.Settings;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Admin.Controllers;

public class SettingsController : Controller
{
    readonly ICachedConfigService _cachedConfigService;
    readonly IDbRepository _dbRepository;
    
    public SettingsController(ICachedConfigService cachedConfigService, IDbRepository dbRepository)
    {
        _cachedConfigService = cachedConfigService;
        _dbRepository = dbRepository;
    }
        
    public IActionResult Index()
    {
        var state = _cachedConfigService.GetState();
        
        var vm = new SettingsEditViewModel
        {
            SettingsItems = state.Settings.Select(s => new SettingsEditItemViewModel
            {
                Name = s.Key,
                Value = s.Value
            }).ToList()
        };
        return View("Index", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SettingsEditViewModel model)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                throw new Exception("Ошибка в введенных данных");
            }

            foreach (var item in model.SettingsItems)
            {
                await _dbRepository.SaveSettingsAsync(item.Name, item.Value);
            }
            
            _cachedConfigService.SignalChange();
            await Task.Delay(2000);

            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            ViewBag.Error = e.Message;

            return Index();
        }
    }
}