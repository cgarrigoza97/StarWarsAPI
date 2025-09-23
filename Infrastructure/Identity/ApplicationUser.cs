using Microsoft.AspNetCore.Identity;
using Domain.Entities;

namespace Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public User ToDomainUser()
    {
        return new User
        {
            Id = Guid.Parse(Id),
            FirstName = FirstName,
            LastName = LastName,
            Email = Email ?? string.Empty,
            UserName = UserName ?? string.Empty,
            Role = Role,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
    
    public void UpdateFromDomainUser(User domainUser)
    {
        FirstName = domainUser.FirstName;
        LastName = domainUser.LastName;
        Email = domainUser.Email;
        UserName = domainUser.UserName;
        Role = domainUser.Role;
        UpdatedAt = DateTime.UtcNow;
    }
}
