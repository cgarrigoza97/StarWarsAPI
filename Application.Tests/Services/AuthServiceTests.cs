using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Application.Services.Auth;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Application.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IConfigurationSection> _mockJwtSection;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();
        
        _mockJwtSection.Setup(x => x["SecretKey"]).Returns("ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256Algorithm");
        _mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        _mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
        _mockJwtSection.Setup(x => x["TokenExpirationInMinutes"]).Returns("60");
        
        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(_mockJwtSection.Object);
        
        _authService = new AuthService(_mockUserRepository.Object, _mockConfiguration.Object);
    }

    [Test]
    public async Task RegisterAsync_ValidInput_ReturnsAuthResponse()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "Han",
            LastName = "Solo",
            Email = "hansolo@example.com",
            Password = "password123",
            Role = UserRole.User
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Han",
            LastName = "Solo",
            Email = "hansolo@example.com",
            Role = UserRole.User
        };

        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(createdUser);
        _mockUserRepository.Setup(x => x.RoleExistsAsync("User")).ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.AddToRoleAsync(createdUser, "User")).ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetRolesAsync(createdUser)).ReturnsAsync(new List<string> { "User" });

        var result = await _authService.RegisterAsync(registerDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(registerDto.Email));
        Assert.That(result.FirstName, Is.EqualTo(registerDto.FirstName));
        Assert.That(result.LastName, Is.EqualTo(registerDto.LastName));
        Assert.That(result.Role, Is.EqualTo(registerDto.Role));
        Assert.That(result.Token, Is.Not.Empty);
    }

    [Test]
    public async Task RegisterAsync_AdminRole_WhenRoleExists_AddsAdminRole()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            Password = "password123",
            Role = UserRole.Admin
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            Role = UserRole.Admin
        };

        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(createdUser);
        _mockUserRepository.Setup(x => x.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.AddToRoleAsync(createdUser, "Admin")).ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetRolesAsync(createdUser)).ReturnsAsync(new List<string> { "Admin" });

        var result = await _authService.RegisterAsync(registerDto);

        Assert.That(result.Role, Is.EqualTo(UserRole.Admin));
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "hansolo@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Han",
            LastName = "Solo",
            Email = "hansolo@example.com",
            Role = UserRole.User
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var result = await _authService.LoginAsync(loginDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(user.Email));
        Assert.That(result.FirstName, Is.EqualTo(user.FirstName));
        Assert.That(result.LastName, Is.EqualTo(user.LastName));
        Assert.That(result.Role, Is.EqualTo(user.Role));
        Assert.That(result.Token, Is.Not.Empty);
    }

    [Test]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
    }

    [Test]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var loginDto = new LoginDto
        {
            Email = "hansolo@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Han",
            LastName = "Solo",
            Email = "hansolo@example.com",
            Role = UserRole.User
        };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(false);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
    }
}
