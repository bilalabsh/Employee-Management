using EmployeeManagement.Domain.Data;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EmployeeManagement.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveRefreshToken(string username, string refreshToken, DateTime expiryDate)
        {
            var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Username == username);
            if (existingToken != null)
            {
                _context.RefreshTokens.Remove(existingToken);
            }

            var newRefreshToken = new RefreshToken
            {
                Username = username,
                Token = refreshToken,
                ExpiryDate = expiryDate
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshToken(string username)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Username == username);
        }

        public async Task DeleteRefreshToken(string username)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Username == username);
            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }
    }
}
