using Microsoft.AspNetCore.Mvc;
using FourthAssignment.Controllers;
using FourthAssignment.Models;
using FourthAssignment.Providers;

namespace FourthAssignment.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewWithWeekScheduleViewModel()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = new HomeController(new ShiftsProvider(db));

        var result = controller.Index() as ViewResult;

        Assert.NotNull(result);
        Assert.IsType<WeekScheduleViewModel>(result.Model);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = new HomeController(new ShiftsProvider(db));

        var result = controller.Privacy() as ViewResult;

        Assert.NotNull(result);
    }
}
