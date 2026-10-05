using ClinicApi.DTOs.Appointments;

namespace ClinicApi.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> ConfirmAsync(int id);
    Task<AppointmentDto?> CompleteAsync(int id);
    Task<AppointmentDto?> CancelAsync(int id, CancelAppointmentDto dto);
}
