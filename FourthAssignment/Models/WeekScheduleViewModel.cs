namespace FourthAssignment.Models;

public class WeekScheduleViewModel
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<EmployeeWeekSchedule> Employees { get; set; } = new();
}
