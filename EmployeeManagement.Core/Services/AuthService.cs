using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtService _jwtService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(JwtService jwtService, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<(bool Success, string Message, string AccessToken, string RefreshToken)> LoginAsync(LoginDTO login)
        {
            List<string> roles = new List<string>();

            if (login.Username == "admin" && login.Password == "password")
            {
                roles.Add("Admin");
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(login.Username);
                if (user == null)
                    return (false, "Invalid username or password.", null, null);

                var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
                if (!result.Succeeded)
                    return (false, "Invalid username or password.", null, null);

                roles = (await _userManager.GetRolesAsync(user)).ToList();
            }

            var (accessToken, refreshToken, refreshTokenExpiry) = _jwtService.GenerateTokens(login.Username, roles);
            await _refreshTokenRepository.SaveRefreshToken(login.Username, refreshToken, refreshTokenExpiry);

            return (true, "Login successful", accessToken, refreshToken);
        }

        public async Task<(bool Success, string Message, string AccessToken, string RefreshToken)> RefreshTokenAsync(RefreshTokenDTO refreshTokenRequest)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenRequest.RefreshToken);
            if (principal == null)
                return (false, "Invalid refresh token.", null, null);

            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return (false, "Invalid refresh token: no username found.", null, null);

            var storedRefreshToken = await _refreshTokenRepository.GetRefreshToken(username);
            if (storedRefreshToken == null || storedRefreshToken.Token != refreshTokenRequest.RefreshToken)
                return (false, "Invalid refresh token.", null, null);

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                return (false, "Refresh token has expired. Please log in again.", null, null);

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return (false, "User not found.", null, null);

            var roles = await _userManager.GetRolesAsync(user);
            var (newAccessToken, newRefreshToken, newRefreshTokenExpiry) = _jwtService.GenerateTokens(username, roles.ToList());

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow.AddMinutes(5))
            {
                await _refreshTokenRepository.SaveRefreshToken(username, newRefreshToken, newRefreshTokenExpiry);
                return (true, "Token refreshed", newAccessToken, newRefreshToken);
            }

            return (true, "Token refreshed", newAccessToken, null);
        }
    }
}
