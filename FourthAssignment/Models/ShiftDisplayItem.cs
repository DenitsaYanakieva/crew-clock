namespace FourthAssignment.Models;

public class ShiftDisplayItem
{
    public int ShiftId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
