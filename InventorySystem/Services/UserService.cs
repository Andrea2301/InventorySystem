using BCrypt.Net;
using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task<User> CreateAsync(string username, string password, string fullName, UserRole role)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            var user = new User
            {
                Username = username.Trim(),
                PasswordHash = hash,
                FullName = fullName.Trim(),
                Role = role,
                IsActive = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> SetActiveAsync(int userId, bool isActive)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        {
            return await _db.Users.AnyAsync(u =>
                u.Username == username.Trim() &&
                (excludeId == null || u.Id != excludeId));
        }
    }
}
