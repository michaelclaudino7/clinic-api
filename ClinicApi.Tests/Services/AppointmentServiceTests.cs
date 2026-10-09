using ClinicApi.DTOs.Appointments;
using ClinicApi.Models;
using ClinicApi.Repositories;
using ClinicApi.Services;
using Moq;
using Xunit;

namespace ClinicApi.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();

        _service = new AppointmentService(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenScheduledInPast_ThrowsInvalidAppointmentDateException()
    {
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduledAt = DateTime.Now.AddDays(-1)
        };

        await Assert.ThrowsAsync<InvalidAppointmentDateException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenPatientNotFound_ThrowsEntityNotFoundException()
    {
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduledAt = DateTime.Now.AddDays(1)
        };

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.PatientId))
            .ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenDoctorNotFound_ThrowsEntityNotFoundException()
    {
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduledAt = DateTime.Now.AddDays(1)
        };

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.PatientId))
            .ReturnsAsync(new Patient { Id = 1, Name = "John Doe" });

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.DoctorId))
            .ReturnsAsync((Doctor?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenDoctorHasConflict_ThrowsAppointmentConflictException()
    {
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduledAt = DateTime.Now.AddDays(1)
        };

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.PatientId))
            .ReturnsAsync(new Patient { Id = 1, Name = "John Doe" });

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.DoctorId))
            .ReturnsAsync(new Doctor { Id = 1, Name = "Dr. Smith" });

        _appointmentRepositoryMock
            .Setup(r => r.HasConflictAsync(dto.DoctorId, dto.ScheduledAt))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<AppointmentConflictException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_CreatesAndReturnsScheduledAppointment()
    {
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduledAt = DateTime.Now.AddDays(1)
        };

        var patient = new Patient { Id = 1, Name = "John Doe" };
        var doctor = new Doctor { Id = 1, Name = "Dr. Smith" };

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(dto.PatientId)).ReturnsAsync(patient);
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(dto.DoctorId)).ReturnsAsync(doctor);
        _appointmentRepositoryMock.Setup(r => r.HasConflictAsync(dto.DoctorId, dto.ScheduledAt)).ReturnsAsync(false);
        _appointmentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Appointment>())).ReturnsAsync(new Appointment());
        _appointmentRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var createdAppointment = new Appointment
        {
            Id = 10,
            PatientId = dto.PatientId,
            Patient = patient,
            DoctorId = dto.DoctorId,
            Doctor = doctor,
            ScheduledAt = dto.ScheduledAt,
            Status = AppointmentStatus.Scheduled
        };

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(createdAppointment);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        Assert.Equal(patient.Name, result.PatientName);
        Assert.Equal(doctor.Name, result.DoctorName);
    }

    [Fact]
    public async Task ConfirmAsync_WhenAppointmentIsNotScheduled_ThrowsInvalidAppointmentStatusException()
    {
        var appointment = new Appointment
        {
            Id = 1,
            Status = AppointmentStatus.Completed,
            Patient = new Patient { Name = "John Doe" },
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<InvalidAppointmentStatusException>(() => _service.ConfirmAsync(1));
    }

    [Fact]
    public async Task ConfirmAsync_WhenAppointmentNotFound_ReturnsNull()
    {
        _appointmentRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _service.ConfirmAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CancelAsync_WhenAppointmentIsCompleted_ThrowsInvalidAppointmentStatusException()
    {
        var appointment = new Appointment
        {
            Id = 1,
            Status = AppointmentStatus.Completed,
            Patient = new Patient { Name = "John Doe" },
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(appointment);

        var dto = new CancelAppointmentDto { Reason = "Patient request" };

        await Assert.ThrowsAsync<InvalidAppointmentStatusException>(() => _service.CancelAsync(1, dto));
    }

    [Fact]
    public async Task CancelAsync_WhenAppointmentIsScheduled_CancelsSuccessfully()
    {
        var appointment = new Appointment
        {
            Id = 1,
            Status = AppointmentStatus.Scheduled,
            Patient = new Patient { Name = "John Doe" },
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(appointment);

        _appointmentRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var dto = new CancelAppointmentDto { Reason = "Patient request" };

        var result = await _service.CancelAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Cancelled, result!.Status);
        Assert.Equal("Patient request", result.CancellationReason);
    }
}
