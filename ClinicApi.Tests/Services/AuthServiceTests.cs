using ClinicApi.DTOs.Auth;
using ClinicApi.Models;
using ClinicApi.Repositories;
using ClinicApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ClinicApi.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configuration = BuildJwtConfiguration();
        _service = new AuthService(_userRepositoryMock.Object, _configuration);
    }

    private static IConfiguration BuildJwtConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "UnitTestSuperSecretKeyThatIsLongEnough123456" },
            { "Jwt:Issuer", "ClinicApiTests" },
            { "Jwt:Audience", "ClinicApiTestsUsers" },
            { "Jwt:ExpiresInMinutes", "60" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        var dto = new RegisterDto { Name = "John Doe", Email = "john@test.com", Password = "Senha123!" };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new User { Id = 1, Email = dto.Email });

        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _service.RegisterAsync(dto));

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsNew_CreatesUserAndReturnsToken()
    {
        var dto = new RegisterDto { Name = "Jane Doe", Email = "jane@test.com", Password = "Senha123!" };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(new User());

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(true);

        var result = await _service.RegisterAsync(dto);

        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsInvalidCredentialsException()
    {
        var dto = new LoginDto { Email = "unknown@test.com", Password = "Senha123!" };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ThrowsInvalidCredentialsException()
    {
        var dto = new LoginDto { Email = "john@test.com", Password = "WrongPassword" };

        var user = new User
        {
            Id = 1,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsToken()
    {
        var dto = new LoginDto { Email = "john@test.com", Password = "CorrectPassword" };

        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var result = await _service.LoginAsync(dto);

        Assert.False(string.IsNullOrEmpty(result.Token));
    }
}
