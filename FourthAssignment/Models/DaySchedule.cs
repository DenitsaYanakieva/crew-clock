namespace FourthAssignment.Models;

public class DaySchedule
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public List<ShiftDisplayItem> Shifts { get; set; } = new();
}
