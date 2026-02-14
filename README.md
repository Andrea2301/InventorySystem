# InventorySystem

A modern, high-performance Desktop Inventory and Point of Sale (POS) management system built with **WPF** and **.NET 8**.

## 🚀 Overview

This application has been architected to provide a robust, scalable, and professional experience for small to medium-sized businesses. It adheres to modern software design patterns such as **Dependency Injection (DI)**, the **Repository Pattern**, and **Asynchronous Programming** to ensure a smooth, non-blocking UI.

## ✨ Key Features

### 📦 Inventory & Product Management
*   **Real-time Stock Tracking**: Automatic inventory deduction upon sale finalization.
*   **Detailed Catalog**: Manage products with categories, prices, and descriptions.
*   **Visual Status**: Instant visibility of "In Stock", "Out of Stock", or "Inactive" items.

### 👥 Client & Customer Management
*   **Customer Profiles**: Detailed record keeping for all your clients.
*   **Bulk Import**: Quickly populate your database by importing client lists from **Excel/CSV** using MiniExcel.
*   **Integrated Selection**: Easily assign customers to sales for personalized transaction tracking.

### 🚛 Supplier Management
*   **Full CRUD support**: Manage your supply chain by keeping track of company details, contacts, and categories.

### 🛒 Point of Sale (POS)
*   **Dynamic Cart**: Add products to orders in real-time with subtotal and total calculations.
*   **Atomic Transactions**: Guaranteed data integrity through database transactions—stock updates and sale records succeed or fail together.
*   **Smart Customer Assignment**: Support for both registered customers and anonymous "General Public" sales.

### 📜 Sales History & PDF Invoicing
*   **Transactional Records**: Browse your complete sales history with detailed views of past orders.
*   **Professional Invoices**: Generate high-quality PDF receipts on-demand using **QuestPDF**.
*   **Financial Summary**: Real-time total amount calculations for every transaction.

## 🛠️ Technical Stack

*   **Framework**: .NET 8 (WPF)
*   **Database**: SQLite with Entity Framework Core
*   **Dependency Injection**: Microsoft.Extensions.DependencyInjection
*   **PDF Engine**: QuestPDF
*   **UI/UX**: 
    *   Modern, responsive design.
    *   **FontAwesome.Sharp** for high-quality iconography.
    *   **LiveCharts.Wpf** for future analytical dashboards.
*   **Data Processing**: MiniExcel (High-speed Excel reading/writing).

## 🚦 Getting Started

1.  **Prerequisites**: Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2.  **Clone/Download**: Access the project source code.
3.  **Run**: Execute `dotnet run` or start the project from Visual Studio.
4.  **Database**: The application uses a local `inventory.db` (SQLite), which is automatically initialized on the first run.

## 🏗️ Architectural Pattern

The project follows a clean separation of concerns:
*   **Models**: Plain C# classes with Data Annotations.
*   **Services (Repositories)**: Centralized logic for data access and business rules.
*   **ViewModels**: MVVM implementation using a custom base for property notification.
*   **Views**: XAML-based modern UI.
