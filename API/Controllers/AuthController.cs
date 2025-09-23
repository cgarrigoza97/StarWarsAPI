using Application.DTOs.Auth;
using Application.DTOs.Common;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace StarWarsWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Endpoints for user registration and login")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)

    {
        _authService = authService;
    }

    /// <summary>
    /// Create a new user account
    /// </summary>
    /// <param name="registerDto">User registration data</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">User successfully registered</response>
    /// <response code="400">Invalid input data or validation errors</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register a new user",
        Description = "Creates a new user account with the provided details and returns a JWT token for authentication"
    )]
    [SwaggerResponse(200, "User successfully registered", typeof(AuthResponseDto))]
    [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseDto))]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var response = await _authService.RegisterAsync(registerDto);
        return Ok(response);
    }

    /// <summary>
    /// Login user and get JWT token
    /// </summary>
    /// <param name="loginDto">User credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    /// <response code="200">User successfully authenticated</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login user",
        Description = "Authenticates a user with email and password, returning a JWT token"
    )]
    [SwaggerResponse(200, "User successfully authenticated", typeof(AuthResponseDto))]
    [SwaggerResponse(400, "Invalid input data", typeof(ErrorResponseDto))]
    [SwaggerResponse(401, "Invalid credentials", typeof(ErrorResponseDto))]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);
        return Ok(response);
    }
}
