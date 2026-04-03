using Microsoft.AspNetCore.Mvc;
using FourthAssignment.Models;
using FourthAssignment.Providers;
using FourthAssignment.Validators;

namespace FourthAssignment.Controllers;

public class ShiftsController : Controller
{
    private readonly IShiftsProvider _provider;
    private readonly IShiftValidator _validator;

    public ShiftsController(IShiftsProvider provider, IShiftValidator validator)
    {
        _provider = provider;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Create(int employeeId, string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format.");

        var employee = _provider.GetEmployee(employeeId);
        if (employee == null) return NotFound();

        return View(new ShiftFormViewModel
        {
            EmployeeId = employeeId,
            EmployeeName = employee.Name,
            Date = parsedDate,
            AvailableRoles = _provider.GetEmployeeRoles(employeeId)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(ShiftFormViewModel model)
    {
        if (!TimeSpan.TryParse(model.StartTime, out var startTime))
            ModelState.AddModelError("StartTime", "Invalid start time format.");

        if (!TimeSpan.TryParse(model.EndTime, out var endTime))
            ModelState.AddModelError("EndTime", "Invalid end time format.");

        if (ModelState.IsValid)
        {
            foreach (var e in _validator.Validate(model.EmployeeId, model.Date, startTime, endTime, model.RoleId))
                ModelState.AddModelError(e.Key, e.Message);
        }

        if (ModelState.IsValid)
        {
            _provider.CreateShift(new Shift
            {
                Date = model.Date,
                StartTime = startTime,
                EndTime = endTime,
                EmployeeId = model.EmployeeId,
                RoleId = model.RoleId
            });
            return RedirectToAction("Index", "Home");
        }

        model.AvailableRoles = _provider.GetEmployeeRoles(model.EmployeeId);
        return View(model);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var shift = _provider.GetShiftWithDetails(id);
        if (shift == null) return NotFound();

        return View(new ShiftFormViewModel
        {
            ShiftId = shift.Id,
            EmployeeId = shift.EmployeeId,
            EmployeeName = shift.Employee.Name,
            Date = shift.Date,
            RoleId = shift.RoleId,
            StartTime = shift.StartTime.ToString(@"hh\:mm"),
            EndTime = shift.EndTime.ToString(@"hh\:mm"),
            AvailableRoles = _provider.GetEmployeeRoles(shift.EmployeeId)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(ShiftFormViewModel model)
    {
        if (!TimeSpan.TryParse(model.StartTime, out var startTime))
            ModelState.AddModelError("StartTime", "Invalid start time format.");

        if (!TimeSpan.TryParse(model.EndTime, out var endTime))
            ModelState.AddModelError("EndTime", "Invalid end time format.");

        if (ModelState.IsValid)
        {
            foreach (var e in _validator.Validate(model.EmployeeId, model.Date, startTime, endTime, model.RoleId, model.ShiftId))
                ModelState.AddModelError(e.Key, e.Message);
        }

        if (ModelState.IsValid)
        {
            var shift = _provider.GetShift(model.ShiftId);
            if (shift == null) return NotFound();

            _provider.UpdateShift(shift, model.RoleId, startTime, endTime);
            return RedirectToAction("Index", "Home");
        }

        model.AvailableRoles = _provider.GetEmployeeRoles(model.EmployeeId);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _provider.DeleteShift(id);
        return RedirectToAction("Index", "Home");
    }
}
