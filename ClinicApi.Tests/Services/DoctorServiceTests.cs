using ClinicApi.DTOs.Doctors;
using ClinicApi.Models;
using ClinicApi.Repositories;
using ClinicApi.Services;
using Moq;
using Xunit;

namespace ClinicApi.Tests.Services;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _repositoryMock;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _repositoryMock = new Mock<IDoctorRepository>();
        _service = new DoctorService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenLicenseNumberAlreadyExists_ThrowsDoctorAlreadyExistsException()
    {
        var dto = new CreateDoctorDto
        {
            Name = "Dr. Smith",
            Specialty = "Cardiology",
            LicenseNumber = "CRM12345",
            Email = "smith@test.com"
        };

        _repositoryMock
            .Setup(r => r.GetByLicenseNumberAsync(dto.LicenseNumber))
            .ReturnsAsync(new Doctor { Id = 1, LicenseNumber = dto.LicenseNumber });

        await Assert.ThrowsAsync<DoctorAlreadyExistsException>(() => _service.CreateAsync(dto));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Doctor>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenLicenseNumberIsNew_CreatesAndReturnsDoctor()
    {
        var dto = new CreateDoctorDto
        {
            Name = "Dr. Jones",
            Specialty = "Dermatology",
            LicenseNumber = "CRM67890",
            Email = "jones@test.com"
        };

        _repositoryMock
            .Setup(r => r.GetByLicenseNumberAsync(dto.LicenseNumber))
            .ReturnsAsync((Doctor?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Doctor>()))
            .ReturnsAsync(new Doctor());

        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.LicenseNumber, result.LicenseNumber);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Doctor>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenDoctorDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Doctor?)null);

        var dto = new UpdateDoctorDto { Name = "New Name", Specialty = "New Specialty", Email = "new@test.com" };

        var result = await _service.UpdateAsync(99, dto);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenDoctorExists_UpdatesAndReturnsDoctor()
    {
        var doctor = new Doctor { Id = 1, Name = "Old Name", Specialty = "Old Specialty", Email = "old@test.com", LicenseNumber = "CRM11111" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(doctor);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var dto = new UpdateDoctorDto { Name = "New Name", Specialty = "New Specialty", Email = "new@test.com" };

        var result = await _service.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("New Name", result!.Name);
        Assert.Equal("New Specialty", result.Specialty);
    }
}
