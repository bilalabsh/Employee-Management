using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Core.Services;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var jwtService = context.RequestServices.GetRequiredService<JwtService>();
        var refreshTokenRepository = context.RequestServices.GetRequiredService<IRefreshTokenRepository>();

        var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var refreshToken = context.Request.Headers["Refresh-Token"].FirstOrDefault(); 

        Console.WriteLine($"access Token: {accessToken}");
        Console.WriteLine($"refresh Token: {refreshToken}");

        if (!string.IsNullOrEmpty(accessToken))
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(accessToken);
            if (principal != null)
            {
                var username = principal.Identity?.Name;
                Console.WriteLine($"🟢 [DEBUG] Extracted Username from Access Token: {username}");

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                Console.WriteLine($"🟡 [DEBUG] Access Token Expiry: {jwtToken.ValidTo} | Current UTC Time: {DateTime.UtcNow}");

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    var storedRefreshToken = await refreshTokenRepository.GetRefreshToken(username);

                    if (storedRefreshToken == null)
                    {
                        Console.WriteLine($"❌ [DEBUG] No refresh token found for user: {username}");
                    }
                    else
                    {
                        Console.WriteLine($"🟢 [DEBUG] Found stored refresh token: {storedRefreshToken.Token}");
                        Console.WriteLine($"🟡 [DEBUG] Stored Refresh Token Expiry: {storedRefreshToken.ExpiryDate} | Current UTC Time: {DateTime.UtcNow}");
                    }

                    if (storedRefreshToken != null && storedRefreshToken.Token == refreshToken &&
                        storedRefreshToken.ExpiryDate > DateTime.UtcNow)
                    {
                        Console.WriteLine($"Refresh token is valid. User authenticated.");
                        context.User = principal;
                    }
                    else
                    {
                        Console.WriteLine($"Refresh token is invalid or expired!");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Refresh token expired. Please re-authenticate.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine($"Access token is still valid.");
                    context.User = principal;
                }
            }
        }

        await _next(context);
    }

}
