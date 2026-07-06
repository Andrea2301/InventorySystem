using BCrypt.Net;
using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        public User? CurrentUser { get; private set; }
        public bool IsAuthenticated => CurrentUser != null;

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username.Trim() && u.IsActive);

            if (user == null)
                return null;

            // BCrypt.Verify is CPU-intensive — run on background thread to avoid freezing the UI
            bool isValid = await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
            if (!isValid)
                return null;

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            CurrentUser = user;

            // Log the login
            await LogAuditAsync("LOGIN", $"User '{user.Username}' logged in successfully.");

            return user;
        }

        public void Logout()
        {
            if (CurrentUser != null)
            {
                // Fire-and-forget log before clearing
                var userId = CurrentUser.Id;
                var username = CurrentUser.Username;
                _ = LogAuditCoreAsync(userId, "LOGOUT", $"User '{username}' logged out.");
            }
            CurrentUser = null;
        }

        public async Task<User> CreateUserAsync(string username, string password, string fullName, UserRole role)
        {
            // BCrypt.HashPassword is CPU-intensive — run on background thread
            string hash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12));

            var user = new User
            {
                Username = username.Trim(),
                PasswordHash = hash,
                FullName = fullName.Trim(),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            bool isValid = await Task.Run(() => BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash));
            if (!isValid) return false;

            user.PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12));
            await _db.SaveChangesAsync();
            await LogAuditAsync("CHANGE_PASSWORD", $"User '{user.Username}' changed their password.");
            return true;
        }

        public async Task LogAuditAsync(string action, string? details = null)
        {
            if (CurrentUser == null) return;
            await LogAuditCoreAsync(CurrentUser.Id, action, details);
        }

        private async Task LogAuditCoreAsync(int userId, string action, string? details)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                };
                _db.AuditLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // Audit logging should never crash the app
            }
        }

        public async Task EnsureDefaultAdminAsync()
        {
            bool adminExists = await _db.Users.AnyAsync();
            if (!adminExists)
            {
                await CreateUserAsync("admin", "Admin123!", "Administrator", UserRole.Admin);
            }
        }
    }
}
