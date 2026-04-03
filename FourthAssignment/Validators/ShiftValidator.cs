using FourthAssignment.Providers;

namespace FourthAssignment.Validators;

public record ShiftValidationError(string Key, string Message);

public interface IShiftValidator
{
    List<ShiftValidationError> Validate(
        int employeeId, DateTime date, TimeSpan startTime, TimeSpan endTime,
        int roleId, int excludeShiftId = 0);
}

public class ShiftValidator : IShiftValidator
{
    private readonly IShiftsProvider _provider;

    public ShiftValidator(IShiftsProvider provider) => _provider = provider;

    public List<ShiftValidationError> Validate(
        int employeeId, DateTime date, TimeSpan startTime, TimeSpan endTime,
        int roleId, int excludeShiftId = 0)
    {
        var errors = new List<ShiftValidationError>();

        if (startTime >= endTime)
            errors.Add(new("StartTime", "Start time must be before end time."));

        if (_provider.HasOverlap(employeeId, date, startTime, endTime, excludeShiftId))
            errors.Add(new("", "This shift overlaps with an existing shift for this employee."));

        var roles = _provider.GetEmployeeRoles(employeeId);
        if (!roles.Any(r => r.Id == roleId))
            errors.Add(new("RoleId", "Selected role is not valid for this employee."));

        return errors;
    }
}
