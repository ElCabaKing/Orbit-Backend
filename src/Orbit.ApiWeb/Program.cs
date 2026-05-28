using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orbit.ApiWeb.Validators;
using Orbit.Application.Features.Auth;
using Orbit.Application.Features.Follows;
using Orbit.Application.Features.Posts;
using Orbit.Application.Features.Profiles;
using Orbit.Application.Interfaces;
using Orbit.Infrastructure.Extensions;
using Orbit.Shared.Constants;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

var jwtSecret = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtSecret) ?? string.Empty;
var jwtIssuer = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtIssuer) ?? DefaultsConstants.JwtIssuer;
var jwtAudience = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtAudience) ?? DefaultsConstants.JwtAudience;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
