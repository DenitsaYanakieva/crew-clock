using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FourthAssignment.Data;
using FourthAssignment.Models;

namespace FourthAssignment.Tests;

public static class TestHelper
{
    public static AppDbContext CreateDb()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static AppDbContext CreateSeededDb()
    {
        var db = CreateDb();
        DbInitializer.Seed(db);
        return db;
    }

    /// <summary>
    /// Seeds DB and adds a shift for the given employee on the given date.
    /// Returns the created shift's Id.
    /// </summary>
    public static (AppDbContext db, int shiftId) CreateDbWithShift(
        DateTime date, TimeSpan start, TimeSpan end, string employeeName = "John Smith", string roleName = "Waiter")
    {
        var db = CreateSeededDb();
        var emp = db.Employees.First(e => e.Name == employeeName);
        var role = db.Roles.First(r => r.Name == roleName);

        var shift = new Shift
        {
            Date = date,
            StartTime = start,
            EndTime = end,
            EmployeeId = emp.Id,
            RoleId = role.Id
        };
        db.Shifts.Add(shift);
        db.SaveChanges();
        return (db, shift.Id);
    }
}
