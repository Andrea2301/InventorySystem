using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;

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
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

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

                // Restoring is more dangerous because the file might be in use
                // In a more complex app, we'd need to close all DB connections first.
                // For this simple SQLite app, we'll try to copy over it.
                // If it fails due to locks, we recommend the user to restart the app.
                
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
    }
}
