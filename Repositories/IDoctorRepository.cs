using ClinicApi.Models;

namespace ClinicApi.Repositories;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<Doctor?> GetByLicenseNumberAsync(string licenseNumber);
}
