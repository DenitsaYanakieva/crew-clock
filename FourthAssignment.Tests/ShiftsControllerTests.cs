using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FourthAssignment.Controllers;
using FourthAssignment.Models;
using FourthAssignment.Providers;
using FourthAssignment.Validators;

namespace FourthAssignment.Tests;

public class ShiftsControllerTests
{
    private static readonly DateTime TestDate = new(2025, 6, 2);

    private static ShiftsController CreateController(Data.AppDbContext db)
    {
        var provider = new ShiftsProvider(db);
        return new ShiftsController(provider, new ShiftValidator(provider));
    }

    // ── CREATE GET ──────────────────────────────────────────

    [Fact]
    public void CreateGet_ReturnsViewWithFormModel()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var emp = db.Employees.First();

        var result = controller.Create(emp.Id, TestDate.ToString("yyyy-MM-dd")) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<ShiftFormViewModel>(result.Model);
        Assert.Equal(emp.Id, model.EmployeeId);
        Assert.Equal(emp.Name, model.EmployeeName);
        Assert.Equal(TestDate, model.Date);
    }

    [Fact]
    public void CreateGet_PopulatesAvailableRoles()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var john = db.Employees.First(e => e.Name == "John Smith");

        var result = controller.Create(john.Id, TestDate.ToString("yyyy-MM-dd")) as ViewResult;
        var model = result!.Model as ShiftFormViewModel;

        Assert.NotEmpty(model!.AvailableRoles);
    }

    [Fact]
    public void CreateGet_InvalidEmployee_ReturnsNotFound()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);

        Assert.IsType<NotFoundResult>(controller.Create(9999, TestDate.ToString("yyyy-MM-dd")));
    }

    // ── CREATE POST ─────────────────────────────────────────

    [Fact]
    public void CreatePost_ValidShift_RedirectsToIndex()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var john = db.Employees.Include(e => e.Roles).First(e => e.Name == "John Smith");
        var waiter = john.Roles.First(r => r.Name == "Waiter");

        var result = controller.Create(new ShiftFormViewModel
        {
            EmployeeId = john.Id, Date = TestDate,
            RoleId = waiter.Id, StartTime = "10:00", EndTime = "14:00"
        }) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Home", result.ControllerName);
    }

    [Fact]
    public void CreatePost_ValidationFailure_ReturnsView()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var john = db.Employees.Include(e => e.Roles).First(e => e.Name == "John Smith");
        var waiter = john.Roles.First(r => r.Name == "Waiter");

        var result = controller.Create(new ShiftFormViewModel
        {
            EmployeeId = john.Id, Date = TestDate,
            RoleId = waiter.Id, StartTime = "16:00", EndTime = "10:00"
        }) as ViewResult;

        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public void CreatePost_ValidationFailure_RepopulatesRoles()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var john = db.Employees.Include(e => e.Roles).First(e => e.Name == "John Smith");
        var waiter = john.Roles.First(r => r.Name == "Waiter");

        var result = controller.Create(new ShiftFormViewModel
        {
            EmployeeId = john.Id, Date = TestDate,
            RoleId = waiter.Id, StartTime = "16:00", EndTime = "10:00"
        }) as ViewResult;

        var model = result!.Model as ShiftFormViewModel;
        Assert.NotEmpty(model!.AvailableRoles);
    }

    [Fact]
    public void CreatePost_ValidShift_PersistsToDb()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var john = db.Employees.Include(e => e.Roles).First(e => e.Name == "John Smith");
        var waiter = john.Roles.First(r => r.Name == "Waiter");

        controller.Create(new ShiftFormViewModel
        {
            EmployeeId = john.Id, Date = TestDate,
            RoleId = waiter.Id, StartTime = "09:30", EndTime = "15:45"
        });

        Assert.Single(db.Shifts);
    }

    // ── EDIT GET ─────────────────────────────────────────────

    [Fact]
    public void EditGet_ExistingShift_ReturnsViewWithModel()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var controller = CreateController(db);

        var result = controller.Edit(shiftId) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<ShiftFormViewModel>(result.Model);
        Assert.Equal(shiftId, model.ShiftId);
        Assert.Equal("10:00", model.StartTime);
        Assert.Equal("14:00", model.EndTime);
        db.Dispose();
    }

    [Fact]
    public void EditGet_NonExistentShift_ReturnsNotFound()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);

        Assert.IsType<NotFoundResult>(controller.Edit(9999));
    }

    // ── EDIT POST ────────────────────────────────────────────

    [Fact]
    public void EditPost_ValidUpdate_RedirectsToIndex()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var controller = CreateController(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var barman = db.Roles.First(r => r.Name == "Barman");

        var result = controller.Edit(new ShiftFormViewModel
        {
            ShiftId = shiftId, EmployeeId = john.Id, Date = TestDate,
            RoleId = barman.Id, StartTime = "11:00", EndTime = "15:00"
        }) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        db.Dispose();
    }

    [Fact]
    public void EditPost_ValidationFailure_ReturnsView()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var controller = CreateController(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var waiter = db.Roles.First(r => r.Name == "Waiter");

        var result = controller.Edit(new ShiftFormViewModel
        {
            ShiftId = shiftId, EmployeeId = john.Id, Date = TestDate,
            RoleId = waiter.Id, StartTime = "18:00", EndTime = "10:00"
        }) as ViewResult;

        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
        db.Dispose();
    }

    [Fact]
    public void EditPost_NonExistentShift_ReturnsNotFound()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);
        var john = db.Employees.Include(e => e.Roles).First(e => e.Name == "John Smith");
        var waiter = john.Roles.First(r => r.Name == "Waiter");

        var result = controller.Edit(new ShiftFormViewModel
        {
            ShiftId = 9999, EmployeeId = john.Id, Date = TestDate,
            RoleId = waiter.Id, StartTime = "10:00", EndTime = "14:00"
        });

        Assert.IsType<NotFoundResult>(result);
    }

    // ── DELETE ────────────────────────────────────────────────

    [Fact]
    public void Delete_RedirectsToIndex()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var controller = CreateController(db);

        var result = controller.Delete(shiftId) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Home", result.ControllerName);
        db.Dispose();
    }

    [Fact]
    public void Delete_NonExistent_StillRedirects()
    {
        using var db = TestHelper.CreateSeededDb();
        var controller = CreateController(db);

        var result = controller.Delete(9999) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
    }
}
