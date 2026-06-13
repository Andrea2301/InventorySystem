using InventorySystem.Models;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IAuthService
    {
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }

        Task<User?> LoginAsync(string username, string password);
        void Logout();
        Task<User> CreateUserAsync(string username, string password, string fullName, UserRole role);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task LogAuditAsync(string action, string? details = null);
        Task EnsureDefaultAdminAsync();
    }
}
