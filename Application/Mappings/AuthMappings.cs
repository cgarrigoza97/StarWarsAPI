using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Mappings;

public static class AuthMappings
{
    public static User ToEntity(this RegisterDto registerDto)
    {
        return new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Role = registerDto.Role
        };
    }

    public static AuthResponseDto ToResponse(this User user, string token)
    {
        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }
}