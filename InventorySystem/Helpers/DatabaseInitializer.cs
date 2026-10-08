using InventorySystem.Data;
using InventorySystem.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Helpers
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, IAuthService authService)
        {
            // 1. Enable WAL mode and Busy Timeout for concurrency and performance
            try
            {
                await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;");
                await context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout = 5000;");
                await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL;");
            }
            catch
            {
                // Ignore if pragma fails on specific connection state
            }

            // 2. Try applying pending migrations, fallback to EnsureCreated
            try
            {
                await context.Database.MigrateAsync();
            }
            catch
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 3. Self-healing schema: Ensure every table exists unconditionally
            await EnsureAllTablesExistAsync(context);

            // 4. Ensure any missing columns in existing tables (e.g. Sales table additions)
            await EnsureColumnsExistAsync(context);

            // 5. Ensure default admin user exists
            await authService.EnsureDefaultAdminAsync();
        }

        private static async Task EnsureAllTablesExistAsync(AppDbContext context)
        {
            var tableScripts = new[]
            {
                @"CREATE TABLE IF NOT EXISTS ""Products"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Products"" PRIMARY KEY AUTOINCREMENT,
                    ""Name"" TEXT NOT NULL,
                    ""Description"" TEXT NULL,
                    ""Category"" TEXT NOT NULL,
                    ""Price"" TEXT NOT NULL,
                    ""Quantity"" INTEGER NOT NULL,
                    ""IsActive"" INTEGER NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""ImageData"" BLOB NULL,
                    ""ImagePath"" TEXT NULL
                );",

                @"CREATE TABLE IF NOT EXISTS ""Clients"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Clients"" PRIMARY KEY AUTOINCREMENT,
                    ""Email"" TEXT NOT NULL,
                    ""PhoneNumber"" TEXT NOT NULL,
                    ""IsActive"" INTEGER NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""DocumentNumber"" TEXT NOT NULL,
                    ""FirstName"" TEXT NOT NULL,
                    ""LastName"" TEXT NOT NULL,
                    ""Address"" TEXT NULL,
                    ""DateOfBirth"" TEXT NULL
                );",

                @"CREATE TABLE IF NOT EXISTS ""Suppliers"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Suppliers"" PRIMARY KEY AUTOINCREMENT,
                    ""CompanyName"" TEXT NOT NULL,
                    ""Email"" TEXT NOT NULL,
                    ""PhoneNumber"" TEXT NOT NULL,
                    ""Website"" TEXT NULL,
                    ""Category"" TEXT NULL,
                    ""IsActive"" INTEGER NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""DocumentNumber"" TEXT NOT NULL,
                    ""FirstName"" TEXT NOT NULL,
                    ""LastName"" TEXT NOT NULL,
                    ""Address"" TEXT NULL,
                    ""DateOfBirth"" TEXT NULL
                );",

                @"CREATE TABLE IF NOT EXISTS ""Users"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Users"" PRIMARY KEY AUTOINCREMENT,
                    ""Username"" TEXT NOT NULL,
                    ""PasswordHash"" TEXT NOT NULL,
                    ""FullName"" TEXT NOT NULL,
                    ""Role"" INTEGER NOT NULL,
                    ""IsActive"" INTEGER NOT NULL,
                    ""CreatedAt"" TEXT NOT NULL,
                    ""LastLogin"" TEXT NULL
                );",

                @"CREATE TABLE IF NOT EXISTS ""Sales"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Sales"" PRIMARY KEY AUTOINCREMENT,
                    ""ClientId"" INTEGER NOT NULL,
                    ""SaleDate"" TEXT NOT NULL,
                    ""TotalAmount"" TEXT NOT NULL,
                    ""CreatedByUserId"" INTEGER NULL,
                    ""PaymentMethod"" TEXT NOT NULL DEFAULT 'Efectivo',
                    ""AmountPaid"" TEXT NOT NULL DEFAULT 0,
                    ""ChangeDue"" TEXT NOT NULL DEFAULT 0,
                    ""Currency"" TEXT NOT NULL DEFAULT 'COP',
                    CONSTRAINT ""FK_Sales_Clients_ClientId"" FOREIGN KEY (""ClientId"") REFERENCES ""Clients"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_Sales_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""Id"") ON DELETE SET NULL
                );",

                @"CREATE TABLE IF NOT EXISTS ""SaleDetails"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_SaleDetails"" PRIMARY KEY AUTOINCREMENT,
                    ""SaleId"" INTEGER NOT NULL,
                    ""ProductId"" INTEGER NOT NULL,
                    ""Quantity"" INTEGER NOT NULL,
                    ""UnitPrice"" TEXT NOT NULL,
                    ""TotalPrice"" TEXT NOT NULL,
                    CONSTRAINT ""FK_SaleDetails_Products_ProductId"" FOREIGN KEY (""ProductId"") REFERENCES ""Products"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_SaleDetails_Sales_SaleId"" FOREIGN KEY (""SaleId"") REFERENCES ""Sales"" (""Id"") ON DELETE CASCADE
                );",

                @"CREATE TABLE IF NOT EXISTS ""AuditLogs"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AuditLogs"" PRIMARY KEY AUTOINCREMENT,
                    ""UserId"" INTEGER NOT NULL,
                    ""Action"" TEXT NOT NULL,
                    ""Details"" TEXT NULL,
                    ""Timestamp"" TEXT NOT NULL,
                    CONSTRAINT ""FK_AuditLogs_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                );",

                @"CREATE TABLE IF NOT EXISTS ""BusinessSettings"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_BusinessSettings"" PRIMARY KEY AUTOINCREMENT,
                    ""CompanyName"" TEXT NOT NULL,
                    ""TaxId"" TEXT NOT NULL,
                    ""Address"" TEXT NOT NULL,
                    ""Phone"" TEXT NOT NULL,
                    ""Email"" TEXT NOT NULL,
                    ""TaxPercentage"" TEXT NOT NULL,
                    ""CurrencySymbol"" TEXT NOT NULL
                );"
            };

            foreach (var script in tableScripts)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(script);
                }
                catch
                {
                    // Ignore if already created
                }
            }
        }

        private static async Task EnsureColumnsExistAsync(AppDbContext context)
        {
            var salesColumns = new Dictionary<string, string>
            {
                { "CreatedByUserId", "INTEGER NULL" },
                { "PaymentMethod", "TEXT NOT NULL DEFAULT 'Efectivo'" },
                { "AmountPaid", "TEXT NOT NULL DEFAULT 0" },
                { "ChangeDue", "TEXT NOT NULL DEFAULT 0" },
                { "Currency", "TEXT NOT NULL DEFAULT 'COP'" }
            };

            foreach (var col in salesColumns)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync($"ALTER TABLE \"Sales\" ADD COLUMN \"{col.Key}\" {col.Value};");
                }
                catch
                {
                    // Column already exists, safe to ignore
                }
            }
        }
    }
}
