using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Rentify.Shared;

public static class JwtAuthExtensions
{
    public static IServiceCollection AddRentifyJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["JwtSettings:key"]
            ?? Environment.GetEnvironmentVariable("RENTIFY_JWT_KEY");

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "JwtSettings:key no está configurada. Ejecutá .\\setup-secrets.ps1 o definí RENTIFY_JWT_KEY.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256, SecurityAlgorithms.HmacSha256Signature },
            RoleClaimType = "role",
            NameClaimType = "sub",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
            });

        services.AddAuthorization();
        return services;
    }
}
