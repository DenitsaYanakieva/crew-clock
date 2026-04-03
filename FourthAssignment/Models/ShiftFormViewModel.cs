using System.ComponentModel.DataAnnotations;

namespace FourthAssignment.Models;

public class ShiftFormViewModel
{
    public int ShiftId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    public string StartTime { get; set; } = string.Empty;

    [Required(ErrorMessage = "End time is required")]
    public string EndTime { get; set; } = string.Empty;

    public IEnumerable<Role> AvailableRoles { get; set; } = Enumerable.Empty<Role>();
}
