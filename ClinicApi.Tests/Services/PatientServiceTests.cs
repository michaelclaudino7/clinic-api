using ClinicApi.DTOs.Patients;
using ClinicApi.Models;
using ClinicApi.Repositories;
using ClinicApi.Services;
using Moq;
using Xunit;

namespace ClinicApi.Tests.Services;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _repositoryMock;
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _repositoryMock = new Mock<IPatientRepository>();
        _service = new PatientService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenCpfAlreadyExists_ThrowsPatientAlreadyExistsException()
    {
        var dto = new CreatePatientDto
        {
            Name = "John Doe",
            Cpf = "12345678900",
            Email = "john@test.com",
            BirthDate = new DateOnly(1990, 1, 1)
        };

        _repositoryMock
            .Setup(r => r.GetByCpfAsync(dto.Cpf))
            .ReturnsAsync(new Patient { Id = 1, Cpf = dto.Cpf });

        await Assert.ThrowsAsync<PatientAlreadyExistsException>(() => _service.CreateAsync(dto));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCpfIsNew_CreatesAndReturnsPatient()
    {
        var dto = new CreatePatientDto
        {
            Name = "Jane Doe",
            Cpf = "98765432100",
            Email = "jane@test.com",
            BirthDate = new DateOnly(1995, 5, 20)
        };

        _repositoryMock
            .Setup(r => r.GetByCpfAsync(dto.Cpf))
            .ReturnsAsync((Patient?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Patient>()))
            .ReturnsAsync(new Patient());

        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Cpf, result.Cpf);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Patient?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenPatientDoesNotExist_ReturnsFalse()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Patient?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Patient>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenPatientExists_DeletesAndReturnsTrue()
    {
        var patient = new Patient { Id = 1, Name = "John Doe", Cpf = "12345678900" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(patient);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
        _repositoryMock.Verify(r => r.Delete(patient), Times.Once);
    }
}
