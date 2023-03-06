using AppSample.Admin.Models.Statistics;
using AppSample.Domain.DAL;
using AppSample.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Admin.Controllers;

public class StatisticsController : Controller
{
    readonly IDbRepository _dbRepository;
    readonly IServiceProviderService _serviceProviderService;

    public StatisticsController(IDbRepository dbRepository, IServiceProviderService serviceProviderService)
    {
        _dbRepository = dbRepository;
        _serviceProviderService = serviceProviderService;
    }

    [HttpGet]
    public ActionResult Index()
    {
        StatisticsStatViewModel model = new StatisticsStatViewModel();
        _setAddEditValues(model);
        var date = DateTime.Now.Date;
        model.BeginDate = date;
        model.EndDate = date.Add(new TimeSpan(23, 59, 0));
        return View(model);
    }

    void _setAddEditValues(StatisticsStatViewModel model)
    {
    }
}