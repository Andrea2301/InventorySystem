using InventorySystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(string username, string password, string fullName, UserRole role);
        Task UpdateAsync(User user);
        Task<bool> SetActiveAsync(int userId, bool isActive);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
    }
}
