using InventorySystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BusinessSetting> BusinessSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Use BaseDirectory to ensure the DB is created in the app folder, not System32 or elsewhere
            string dbName = "inventory.db";
            string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName);

            // Detect if existing DB is standard unencrypted SQLite (starts with 'SQLite format 3')
            bool isUnencrypted = false;
            if (System.IO.File.Exists(dbPath))
            {
                try
                {
                    using var fs = new System.IO.FileStream(dbPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                    byte[] header = new byte[16];
                    int bytesRead = fs.Read(header, 0, 16);
                    if (bytesRead >= 15 && System.Text.Encoding.ASCII.GetString(header, 0, 15).StartsWith("SQLite format 3"))
                    {
                        isUnencrypted = true;
                    }
                }
                catch
                {
                    // Fallback to default
                }
            }

            if (isUnencrypted)
            {
                // Open unencrypted existing database without password to preserve all existing data!
                options.UseSqlite($"Data Source={dbPath};Default Timeout=30;");
            }
            else
            {
                // Open with SQLCipher encryption password
                options.UseSqlite($"Data Source={dbPath};Password=AntiGravitySecure123!;Default Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sale -> User (nullable FK, no cascade delete)
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.CreatedBy)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // AuditLog -> User
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
