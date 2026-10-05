using ClinicApi.Data;
using ClinicApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicApi.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ClinicDbContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ToListAsync();
    }

    public async Task<bool> HasConflictAsync(int doctorId, DateTime scheduledAt)
    {
        return await _dbSet.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.ScheduledAt == scheduledAt &&
            a.Status != AppointmentStatus.Cancelled);
    }
}
