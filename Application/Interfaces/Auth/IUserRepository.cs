using Domain.Entities;

namespace Application.Interfaces.Auth;

public interface IUserRepository
{
    Task<User> CreateAsync(User user, string password);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<bool> AddToRoleAsync(User user, string roleName);
    Task<IList<string>> GetRolesAsync(User user);
    Task<bool> RoleExistsAsync(string roleName);
}
