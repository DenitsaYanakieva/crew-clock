using Microsoft.EntityFrameworkCore;
using FourthAssignment.Models;

namespace FourthAssignment.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Roles)
            .WithMany()
            .UsingEntity("EmployeeRoles");

        base.OnModelCreating(modelBuilder);
    }
}
