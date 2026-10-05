using ClinicApi.Models;

namespace ClinicApi.Repositories;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<Patient?> GetByCpfAsync(string cpf);
}
