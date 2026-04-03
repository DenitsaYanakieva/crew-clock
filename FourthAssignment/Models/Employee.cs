using System.ComponentModel.DataAnnotations;

namespace FourthAssignment.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public List<Role> Roles { get; set; } = [];
}
