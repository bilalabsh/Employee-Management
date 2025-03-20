using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Core.Services;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagement.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthController(JwtService jwtService, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenRepository = refreshTokenRepository;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            List<string> roles = new List<string>();

            //admin check
            if (login.Username == "admin" && login.Password == "password")
            {
                roles.Add("Admin");
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(login.Username);
                if (user == null)
                {
                    return Unauthorized("Invalid username or password.");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized("Invalid username or password.");
                }

               //Fetch roles from Identity tables
                roles = (await _userManager.GetRolesAsync(user)).ToList();
            }

            //Generate Tokens
            var (accessToken, refreshToken, refreshTokenExpiry) = _jwtService.GenerateTokens(login.Username, roles);
            await _refreshTokenRepository.SaveRefreshToken(login.Username, refreshToken, refreshTokenExpiry);

            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenRequest)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenRequest.RefreshToken);
            if (principal == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Invalid refresh token: no username found.");
            }

            var storedRefreshToken = await _refreshTokenRepository.GetRefreshToken(username);
            if (storedRefreshToken == null || storedRefreshToken.Token != refreshTokenRequest.RefreshToken)
            {
                return Unauthorized("Invalid refresh token.");
            }

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return Unauthorized("Refresh token has expired. Please log in again.");
            }

            // Get roles from Identity
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var (newAccessToken, newRefreshToken, newRefreshTokenExpiry) = _jwtService.GenerateTokens(username, roles.ToList());

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow.AddMinutes(5))
            {
                await _refreshTokenRepository.SaveRefreshToken(username, newRefreshToken, newRefreshTokenExpiry);
                return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
            }

            return Ok(new { AccessToken = newAccessToken });
        }





    }
}
