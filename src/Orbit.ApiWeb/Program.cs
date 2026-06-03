using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Orbit.ApiWeb.Validators;
using Orbit.ApiWeb.Workers;
using Orbit.Application.Features.Auth;
using Orbit.Application.Features.Chats;
using Orbit.Application.Features.Follows;
using Orbit.Application.Features.Posts;
using Orbit.Application.Features.Profiles;
using Orbit.Application.Features.Communities;
using Orbit.Application.Features.Hashtags;
using Orbit.Application.Features.Roles;
using Orbit.Application.Interfaces;
using Orbit.Infrastructure.Extensions;
using Orbit.Shared.Constants;
using Scalar.AspNetCore;

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
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IHashtagService, HashtagService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
builder.Services.AddHostedService<NotificationBackgroundService>();

var frontendUrl = Environment.GetEnvironmentVariable(EnvironmentConstants.FrontendUrl);
var frontendUrlDev = Environment.GetEnvironmentVariable(EnvironmentConstants.FrontendUrlDev);

var frontendUrls = new List<string>();
if (!string.IsNullOrWhiteSpace(frontendUrl)) frontendUrls.Add(frontendUrl);
if (!string.IsNullOrWhiteSpace(frontendUrlDev) && frontendUrlDev != frontendUrl) frontendUrls.Add(frontendUrlDev);

if (frontendUrls.Count == 0)
    frontendUrls.Add("http://localhost:4200");
var jwtSecret = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtSecret) ?? string.Empty;
var jwtIssuer = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtIssuer) ?? DefaultsConstants.JwtIssuer;
var jwtAudience = Environment.GetEnvironmentVariable(EnvironmentConstants.JwtAudience) ?? DefaultsConstants.JwtAudience;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
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
        policy.WithOrigins([.. frontendUrls])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("ModeratorOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("moderator") || context.User.IsInRole("admin")));
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme. Enter your token."
        });
        document.SecurityRequirements = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
                }] = new List<string>()
            }
        };
        return Task.CompletedTask;
    });
});
builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 128 * 1024;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/hubs"))
    {
        var token = context.Request.Query["access_token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(token))
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }
    }
    await next();
});

app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Orbit.ApiWeb.Hubs.ChatHub>("/hubs/chat");
app.MapHub<Orbit.ApiWeb.Hubs.NotificationHub>("/hubs/notifications");

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Orbit API")
        .WithTheme(ScalarTheme.Purple);
});

app.Run();
