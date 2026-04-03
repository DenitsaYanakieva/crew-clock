namespace FourthAssignment.Models;

public class EmployeeWeekSchedule
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<DaySchedule> Days { get; set; } = new();
}
