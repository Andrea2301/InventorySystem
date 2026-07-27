BEGIN TRANSACTION;

ALTER TABLE "Sales" ADD "AmountPaid" TEXT NOT NULL DEFAULT '0.0';

ALTER TABLE "Sales" ADD "ChangeDue" TEXT NOT NULL DEFAULT '0.0';

ALTER TABLE "Sales" ADD "CreatedByUserId" INTEGER NULL;

ALTER TABLE "Sales" ADD "Currency" TEXT NOT NULL DEFAULT '';

ALTER TABLE "Sales" ADD "PaymentMethod" TEXT NOT NULL DEFAULT '';

CREATE TABLE "BusinessSettings" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BusinessSettings" PRIMARY KEY AUTOINCREMENT,
    "CompanyName" TEXT NOT NULL,
    "TaxId" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "Phone" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "TaxPercentage" TEXT NOT NULL,
    "CurrencySymbol" TEXT NOT NULL
);

CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "FullName" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "LastLogin" TEXT NULL
);

CREATE TABLE "AuditLogs" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AuditLogs" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "Action" TEXT NOT NULL,
    "Details" TEXT NULL,
    "Timestamp" TEXT NOT NULL,
    CONSTRAINT "FK_AuditLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Sales_CreatedByUserId" ON "Sales" ("CreatedByUserId");

CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");

CREATE TABLE "ef_temp_Sales" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Sales" PRIMARY KEY AUTOINCREMENT,
    "AmountPaid" TEXT NOT NULL,
    "ChangeDue" TEXT NOT NULL,
    "ClientId" INTEGER NOT NULL,
    "CreatedByUserId" INTEGER NULL,
    "Currency" TEXT NOT NULL,
    "PaymentMethod" TEXT NOT NULL,
    "SaleDate" TEXT NOT NULL,
    "TotalAmount" TEXT NOT NULL,
    CONSTRAINT "FK_Sales_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Sales_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

INSERT INTO "ef_temp_Sales" ("Id", "AmountPaid", "ChangeDue", "ClientId", "CreatedByUserId", "Currency", "PaymentMethod", "SaleDate", "TotalAmount")
SELECT "Id", "AmountPaid", "ChangeDue", "ClientId", "CreatedByUserId", "Currency", "PaymentMethod", "SaleDate", "TotalAmount"
FROM "Sales";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Sales";

ALTER TABLE "ef_temp_Sales" RENAME TO "Sales";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

CREATE INDEX "IX_Sales_ClientId" ON "Sales" ("ClientId");

CREATE INDEX "IX_Sales_CreatedByUserId" ON "Sales" ("CreatedByUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260727184440_AddBusinessSettings', '8.0.0');

COMMIT;

