using ClinicApi.Data;
using ClinicApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicApi.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ClinicDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByCpfAsync(string cpf)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Cpf == cpf);
    }
}
