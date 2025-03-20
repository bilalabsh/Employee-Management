using EmployeeManagement.Domain.Entities;
using System.Threading.Tasks;

namespace EmployeeManagement.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task SaveRefreshToken(string username, string refreshToken, DateTime expiryDate);
        Task<RefreshToken> GetRefreshToken(string username);
        Task DeleteRefreshToken(string username);
    }
}
