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
            // Added Password for SQLCipher encryption (Option A)
            options.UseSqlite($"Data Source={dbPath};Password=AntiGravitySecure123!");
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
