using Application.Interfaces.Auth;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRepository(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public async Task<User> CreateAsync(User user, string password)
    {
        var applicationUser = new ApplicationUser();
        applicationUser.UpdateFromDomainUser(user);
        applicationUser.Id = Guid.NewGuid().ToString();

        var result = await _userManager.CreateAsync(applicationUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        return applicationUser.ToDomainUser();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var applicationUser = await _userManager.FindByEmailAsync(email);
        return applicationUser?.ToDomainUser();
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (applicationUser == null) return false;

        var result = await _signInManager.CheckPasswordSignInAsync(applicationUser, password, false);
        return result.Succeeded;
    }

    public async Task<bool> AddToRoleAsync(User user, string roleName)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (applicationUser == null) return false;

        var result = await _userManager.AddToRoleAsync(applicationUser, roleName);
        return result.Succeeded;
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        var applicationUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (applicationUser == null) return new List<string>();

        return await _userManager.GetRolesAsync(applicationUser);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }
}
