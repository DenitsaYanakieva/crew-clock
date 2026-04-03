using FourthAssignment.Data;
using FourthAssignment.Models;

namespace FourthAssignment.Tests;

public class DbInitializerTests
{
    [Fact]
    public void Seed_CreatesAllFourRoles()
    {
        using var db = TestHelper.CreateSeededDb();
        var roles = db.Roles.Select(r => r.Name).OrderBy(n => n).ToList();
        Assert.Equal(4, roles.Count);
        Assert.Contains("Waiter", roles);
        Assert.Contains("Barman", roles);
        Assert.Contains("Chef", roles);
        Assert.Contains("Cleaner", roles);
    }

    [Fact]
    public void Seed_CreatesFourEmployees()
    {
        using var db = TestHelper.CreateSeededDb();
        Assert.Equal(4, db.Employees.Count());
    }

    [Theory]
    [InlineData("John Smith", new[] { "Waiter", "Barman" })]
    [InlineData("Mary Johnson", new[] { "Chef" })]
    [InlineData("Joe Bloggs", new[] { "Barman", "Cleaner", "Waiter" })]
    [InlineData("Ben Arnold", new[] { "Barman", "Chef" })]
    public void Seed_AssignsCorrectRolesToEmployee(string name, string[] expectedRoles)
    {
        using var db = TestHelper.CreateSeededDb();
        var emp = db.Employees
            .Include(e => e.Roles)
            .First(e => e.Name == name);

        var actual = emp.Roles.Select(r => r.Name).OrderBy(n => n).ToArray();
        Assert.Equal(expectedRoles.OrderBy(n => n).ToArray(), actual);
    }

    [Fact]
    public void Seed_IsIdempotent_DoesNotDuplicateData()
    {
        using var db = TestHelper.CreateSeededDb();
        // Seed again
        DbInitializer.Seed(db);

        Assert.Equal(4, db.Roles.Count());
        Assert.Equal(4, db.Employees.Count());
    }

    [Fact]
    public void Seed_ShiftsTableStartsEmpty()
    {
        using var db = TestHelper.CreateSeededDb();
        Assert.Empty(db.Shifts);
    }
}
