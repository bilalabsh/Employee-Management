using EmployeeManagement.Core.DTOs;
using System.Threading.Tasks;

namespace EmployeeManagement.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string AccessToken, string RefreshToken)> LoginAsync(LoginDTO login);
        Task<(bool Success, string Message, string AccessToken, string RefreshToken)> RefreshTokenAsync(RefreshTokenDTO refreshTokenRequest);
    }
}
