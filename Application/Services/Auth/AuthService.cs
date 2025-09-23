using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Application.Mappings;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var user = registerDto.ToEntity();

        var createdUser = await _userRepository.CreateAsync(user, registerDto.Password);

        var roleName = registerDto.Role.ToString();
        if (await _userRepository.RoleExistsAsync(roleName))
        {
            await _userRepository.AddToRoleAsync(createdUser, roleName);
        }
        else
        {
            await _userRepository.AddToRoleAsync(createdUser, "User");
        }

        var token = await GenerateJwtTokenAsync(createdUser);

        return createdUser.ToResponse(token);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isValidPassword = await _userRepository.CheckPasswordAsync(user, loginDto.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = await GenerateJwtTokenAsync(user);

        return user.ToResponse(token);
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        // Add roles
        var userRoles = await _userRepository.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenExpirationMinutes = GetTokenExpirationMinutes();
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "",
            audience: jwtSettings["Audience"] ?? "",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetTokenExpirationMinutes()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        if (int.TryParse(jwtSettings["TokenExpirationInMinutes"], out int minutes))
        {
            return minutes;
        }
        return 60; 
    }
}