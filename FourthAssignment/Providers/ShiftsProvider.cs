using Microsoft.EntityFrameworkCore;
using FourthAssignment.Data;
using FourthAssignment.Models;

namespace FourthAssignment.Providers;

public class ShiftsProvider(AppDbContext db) : IShiftsProvider
{
    public WeekScheduleViewModel GetWeekSchedule()
    {
        var today = DateTime.Today;
        var weekStart = GetWeekStart(today);
        var weekEnd = weekStart.AddDays(6);

        var employees = db.Employees
            .Include(e => e.Roles)
            .OrderBy(e => e.Name)
            .ToList();

        var shifts = db.Shifts
            .Include(s => s.Role)
            .Where(s => s.Date >= weekStart && s.Date <= weekEnd)
            .ToList();

        return new WeekScheduleViewModel
        {
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            Employees = employees.Select(emp => new EmployeeWeekSchedule
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                Days = Enumerable.Range(0, 7).Select(offset =>
                {
                    var day = weekStart.AddDays(offset);
                    return new DaySchedule
                    {
                        Date = day,
                        DayName = day.ToString("ddd"),
                        Shifts = shifts
                            .Where(s => s.EmployeeId == emp.Id && s.Date.Date == day.Date)
                            .OrderBy(s => s.StartTime)
                            .Select(s => new ShiftDisplayItem
                            {
                                ShiftId = s.Id,
                                RoleName = s.Role.Name,
                                StartTime = s.StartTime,
                                EndTime = s.EndTime
                            }).ToList()
                    };
                }).ToList()
            }).ToList()
        };
    }

    public Employee? GetEmployee(int employeeId) => db.Employees.Find(employeeId);

    public IEnumerable<Role> GetEmployeeRoles(int employeeId)
    {
        var employee = db.Employees
            .Include(e => e.Roles)
            .FirstOrDefault(e => e.Id == employeeId);
        return employee?.Roles ?? Enumerable.Empty<Role>().ToList();
    }

    public Shift? GetShiftWithDetails(int shiftId)
    {
        return db.Shifts
            .Include(s => s.Employee)
            .Include(s => s.Role)
            .FirstOrDefault(s => s.Id == shiftId);
    }

    public bool HasOverlap(int employeeId, DateTime date, TimeSpan start, TimeSpan end, int excludeId)
    {
        return db.Shifts
            .Where(s => s.EmployeeId == employeeId && s.Date == date && s.Id != excludeId)
            .AsEnumerable()
            .Any(s => s.StartTime < end && s.EndTime > start);
    }

    public void CreateShift(Shift shift)
    {
        db.Shifts.Add(shift);
        db.SaveChanges();
    }

    public Shift? GetShift(int shiftId) => db.Shifts.Find(shiftId);

    public void UpdateShift(Shift shift, int roleId, TimeSpan startTime, TimeSpan endTime)
    {
        shift.RoleId = roleId;
        shift.StartTime = startTime;
        shift.EndTime = endTime;
        db.SaveChanges();
    }

    public void DeleteShift(int shiftId)
    {
        var shift = db.Shifts.Find(shiftId);
        if (shift != null)
        {
            db.Shifts.Remove(shift);
            db.SaveChanges();
        }
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.Date.AddDays(-diff);
    }
}
