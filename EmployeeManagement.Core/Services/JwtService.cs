using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace EmployeeManagement.Core.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry) GenerateTokens(string username, List<string> roles)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);

            // Generate Access Token
            var accessToken = GenerateToken(username, roles, key,
                DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpiration"])), "access");

            // Generate Refresh Token
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpiration"]));
            var refreshToken = GenerateToken(username, new List<string>(), key, refreshTokenExpiry, "refresh");

            return (accessToken, refreshToken, refreshTokenExpiry);
        }

        private string GenerateToken(string username, List<string> roles, byte[] key, DateTime expiration, string tokenType)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("typ", tokenType) 
            };

            if (tokenType == "access")
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // Don't validate expiration for refresh token
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
            var jwtToken = securityToken as JwtSecurityToken;

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public bool IsRefreshToken(string token)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            return principal.Claims.Any(c => c.Type == "typ" && c.Value == "refresh");
        }
    }
}
