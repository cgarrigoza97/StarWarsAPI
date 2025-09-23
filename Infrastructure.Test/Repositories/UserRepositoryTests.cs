using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Test.Repositories;

[TestFixture]
public class UserRepositoryTests
{
    private ApplicationDbContext _context;
    private UserManager<ApplicationUser> _userManager;
    private SignInManager<ApplicationUser> _signInManager;
    private RoleManager<IdentityRole> _roleManager;
    private UserRepository _userRepository;
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        _context.Database.EnsureCreated();

        _userRepository = new UserRepository(_userManager, _signInManager, _roleManager);

        SeedRoles().Wait();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _userManager?.Dispose();
        _roleManager?.Dispose();
        _serviceProvider?.Dispose();
    }

    private async Task SeedRoles()
    {
        var roles = new[] { "User", "Admin" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    [Test]
    public async Task CreateAsync_ValidUser_CreatesUserSuccessfully()
    {
        var user = new User
        {
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "luke@rebels.com",
            UserName = "luke@rebels.com",
            Role = UserRole.User
        };
        var password = "password123";

        var result = await _userRepository.CreateAsync(user, password);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.FirstName, Is.EqualTo("Luke"));
        Assert.That(result.LastName, Is.EqualTo("Skywalker"));
        Assert.That(result.Email, Is.EqualTo("luke@rebels.com"));
        Assert.That(result.Role, Is.EqualTo(UserRole.User));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));

        var createdUser = await _userManager.FindByEmailAsync("luke@rebels.com");
        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser.FirstName, Is.EqualTo("Luke"));
        Assert.That(createdUser.LastName, Is.EqualTo("Skywalker"));
    }

    [Test]
    public async Task CreateAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var user1 = new User
        {
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "luke@rebels.com",
            UserName = "luke@rebels.com",
            Role = UserRole.User
        };

        var user2 = new User
        {
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "luke@rebels.com",
            UserName = "luke@rebels.com",
            Role = UserRole.User
        };

        await _userRepository.CreateAsync(user1, "password123");

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _userRepository.CreateAsync(user2, "password123"));
    }

    [Test]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        var user = new User
        {
            FirstName = "Leia",
            LastName = "Organa",
            Email = "leia@rebels.com",
            UserName = "leia@rebels.com",
            Role = UserRole.Admin
        };
        
        await _userRepository.CreateAsync(user, "password123");

        var result = await _userRepository.GetByEmailAsync("leia@rebels.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.FirstName, Is.EqualTo("Leia"));
        Assert.That(result.LastName, Is.EqualTo("Organa"));
        Assert.That(result.Email, Is.EqualTo("leia@rebels.com"));
        Assert.That(result.Role, Is.EqualTo(UserRole.Admin));
    }

    [Test]
    public async Task GetByEmailAsync_NonExistentUser_ReturnsNull()
    {
        var result = await _userRepository.GetByEmailAsync("nonexistent@example.com");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CheckPasswordAsync_ValidPassword_ReturnsTrue()
    {
        var user = new User
        {
            FirstName = "Han",
            LastName = "Solo",
            Email = "han@rebels.com",
            UserName = "han@rebels.com",
            Role = UserRole.User
        };
        var password = "password123";
        
        var createdUser = await _userRepository.CreateAsync(user, password);

        var result = await _userRepository.CheckPasswordAsync(createdUser, password);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckPasswordAsync_InvalidPassword_ReturnsFalse()
    {
        var user = new User
        {
            FirstName = "Han",
            LastName = "Solo",
            Email = "han@rebels.com",
            UserName = "han@rebels.com",
            Role = UserRole.User
        };
        var password = "password123";
        var wrongPassword = "wrongpassword";
        
        var createdUser = await _userRepository.CreateAsync(user, password);

        var result = await _userRepository.CheckPasswordAsync(createdUser, wrongPassword);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckPasswordAsync_NonExistentUser_ReturnsFalse()
    {
        var nonExistentUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Ghost",
            LastName = "User",
            Email = "ghost@example.com",
            UserName = "ghost@example.com",
            Role = UserRole.User
        };

        var result = await _userRepository.CheckPasswordAsync(nonExistentUser, "password123");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddToRoleAsync_ExistingUserAndRole_ReturnsTrue()
    {
        var user = new User
        {
            FirstName = "Obi-Wan",
            LastName = "Kenobi",
            Email = "obiwan@jedi.com",
            UserName = "obiwan@jedi.com",
            Role = UserRole.User
        };
        
        var createdUser = await _userRepository.CreateAsync(user, "password123");

        var result = await _userRepository.AddToRoleAsync(createdUser, "Admin");

        Assert.That(result, Is.True);

        var userRoles = await _userRepository.GetRolesAsync(createdUser);
        Assert.That(userRoles, Does.Contain("Admin"));
    }

    [Test]
    public async Task AddToRoleAsync_NonExistentUser_ReturnsFalse()
    {
        var nonExistentUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Ghost",
            LastName = "User",
            Email = "ghost@example.com",
            UserName = "ghost@example.com",
            Role = UserRole.User
        };

        var result = await _userRepository.AddToRoleAsync(nonExistentUser, "User");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RoleExistsAsync_ExistingRole_ReturnsTrue()
    {
        var userRoleExists = await _userRepository.RoleExistsAsync("User");
        var adminRoleExists = await _userRepository.RoleExistsAsync("Admin");

        Assert.That(userRoleExists, Is.True);
        Assert.That(adminRoleExists, Is.True);
    }

    [Test]
    public async Task RoleExistsAsync_NonExistentRole_ReturnsFalse()
    {
        var result = await _userRepository.RoleExistsAsync("NonExistentRole");

        Assert.That(result, Is.False);
    }
}
