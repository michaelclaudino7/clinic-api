using ClinicApi.Models;

namespace ClinicApi.Repositories;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<Appointment?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetAllWithDetailsAsync();
    Task<bool> HasConflictAsync(int doctorId, DateTime scheduledAt);
}
