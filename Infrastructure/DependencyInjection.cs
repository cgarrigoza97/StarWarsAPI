using Application.Interfaces.Auth;
using Application.Interfaces.BackgroundServices;
using Application.Interfaces.ExternalAPI;
using Application.Interfaces.Movies;
using Infrastructure.Authentication;
using Infrastructure.BackgroundJobs;
using Infrastructure.Data;
using Infrastructure.ExternalAPI.StarWars.Services;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment? environment = null)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddAuthenticationServices(configuration);
        services.AddAuthorizationPolicies();

        if (environment?.EnvironmentName != "Testing")
        {
            services.AddHangfireServices(configuration);
        }

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();

        services.AddHttpClient<IStarWarsApiService, StarWarsApiService>();

        services.AddScoped<IMovieSyncBackgroundService, MovieSyncBackgroundService>();

        return services;
    }
}
