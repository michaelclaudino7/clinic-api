using ClinicApi.DTOs.Doctors;
using ClinicApi.DTOs.Patients;
using ClinicApi.Models;
using Mapster;

namespace ClinicApi.Mapping;

public class MappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Patient, PatientDto>.NewConfig();
        TypeAdapterConfig<CreatePatientDto, Patient>.NewConfig();
        TypeAdapterConfig<UpdatePatientDto, Patient>.NewConfig();

        TypeAdapterConfig<Doctor, DoctorDto>.NewConfig();
        TypeAdapterConfig<CreateDoctorDto, Doctor>.NewConfig();
        TypeAdapterConfig<UpdateDoctorDto, Doctor>.NewConfig();
    }
}
