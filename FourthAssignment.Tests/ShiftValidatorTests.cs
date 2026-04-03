using Microsoft.EntityFrameworkCore;
using FourthAssignment.Models;
using FourthAssignment.Providers;
using FourthAssignment.Validators;

namespace FourthAssignment.Tests;

public class ShiftValidatorTests
{
    private static readonly DateTime TestDate = new(2025, 6, 2);

    private static (ShiftValidator validator, Data.AppDbContext db) Create(Data.AppDbContext? existingDb = null)
    {
        var db = existingDb ?? TestHelper.CreateSeededDb();
        var provider = new ShiftsProvider(db);
        return (new ShiftValidator(provider), db);
    }

    // ── Time range ───────────────────────────────────────────

    [Fact]
    public void Validate_StartAfterEnd_ReturnsStartTimeError()
    {
        var (validator, db) = Create();
        using (db)
        {
            var john = db.Employees.First(e => e.Name == "John Smith");
            var waiter = db.Roles.First(r => r.Name == "Waiter");

            var errors = validator.Validate(john.Id, TestDate, new(16, 0, 0), new(10, 0, 0), waiter.Id);

            Assert.Contains(errors, e => e.Key == "StartTime");
        }
    }

    [Fact]
    public void Validate_StartEqualsEnd_ReturnsStartTimeError()
    {
        var (validator, db) = Create();
        using (db)
        {
            var john = db.Employees.First(e => e.Name == "John Smith");
            var waiter = db.Roles.First(r => r.Name == "Waiter");

            var errors = validator.Validate(john.Id, TestDate, new(10, 0, 0), new(10, 0, 0), waiter.Id);

            Assert.Contains(errors, e => e.Key == "StartTime");
        }
    }

    [Fact]
    public void Validate_ValidTimeRange_NoTimeError()
    {
        var (validator, db) = Create();
        using (db)
        {
            var john = db.Employees.First(e => e.Name == "John Smith");
            var waiter = db.Roles.First(r => r.Name == "Waiter");

            var errors = validator.Validate(john.Id, TestDate, new(10, 0, 0), new(14, 0, 0), waiter.Id);

            Assert.DoesNotContain(errors, e => e.Key == "StartTime");
        }
    }

    // ── Overlap ──────────────────────────────────────────────

    [Fact]
    public void Validate_OverlappingShift_ReturnsOverlapError()
    {
        var (db, _) = TestHelper.CreateDbWithShift(TestDate, new(10, 0, 0), new(14, 0, 0));
        var (validator, _) = Create(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var barman = db.Roles.First(r => r.Name == "Barman");

        var errors = validator.Validate(john.Id, TestDate, new(12, 0, 0), new(16, 0, 0), barman.Id);

        Assert.Contains(errors, e => e.Key == "" && e.Message.Contains("overlaps"));
        db.Dispose();
    }

    [Fact]
    public void Validate_NonOverlappingShift_NoOverlapError()
    {
        var (db, _) = TestHelper.CreateDbWithShift(TestDate, new(10, 0, 0), new(14, 0, 0));
        var (validator, _) = Create(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var barman = db.Roles.First(r => r.Name == "Barman");

        var errors = validator.Validate(john.Id, TestDate, new(14, 0, 0), new(18, 0, 0), barman.Id);

        Assert.DoesNotContain(errors, e => e.Message.Contains("overlaps"));
        db.Dispose();
    }

    [Fact]
    public void Validate_SelfOverlap_ExcludedById()
    {
        var (db, shiftId) = TestHelper.CreateDbWithShift(TestDate, new(10, 0, 0), new(14, 0, 0));
        var (validator, _) = Create(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var waiter = db.Roles.First(r => r.Name == "Waiter");

        var errors = validator.Validate(john.Id, TestDate, new(10, 30, 0), new(13, 30, 0), waiter.Id, shiftId);

        Assert.DoesNotContain(errors, e => e.Message.Contains("overlaps"));
        db.Dispose();
    }

    // ── Role validation ──────────────────────────────────────

    [Fact]
    public void Validate_InvalidRole_ReturnsRoleError()
    {
        var (validator, db) = Create();
        using (db)
        {
            var john = db.Employees.First(e => e.Name == "John Smith");
            var chef = db.Roles.First(r => r.Name == "Chef");

            var errors = validator.Validate(john.Id, TestDate, new(10, 0, 0), new(14, 0, 0), chef.Id);

            Assert.Contains(errors, e => e.Key == "RoleId");
        }
    }

    [Fact]
    public void Validate_ValidRole_NoRoleError()
    {
        var (validator, db) = Create();
        using (db)
        {
            var john = db.Employees.First(e => e.Name == "John Smith");
            var waiter = db.Roles.First(r => r.Name == "Waiter");

            var errors = validator.Validate(john.Id, TestDate, new(10, 0, 0), new(14, 0, 0), waiter.Id);

            Assert.DoesNotContain(errors, e => e.Key == "RoleId");
        }
    }

    // ── Combined ─────────────────────────────────────────────

    [Fact]
    public void Validate_AllValid_ReturnsNoErrors()
    {
        var (validator, db) = Create();
        using (db)
        {
            var john = db.Employees.First(e => e.Name == "John Smith");
            var waiter = db.Roles.First(r => r.Name == "Waiter");

            var errors = validator.Validate(john.Id, TestDate, new(10, 0, 0), new(14, 0, 0), waiter.Id);

            Assert.Empty(errors);
        }
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAll()
    {
        var (db, _) = TestHelper.CreateDbWithShift(TestDate, new(10, 0, 0), new(14, 0, 0));
        var (validator, _) = Create(db);
        var john = db.Employees.First(e => e.Name == "John Smith");
        var chef = db.Roles.First(r => r.Name == "Chef");

        // bad time range + overlap + invalid role
        var errors = validator.Validate(john.Id, TestDate, new(16, 0, 0), new(10, 0, 0), chef.Id);

        Assert.True(errors.Count >= 2); // at least time + role errors
        db.Dispose();
    }
}
