using FourthAssignment.Models;

namespace FourthAssignment.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Name = "Waiter" },
                new Role { Name = "Barman" },
                new Role { Name = "Chef" },
                new Role { Name = "Cleaner" }
            );
            db.SaveChanges();
        }

        if (!db.Employees.Any())
        {
            var waiter = db.Roles.First(r => r.Name == "Waiter");
            var barman = db.Roles.First(r => r.Name == "Barman");
            var chef = db.Roles.First(r => r.Name == "Chef");
            var cleaner = db.Roles.First(r => r.Name == "Cleaner");

            db.Employees.AddRange(
                new Employee { Name = "John Smith", Roles = new List<Role> { waiter, barman } },
                new Employee { Name = "Mary Johnson", Roles = new List<Role> { chef } },
                new Employee { Name = "Joe Bloggs", Roles = new List<Role> { waiter, barman, cleaner } },
                new Employee { Name = "Ben Arnold", Roles = new List<Role> { barman, chef } }
            );
            db.SaveChanges();
        }
    }
}
