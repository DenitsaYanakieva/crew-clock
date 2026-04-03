using FourthAssignment.Models;

namespace FourthAssignment.Providers;

public interface IShiftsProvider
{
    WeekScheduleViewModel GetWeekSchedule();
    Employee? GetEmployee(int employeeId);
    IEnumerable<Role> GetEmployeeRoles(int employeeId);
    Shift? GetShiftWithDetails(int shiftId);
    bool HasOverlap(int employeeId, DateTime date, TimeSpan start, TimeSpan end, int excludeId);
    void CreateShift(Shift shift);
    Shift? GetShift(int shiftId);
    void UpdateShift(Shift shift, int roleId, TimeSpan startTime, TimeSpan endTime);
    void DeleteShift(int shiftId);
}
