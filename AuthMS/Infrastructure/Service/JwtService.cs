using Application.Interfaces.IServices.IAuthServices;
using Domain.Entities;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class JwtService : IAuthTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateAccessToken(User user)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:key"]!));

            var signingCredentials = new SigningCredentials(
                key: securityKey,
                algorithm: SecurityAlgorithms.HmacSha256Signature
            );

            var claims = new ClaimsIdentity();
            
            // Claims estándar
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            claims.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            claims.AddClaim(new Claim("IsActive", user.IsActive.ToString()));
            claims.AddClaim(new Claim("UserId", user.UserId.ToString()));
            claims.AddClaim(new Claim("FirstName", user.FirstName.ToString()));
            claims.AddClaim(new Claim("LastName", user.LastName.ToString()));
            
            claims.AddClaim(new Claim(CustomClaims.UserId, user.UserId.ToString()));
            claims.AddClaim(new Claim(CustomClaims.UserEmail, user.Email));
            claims.AddClaim(new Claim(CustomClaims.UserRole, user.Role));
            claims.AddClaim(new Claim(CustomClaims.IsEmailVerified, user.IsEmailVerified.ToString()));
            claims.AddClaim(new Claim(CustomClaims.AccountStatus, user.IsActive ? "Active" : "Inactive"));
            AddRoleBasedClaims(claims, user);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:TokenExpirationMinutes"]!)),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();            
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            var serializedJwt = tokenHandler.WriteToken(tokenConfig);

            return serializedJwt;
        }

        public Task<string> GenerateRefreshToken()
        {
            var size = int.Parse(_configuration["RefreshTokenSettings:Length"] ?? "64");
            var buffer = new byte[size];
            using var rn = RandomNumberGenerator.Create();
            rn.GetBytes(buffer);

            return Task.FromResult(Convert.ToBase64String(buffer));
        }
        public Task<int> GetRefreshTokenLifetimeInMinutes()
        {
            return Task.FromResult(int.Parse(_configuration["RefreshTokenSettings:LifeTimeInMinutes"]!));
        }

        /// <summary>
        /// Agrega claims específicos según el rol del usuario
        /// </summary>
        private void AddRoleBasedClaims(ClaimsIdentity claims, User user)
        {
            claims.AddClaim(new Claim(CustomClaims.CanEditOwnProfile, "true"));
            claims.AddClaim(new Claim(CustomClaims.CanViewBranchInfo, "true"));
            claims.AddClaim(new Claim(CustomClaims.CanViewOwnReservations, "true"));

            if (user.Role == UserRoles.Admin)
            {
                claims.AddClaim(new Claim(CustomClaims.CanManageReservations, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageFleet, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageBranches, "true"));
            }

            if (user.Role == UserRoles.Employee)
            {
                claims.AddClaim(new Claim(CustomClaims.CanManageReservations, "true"));
                claims.AddClaim(new Claim(CustomClaims.CanManageFleet, "true"));
            }

            if (user.Role == UserRoles.Customer)
            {
                claims.AddClaim(new Claim(CustomClaims.CustomerId, user.UserId.ToString()));
            }
        }
        
    }
    
}
