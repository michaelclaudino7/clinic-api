using ClinicApi.DTOs.Doctors;
using ClinicApi.Models;
using ClinicApi.Repositories;
using Mapster;

namespace ClinicApi.Services;

public class DoctorAlreadyExistsException : Exception
{
    public DoctorAlreadyExistsException(string message) : base(message) { }
}

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorService(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        var doctors = await _doctorRepository.GetAllAsync();
        return doctors.Adapt<IEnumerable<DoctorDto>>();
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        return doctor?.Adapt<DoctorDto>();
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
    {
        var existing = await _doctorRepository.GetByLicenseNumberAsync(dto.LicenseNumber);
        if (existing is not null)
        {
            throw new DoctorAlreadyExistsException("A doctor with this license number already exists.");
        }

        var doctor = dto.Adapt<Doctor>();
        await _doctorRepository.AddAsync(doctor);
        await _doctorRepository.SaveChangesAsync();

        return doctor.Adapt<DoctorDto>();
    }

    public async Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor is null)
        {
            return null;
        }

        doctor.Name = dto.Name;
        doctor.Specialty = dto.Specialty;
        doctor.Email = dto.Email;

        _doctorRepository.Update(doctor);
        await _doctorRepository.SaveChangesAsync();

        return doctor.Adapt<DoctorDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id);
        if (doctor is null)
        {
            return false;
        }

        _doctorRepository.Delete(doctor);
        return await _doctorRepository.SaveChangesAsync();
    }
}
