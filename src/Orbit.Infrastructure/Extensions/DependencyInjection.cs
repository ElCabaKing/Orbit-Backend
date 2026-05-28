using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orbit.Application.Interfaces;
using Orbit.Infrastructure.DbContext;
using Orbit.Infrastructure.Repositories;
using Orbit.Infrastructure.Services;
using Orbit.Shared.Constants;

namespace Orbit.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable(EnvironmentConstants.DefaultConnection)
            ?? Environment.GetEnvironmentVariable(EnvironmentConstants.DefaultConnectionAlt);

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<OrbitDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddCloudinary();
        services.AddHashing();
        services.AddJwt();
        services.AddRedis();
        services.AddEmail();
        services.AddRepositories();

        return services;
    }

    public static IServiceCollection AddCloudinary(this IServiceCollection services)
    {
        var cloudName = Environment.GetEnvironmentVariable(EnvironmentConstants.CloudinaryCloudName) ?? string.Empty;
        var apiKey = Environment.GetEnvironmentVariable(EnvironmentConstants.CloudinaryApiKey) ?? string.Empty;
        var apiSecret = Environment.GetEnvironmentVariable(EnvironmentConstants.CloudinaryApiSecret) ?? string.Empty;

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
            Secret = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtSecret) ?? string.Empty,
            Issuer = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtIssuer) ?? DefaultsConstants.JwtIssuer,
            Audience = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtAudience) ?? DefaultsConstants.JwtAudience,
            AccessTokenExpirationMinutes = int.TryParse(
                Environment.GetEnvironmentVariable(EnvironmentConstants.JwtAccessTokenExpiration), out var accessMin)
                ? accessMin : DefaultsConstants.JwtAccessTokenExpirationMinutes,
            RefreshTokenExpirationDays = int.TryParse(
                Environment.GetEnvironmentVariable(EnvironmentConstants.JwtRefreshTokenExpiration), out var refreshDays)
                ? refreshDays : DefaultsConstants.JwtRefreshTokenExpirationDays,
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

    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        var connection = Environment.GetEnvironmentVariable(EnvironmentConstants.RedisConnection) ?? DefaultsConstants.RedisConnection;

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connection;
        });

        services.AddScoped<IResetTokenService, ResetTokenService>();

        return services;
    }

    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        var mailOptions = new MailOptions
        {
            Host = Environment.GetEnvironmentVariable(EnvironmentConstants.SmtpHost) ?? DefaultsConstants.SmtpHost,
            Port = int.TryParse(Environment.GetEnvironmentVariable(EnvironmentConstants.SmtpPort), out var port) ? port : DefaultsConstants.SmtpPort,
            Username = Environment.GetEnvironmentVariable(EnvironmentConstants.SmtpUsername) ?? string.Empty,
            Password = Environment.GetEnvironmentVariable(EnvironmentConstants.SmtpPassword) ?? string.Empty,
            FromName = Environment.GetEnvironmentVariable(EnvironmentConstants.SmtpFromName) ?? DefaultsConstants.SmtpFromName,
            FromEmail = Environment.GetEnvironmentVariable(EnvironmentConstants.SmtpFromEmail) ?? DefaultsConstants.SmtpFromEmail,
        };

        services.AddSingleton(mailOptions);
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
