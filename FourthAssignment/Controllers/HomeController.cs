using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FourthAssignment.Models;
using FourthAssignment.Providers;

namespace FourthAssignment.Controllers;

public class HomeController(IShiftsProvider provider) : Controller
{
    public IActionResult Index() => View(provider.GetWeekSchedule());

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
