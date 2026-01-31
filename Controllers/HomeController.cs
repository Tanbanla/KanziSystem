using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;

namespace PRJ_WAREHOUSE_BIVN.Controllers;

public class HomeController : BaseAuthController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Có th? s? d?ng các method t? BaseAuthController
        ViewBag.CurrentUser = GetCurrentUserFullName();
        ViewBag.CurrentSection = GetCurrentUserSection();
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
