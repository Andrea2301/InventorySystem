<div align="center">

# Inventory Management System

**A  Desktop Inventory and Point of Sale (POS) solution built with WPF and .NET 8**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Desktop-0078D4?logo=windows)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?logo=sqlite)](https://www.sqlite.org/)
[![Version](https://img.shields.io/badge/Version-1.0.3-blue.svg)](https://github.com/Andrea2301/InventorySystem/releases)
[![Status](https://img.shields.io/badge/Status-Active-success.svg)](https://github.com/Andrea2301/InventorySystem)
[![Maintenance](https://img.shields.io/badge/Maintained-Yes-brightgreen.svg)](https://github.com/Andrea2301/InventorySystem/graphs/commit-activity)

</div>

---

## Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Usage Guide](#usage-guide)
- [User Experience & Design](#user-experience--design)
- [Development & Quality](#development--quality)
- [Author](#author)

---

## Overview

This project provides a comprehensive inventory management system tailored for small to medium-sized enterprises. The application is built using modern software engineering patterns, including MVVM (Model-View-ViewModel), Dependency Injection, and the Repository Pattern, to ensure a robust, maintainable, and scalable codebase.

The system emphasizes a desktop-first approach, prioritizing:
- **Offline Reliability**: Uninterrupted operation without cloud dependency.
- **Data Sovereignty**: Local storage using SQLite for full control over business data.
- **Performance**: High-speed, responsive local interface.

---

## Interface

### Dashboard Preview
The dashboard provides a high-level view of business operations, including real-time KPIs and critical inventory status.

![Dashboard](./InventorySystem/Assets/Screenshots/Dashboard.png)
*Figure 1: Main dashboard with sales analytics and stock monitoring.*

---

## Key Features

### Inventory and Catalog Management
- Real-time stock tracking with automated deduction during sales.
- Categorized product management with support for active/inactive status.
- Visual alerts for critical stock levels based on configurable thresholds.
- Bulk filtering and search capabilities.

### Customer and Supplier Management
- Detailed profiles for customers and suppliers.
- Integration with MiniExcel for bulk customer data import (Excel/CSV).
- Complete transaction history tracking per entity.
- Support for anonymous sales (General Public) to streamline operations.

### Point of Sale (POS)
- Dynamic shopping cart system with real-time tax and total calculations.
- Integrated product search for rapid item selection.
- Atomic transaction processing to maintain data integrity.
- Immediate inventory synchronization upon sale finalization.

### Analytics and Reporting
- Revenue performance tracking with interactive charts.
- Real-time calculation of total inventory value.
- Professional invoice generation in PDF format via QuestPDF.
- Data-driven summaries for business decision-making.

---

## Technology Stack

### Core Frameworks
- **Framework**: .NET 8 (WPF)
- **Language**: C# 12
- **Persistence**: SQLite with Entity Framework Core 8
- **DI Container**: Microsoft.Extensions.DependencyInjection

### External Dependencies
- **Data Visualization**: LiveCharts.Wpf
- **UI Components**: HandyControl, FontAwesome.Sharp
- **Document Generation**: QuestPDF
- **Data Processing**: MiniExcel

### Design Patterns
- Model-View-ViewModel (MVVM)
- Repository Pattern
- Dependency Injection
- Command & Observer Patterns
- Service-Oriented Architecture

---

## Project Structure

```text
InventorySystem/
├── Models/                 # Domain entities and core data structures
├── ViewModels/             # Business logic and presentation state
├── Views/                  # XAML UI definitions
├── Services/               # Business logic and cross-cutting concerns
│   ├── ProductService.cs
│   ├── ClientService.cs
│   ├── SaleService.cs
│   └── Export/             # PDF and reporting services
├── Data/                   # EF Core Context and migrations
├── Commands/               # Reusable ICommand implementations
├── Converters/             # Data-binding value converters
├── Helpers/                # Utility classes and behaviors
├── Assets/                 # Resources, styles, and static files
└── Shell/                  # Main application window and entry logic
```

---

## Getting Started

### For End Users

1. **Download**: Obtain the latest package from the [Releases](https://github.com/Andrea2301/InventorySystem/releases) page.
2. **Setup**:
   - Extract the `InventorySystem-v1.0.3-win-x64.zip` archive.
   - Run the `install.ps1` script (PowerShell) to create a desktop shortcut.
3. **Run**: Launch `InventorySystem.exe`. All required dependencies are bundled with the application.

### For Developers

**Prerequisites**:
- .NET 8 SDK
- Visual Studio 2022 or VS Code with C# Dev Kit

**Build Steps**:
1. Clone the repository: `git clone https://github.com/Andrea2301/InventorySystem.git`
2. Restore packages: `dotnet restore`
3. Build the solution: `dotnet build --configuration Release`
4. Run the project: `dotnet run --project InventorySystem`

---

## User Experience & Design

The application implements high-end design principles to provide a premium feel:
- **Visual Depth**: Subtle use of transparency and consistent depth through shadows and layers.
- **Interactive Feedback**: Real-time input validation with clear visual status indicators.
- **Navigation**: Structured sidebar navigation with modern iconography.
- **Performance**: Asynchronous data operations to maintain UI responsiveness.

---

## Development & Quality

This project adheres to rigorous development standards:
- **SOLID Principles**: Ensuring clean, decoupled, and testable code.
- **Async Programming**: Utilizing `async/await` throughout the service and viewmodel layers.
- **Error Handling**: Comprehensive exception management with user-facing notifications.
- **Data Integrity**: Enforced via Entity Framework constraints and business layer validation.

---

## Author

**Andrea Ospino** - [github.com/Andrea2301](https://github.com/Andrea2301)  
For inquiries or support, please contact: [andreaospino323@gmail.com](mailto:andreaospino323@gmail.com)

---

<div align="center">

Built with .NET 8 and Windows Presentation Foundation

</div>
