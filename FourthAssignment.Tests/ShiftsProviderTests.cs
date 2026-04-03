using Microsoft.EntityFrameworkCore;
using FourthAssignment.Models;
using FourthAssignment.Providers;

namespace FourthAssignment.Tests;

public class ShiftsProviderTests
{
    private static readonly DateTime TestDate = new(2025, 6, 2);

    // ── GetEmployee ──────────────────────────────────────────

    [Fact]
    public void GetEmployee_ExistingId_ReturnsEmployee()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);
        var emp = db.Employees.First();

        var result = provider.GetEmployee(emp.Id);

        Assert.NotNull(result);
        Assert.Equal(emp.Name, result.Name);
    }

    [Fact]
    public void GetEmployee_InvalidId_ReturnsNull()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        Assert.Null(provider.GetEmployee(9999));
    }

    // ── GetEmployeeRoles ─────────────────────────────────────

    [Fact]
    public void GetEmployeeRoles_ReturnsCorrectRoles()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);
        var john = db.Employees.First(e => e.Name == "John Smith");

        var roles = provider.GetEmployeeRoles(john.Id).Select(r => r.Name).OrderBy(n => n).ToList();

        Assert.Equal(new[] { "Barman", "Waiter" }, roles);
    }

    [Fact]
    public void GetEmployeeRoles_InvalidEmployee_ReturnsEmpty()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        Assert.Empty(provider.GetEmployeeRoles(9999));
    }

    // ── GetShift / GetShiftWithDetails ────────────────────────

    [Fact]
    public void GetShift_ExistingId_ReturnsShift()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);

        Assert.NotNull(provider.GetShift(shiftId));
        db.Dispose();
    }

    [Fact]
    public void GetShift_InvalidId_ReturnsNull()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        Assert.Null(provider.GetShift(9999));
    }

    [Fact]
    public void GetShiftWithDetails_IncludesEmployeeAndRole()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);

        var shift = provider.GetShiftWithDetails(shiftId);

        Assert.NotNull(shift);
        Assert.Equal("John Smith", shift.Employee.Name);
        Assert.Equal("Waiter", shift.Role.Name);
        db.Dispose();
    }

    [Fact]
    public void GetShiftWithDetails_InvalidId_ReturnsNull()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        Assert.Null(provider.GetShiftWithDetails(9999));
    }

    // ── CreateShift ──────────────────────────────────────────

    [Fact]
    public void CreateShift_PersistsToDb()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var waiter = db.Roles.First(r => r.Name == "Waiter");

        provider.CreateShift(new Shift
        {
            Date = TestDate,
            StartTime = new(9, 0, 0),
            EndTime = new(17, 0, 0),
            EmployeeId = john.Id,
            RoleId = waiter.Id
        });

        var shift = db.Shifts.Single();
        Assert.Equal(TestDate, shift.Date);
        Assert.Equal(new TimeSpan(9, 0, 0), shift.StartTime);
        Assert.Equal(john.Id, shift.EmployeeId);
    }

    // ── UpdateShift ──────────────────────────────────────────

    [Fact]
    public void UpdateShift_ChangesFieldsAndPersists()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);
        var barman = db.Roles.First(r => r.Name == "Barman");

        var shift = provider.GetShift(shiftId)!;
        provider.UpdateShift(shift, barman.Id, new(11, 0, 0), new(15, 0, 0));

        var updated = db.Shifts.Find(shiftId)!;
        Assert.Equal(barman.Id, updated.RoleId);
        Assert.Equal(new TimeSpan(11, 0, 0), updated.StartTime);
        Assert.Equal(new TimeSpan(15, 0, 0), updated.EndTime);
        db.Dispose();
    }

    // ── DeleteShift ──────────────────────────────────────────

    [Fact]
    public void DeleteShift_RemovesFromDb()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);

        provider.DeleteShift(shiftId);

        Assert.Empty(db.Shifts);
        db.Dispose();
    }

    [Fact]
    public void DeleteShift_NonExistent_DoesNotThrow()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        provider.DeleteShift(9999); // should not throw
    }

    [Fact]
    public void DeleteShift_OnlyRemovesTarget()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var waiter = db.Roles.First(r => r.Name == "Waiter");
        var barman = db.Roles.First(r => r.Name == "Barman");

        var s1 = new Shift { Date = TestDate, StartTime = new(8, 0, 0), EndTime = new(12, 0, 0), EmployeeId = john.Id, RoleId = waiter.Id };
        var s2 = new Shift { Date = TestDate, StartTime = new(14, 0, 0), EndTime = new(18, 0, 0), EmployeeId = john.Id, RoleId = barman.Id };
        db.Shifts.AddRange(s1, s2);
        db.SaveChanges();

        provider.DeleteShift(s1.Id);

        Assert.Single(db.Shifts);
        Assert.Equal(s2.Id, db.Shifts.Single().Id);
    }

    // ── HasOverlap ───────────────────────────────────────────

    [Fact]
    public void HasOverlap_OverlappingShift_ReturnsTrue()
    {
        var (db, _) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);
        var john = db.Employees.First(e => e.Name == "John Smith");

        Assert.True(provider.HasOverlap(john.Id, TestDate, new(12, 0, 0), new(16, 0, 0), 0));
        db.Dispose();
    }

    [Fact]
    public void HasOverlap_AdjacentShift_ReturnsFalse()
    {
        var (db, _) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);
        var john = db.Employees.First(e => e.Name == "John Smith");

        Assert.False(provider.HasOverlap(john.Id, TestDate, new(14, 0, 0), new(18, 0, 0), 0));
        db.Dispose();
    }

    [Fact]
    public void HasOverlap_ExcludesSelf()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(
            TestDate, new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0));
        var provider = new ShiftsProvider(db);
        var john = db.Employees.First(e => e.Name == "John Smith");

        Assert.False(provider.HasOverlap(john.Id, TestDate, new(10, 30, 0), new(13, 30, 0), shiftId));
        db.Dispose();
    }

    // ── GetWeekSchedule ──────────────────────────────────────

    [Fact]
    public void GetWeekSchedule_ContainsAllEmployees()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        var model = provider.GetWeekSchedule();

        Assert.Equal(4, model.Employees.Count);
    }

    [Fact]
    public void GetWeekSchedule_EmployeesSortedByName()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        var model = provider.GetWeekSchedule();
        var names = model.Employees.Select(e => e.EmployeeName).ToList();

        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public void GetWeekSchedule_EachEmployeeHasSevenDays()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        var model = provider.GetWeekSchedule();

        foreach (var emp in model.Employees)
            Assert.Equal(7, emp.Days.Count);
    }

    [Fact]
    public void GetWeekSchedule_WeekStartsOnMonday()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        Assert.Equal(DayOfWeek.Monday, provider.GetWeekSchedule().WeekStart.DayOfWeek);
    }

    [Fact]
    public void GetWeekSchedule_WeekEndsOnSunday()
    {
        using var db = TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);

        Assert.Equal(DayOfWeek.Sunday, provider.GetWeekSchedule().WeekEnd.DayOfWeek);
    }
}
