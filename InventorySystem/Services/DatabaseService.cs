using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace InventorySystem.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _dbPath;

        public DatabaseService()
        {
            // Same logic as in AppDbContext
            string dbName = "inventory.db";
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName);
        }

        public string GetDatabasePath() => _dbPath;

        public async Task<bool> BackupAsync(string destinationPath)
        {
            try
            {
                // Ensure the directory exists
                string dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) 
                    Directory.CreateDirectory(dir);

                // Use File.Copy with overwrite
                await Task.Run(() => File.Copy(_dbPath, destinationPath, true));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RestoreAsync(string sourcePath)
        {
            try
            {
                if (!File.Exists(sourcePath)) return false;

                // 1. Basic validation: Check if it's a valid SQLite file (Header check)
                if (!await IsValidSqliteFile(sourcePath)) return false;

                // 2. Connection Management: Clear all pools to release locks
                // This is crucial to avoid "The process cannot access the file because it is being used by another process"
                SqliteConnection.ClearAllPools();

                // 3. Forced GC to ensure any pending handles are cleaned up
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 4. Perform the restore
                await Task.Run(() => File.Copy(sourcePath, _dbPath, true));
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public long GetDatabaseSize()
        {
            if (File.Exists(_dbPath))
            {
                return new FileInfo(_dbPath).Length;
            }
            return 0;
        }

        private async Task<bool> IsValidSqliteFile(string filePath)
        {
            try
            {
                byte[] header = new byte[16];
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    await fs.ReadAsync(header.AsMemory(0, 16));
                }
                
                // SQLite files always start with this exact string
                string headerString = System.Text.Encoding.ASCII.GetString(header, 0, 15);
                return headerString.Contains("SQLite format 3");
            }
            catch
            {
                return false;
            }
        }
    }
}
