using Application.Interfaces.BackgroundServices;
using Application.Interfaces.Movies;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Text;

namespace API.Test.Integration;

public class CustomWebApplicationMovieFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecretKey = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256Algorithm";
    public Mock<IMovieRepository> MovieRepositoryMock { get; } = new();
    public Mock<IMovieService> MovieServiceMock { get; } = new();
    public Mock<IMovieSyncBackgroundService> MovieSyncBackgroundServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var hangfireDescriptors = services.Where(d => 
                d.ServiceType.FullName != null && 
                d.ServiceType.FullName.Contains("Hangfire")).ToList();
            
            foreach (var descriptor in hangfireDescriptors)
            {
                services.Remove(descriptor);
            }
            
            var authServiceDescriptors = services.Where(d => 
                d.ServiceType.FullName != null && 
                (d.ServiceType.FullName.Contains("JwtBearer") || 
                 d.ServiceType.FullName.Contains("Authentication"))).ToList();
            
            foreach (var descriptor in authServiceDescriptors)
            {
                services.Remove(descriptor);
            }
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecretKey)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Admin"));
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            });
            
            RemoveService<IMovieRepository>(services);
            RemoveService<IMovieService>(services);
            RemoveService<IMovieSyncBackgroundService>(services);
            RemoveService<DbContextOptions<ApplicationDbContext>>(services);
            RemoveService<ApplicationDbContext>(services);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            services.AddScoped(_ => MovieRepositoryMock.Object);
            services.AddScoped(_ => MovieServiceMock.Object);
            services.AddScoped(_ => MovieSyncBackgroundServiceMock.Object);
        });
    }
    
    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }
}


