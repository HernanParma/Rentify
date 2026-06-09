using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Rentify.Shared;

public static class JwtAuthExtensions
{
    public const string DevJwtKey = "RentifySecretKey2024Minimo32Caracteres!";

    public static IServiceCollection AddRentifyJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["JwtSettings:key"] ?? DevJwtKey;

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
