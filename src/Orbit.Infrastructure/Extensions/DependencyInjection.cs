using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orbit.Application.Interfaces;
using Orbit.Infrastructure.DbContext;
using Orbit.Infrastructure.Repositories;
using Orbit.Infrastructure.Services;

namespace Orbit.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<OrbitDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddCloudinary();
        services.AddHashing();
        services.AddJwt();
        services.AddRepositories();

        return services;
    }

    public static IServiceCollection AddCloudinary(this IServiceCollection services)
    {
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty;
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty;
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty;

        var account = new Account(cloudName, apiKey, apiSecret);
        var cloudinary = new Cloudinary(account);

        services.AddSingleton(cloudinary);
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        return services;
    }

    public static IServiceCollection AddHashing(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        return services;
    }

    public static IServiceCollection AddJwt(this IServiceCollection services)
    {
        var jwtOptions = new JwtOptions
        {
            Secret = Environment.GetEnvironmentVariable("JWT__SECRET") ?? string.Empty,
            Issuer = Environment.GetEnvironmentVariable("JWT__ISSUER") ?? "OrbitApi",
            Audience = Environment.GetEnvironmentVariable("JWT__AUDIENCE") ?? "OrbitClient",
            AccessTokenExpirationMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("JWT__ACCESSTOKENEXPIRATIONMINUTES"), out var accessMin)
                ? accessMin : 15,
            RefreshTokenExpirationDays = int.TryParse(
                Environment.GetEnvironmentVariable("JWT__REFRESHTOKENEXPIRATIONDAYS"), out var refreshDays)
                ? refreshDays : 7,
        };

        services.AddSingleton(jwtOptions);
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        return services;
    }
}
