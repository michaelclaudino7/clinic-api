using ClinicApi.DTOs.Patients;
using ClinicApi.Models;
using ClinicApi.Repositories;
using Mapster;

namespace ClinicApi.Services;

public class PatientAlreadyExistsException : Exception
{
    public PatientAlreadyExistsException(string message) : base(message) { }
}

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var patients = await _patientRepository.GetAllAsync();
        return patients.Adapt<IEnumerable<PatientDto>>();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        return patient?.Adapt<PatientDto>();
    }

    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        var existing = await _patientRepository.GetByCpfAsync(dto.Cpf);
        if (existing is not null)
        {
            throw new PatientAlreadyExistsException("A patient with this CPF already exists.");
        }

        var patient = dto.Adapt<Patient>();
        await _patientRepository.AddAsync(patient);
        await _patientRepository.SaveChangesAsync();

        return patient.Adapt<PatientDto>();
    }

    public async Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient is null)
        {
            return null;
        }

        patient.Name = dto.Name;
        patient.Email = dto.Email;
        patient.Phone = dto.Phone;

        _patientRepository.Update(patient);
        await _patientRepository.SaveChangesAsync();

        return patient.Adapt<PatientDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient is null)
        {
            return false;
        }

        _patientRepository.Delete(patient);
        return await _patientRepository.SaveChangesAsync();
    }
}
