using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.Interfaces.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StarWarsWebApp.Controllers;

namespace API.Test.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Test]
    public async Task Register_WithValidData_ReturnsOkWithAuthResponse()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "luke@jedi.com",
            Password = "password123",
            Role = UserRole.User
        };

        var expectedResponse = new AuthResponseDto
        {
            Token = "valid-jwt-token",
            Email = "luke@jedi.com",
            FirstName = "Luke",
            LastName = "Skywalker",
            Role = UserRole.User
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
            .ReturnsAsync(expectedResponse);
        
        var result = await _controller.Register(registerDto);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public void Register_WithDuplicatedEmail_ServiceThrowsException()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "luke@jedi.com",
            Password = "password123",
            Role = UserRole.User
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));
        
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Register(registerDto));
        
        Assert.That(exception.Message, Is.EqualTo("Email already exists"));
    }

    [Test]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        var loginDto = new LoginDto
        {
            Email = "luke@jedi.com",
            Password = "password123"
        };

        var expectedResponse = new AuthResponseDto
        {
            Token = "valid-jwt-token",
            Email = "luke@jedi.com",
            FirstName = "Luke",
            LastName = "Skywalker",
            Role = UserRole.User
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Login(loginDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public void Login_WithInvalidCredentials_ServiceThrowsException()
    {
        var loginDto = new LoginDto
        {
            Email = "luke@jedi.com",
            Password = "wrongpassword"
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.Login(loginDto));
        
        Assert.That(exception.Message, Is.EqualTo("Invalid credentials"));
        _mockAuthService.Verify(x => x.LoginAsync(loginDto), Times.Once);
    }

    [Test]
    public void Login_WithNonExistentUser_ServiceThrowsException()
    {
        var loginDto = new LoginDto
        {
            Email = "nonexistent@jedi.com",
            Password = "password123"
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("User not found"));

        var exception = Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.Login(loginDto));
        
        Assert.That(exception.Message, Is.EqualTo("User not found"));
    }
}
