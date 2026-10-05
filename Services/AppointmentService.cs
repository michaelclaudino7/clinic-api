using ClinicApi.DTOs.Appointments;
using ClinicApi.Models;
using ClinicApi.Repositories;

namespace ClinicApi.Services;

public class AppointmentConflictException : Exception
{
    public AppointmentConflictException(string message) : base(message) { }
}

public class InvalidAppointmentDateException : Exception
{
    public InvalidAppointmentDateException(string message) : base(message) { }
}

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
}

public class InvalidAppointmentStatusException : Exception
{
    public InvalidAppointmentStatusException(string message) : base(message) { }
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var appointments = await _appointmentRepository.GetAllWithDetailsAsync();
        return appointments.Select(MapToDto);
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
        return appointment is null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (dto.ScheduledAt < DateTime.Now)
        {
            throw new InvalidAppointmentDateException("Appointment date cannot be in the past.");
        }

        var patient = await _patientRepository.GetByIdAsync(dto.PatientId);
        if (patient is null)
        {
            throw new EntityNotFoundException("Patient not found.");
        }

        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId);
        if (doctor is null)
        {
            throw new EntityNotFoundException("Doctor not found.");
        }

        var hasConflict = await _appointmentRepository.HasConflictAsync(dto.DoctorId, dto.ScheduledAt);
        if (hasConflict)
        {
            throw new AppointmentConflictException("The doctor already has an appointment scheduled at this time.");
        }

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            ScheduledAt = dto.ScheduledAt,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled
        };

        await _appointmentRepository.AddAsync(appointment);
        await _appointmentRepository.SaveChangesAsync();

        var created = await _appointmentRepository.GetByIdWithDetailsAsync(appointment.Id);
        return MapToDto(created!);
    }

    public async Task<AppointmentDto?> ConfirmAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
        if (appointment is null)
        {
            return null;
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidAppointmentStatusException("Only scheduled appointments can be confirmed.");
        }

        appointment.Status = AppointmentStatus.Confirmed;
        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> CompleteAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
        if (appointment is null)
        {
            return null;
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new InvalidAppointmentStatusException("Only confirmed appointments can be completed.");
        }

        appointment.Status = AppointmentStatus.Completed;
        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> CancelAsync(int id, CancelAppointmentDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
        if (appointment is null)
        {
            return null;
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            throw new InvalidAppointmentStatusException("This appointment cannot be cancelled.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = dto.Reason;
        _appointmentRepository.Update(appointment);
        await _appointmentRepository.SaveChangesAsync();

        return MapToDto(appointment);
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.Name,
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor.Name,
            ScheduledAt = appointment.ScheduledAt,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CancellationReason = appointment.CancellationReason,
            CreatedAt = appointment.CreatedAt
        };
    }
}
