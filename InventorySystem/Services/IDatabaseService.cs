using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IDatabaseService
    {
        string GetDatabasePath();
        Task<bool> BackupAsync(string destinationPath);
        Task<bool> RestoreAsync(string sourcePath);
        long GetDatabaseSize();
    }
}
