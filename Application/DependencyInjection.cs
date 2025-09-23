using Application.Interfaces.Auth;
using Application.Interfaces.Movies;
using Application.Services.Auth;
using Application.Services.Movies;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMovieService, MovieService>();

        return services;
    }
}
