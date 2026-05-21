using System.Text;
using CodeLens.Application.Interfaces.Auth;
using CodeLens.Application.Interfaces.Users;
using CodeLens.Application.Services;
using CodeLens.Infrastructure.Data;
using CodeLens.Infrastructure.Options;
using CodeLens.Infrastructure.Repositories;
using CodeLens.Infrastructure.Services;
using CodeLens.Infrastructure.Sevices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience =  builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["Frontend:Url"] ?? "")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
} );

builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EncryptionOptions>(builder.Configuration.GetSection("Encryption"));
builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection("Frontend"));

builder.Services.AddHttpClient<IGitHubAuthService, GitHubAuthService>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("Dev");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.Run();


