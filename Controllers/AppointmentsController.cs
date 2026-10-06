using ClinicApi.DTOs.Appointments;
using ClinicApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
    {
        var appointments = await _appointmentService.GetAllAsync();
        return Ok(appointments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(CreateAppointmentDto dto)
    {
        var created = await _appointmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:int}/confirm")]
    public async Task<ActionResult<AppointmentDto>> Confirm(int id)
    {
        var result = await _appointmentService.ConfirmAsync(id);
        if (result is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<ActionResult<AppointmentDto>> Complete(int id)
    {
        var result = await _appointmentService.CompleteAsync(id);
        if (result is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<AppointmentDto>> Cancel(int id, CancelAppointmentDto dto)
    {
        var result = await _appointmentService.CancelAsync(id, dto);
        if (result is null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(result);
    }
}
