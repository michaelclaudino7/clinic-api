using ClinicApi.DTOs.Patients;
using ClinicApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetAll()
    {
        var patients = await _patientService.GetAllAsync();
        return Ok(patients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDto>> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            return NotFound(new { message = "Patient not found." });
        }

        return Ok(patient);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PatientDto>> Create(CreatePatientDto dto)
    {
        var created = await _patientService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<PatientDto>> Update(int id, UpdatePatientDto dto)
    {
        var updated = await _patientService.UpdateAsync(id, dto);
        if (updated is null)
        {
            return NotFound(new { message = "Patient not found." });
        }

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _patientService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Patient not found." });
        }

        return NoContent();
    }
}
