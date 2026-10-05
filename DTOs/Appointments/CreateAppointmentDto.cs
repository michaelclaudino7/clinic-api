namespace ClinicApi.DTOs.Appointments;

public class CreateAppointmentDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Notes { get; set; }
}
