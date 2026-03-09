using FontAwesome.Sharp;
using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using InventorySystem.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _iconChar;

        public ViewModelBase CurrentChildView
        {
            get { return _currentChildView; }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public IconChar IconChar
        {
            get { return _iconChar; }
            set
            {
                _iconChar = value;
                OnPropertyChanged(nameof(IconChar));
            }
        }

        //--> Commands
        public ICommand ShowHomeViewCommand { get;}
        public ICommand ShowClientViewCommand { get;}
        public ICommand ShowProductsViewCommand { get;}
        public ICommand ShowSaleViewCommand { get;}
        public ICommand ShowSalesHistoryViewCommand { get;}
        public ICommand ShowSupplierViewCommand { get; }
        public ICommand ShowReportsViewCommand { get; }
        public ICommand ShowMaintenanceViewCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // Apply any pending migrations on startup - REMOVED: Managed in App.xaml.cs via EnsureCreated
            // using (var scope = _serviceProvider.CreateScope())
            // {
            //     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //     db.Database.Migrate();
            // }

            // Initialize QuestPDF license globally
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowClientViewCommand = new ViewModelCommand(ExecuteShowClientViewCommand);
            ShowProductsViewCommand = new ViewModelCommand(ExecuteShowProductsViewCommand);
            ShowSaleViewCommand = new ViewModelCommand(ExecuteShowSaleViewCommand); 
            ShowSalesHistoryViewCommand = new ViewModelCommand(ExecuteShowSalesHistoryViewCommand);
            ShowSupplierViewCommand = new ViewModelCommand(ExecuteShowSupplierViewCommand);
            ShowReportsViewCommand = new ViewModelCommand(ExecuteShowReportsViewCommand);
            ShowMaintenanceViewCommand = new ViewModelCommand(ExecuteShowMaintenanceViewCommand);
            
            //Default view
            ExecuteShowHomeViewCommand(null);
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<HomeViewModel>();
            Caption = "Dashboard";
            IconChar = IconChar.Home;
        }

        private void ExecuteShowClientViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<ClientViewModel>();
            Caption = "Clients";
            IconChar = IconChar.UserGroup; 
        }

        private void ExecuteShowProductsViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<ProductsViewModel>();
            Caption = "Products";
            IconChar = IconChar.Boxes;
        }

        private void ExecuteShowSaleViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<SaleViewModel>();
            Caption = "Sales";
            IconChar = IconChar.FileInvoiceDollar;
        }

        private void ExecuteShowSalesHistoryViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<SalesHistoryViewModel>();
            Caption = "Sales History";
            IconChar = IconChar.History;
        }

        private void ExecuteShowSupplierViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<SupplierViewModel>();
            Caption = "Suppliers";
            IconChar = IconChar.Truck;
        }

        private void ExecuteShowReportsViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<ReportsViewModel>();
            Caption = "Reports";
            IconChar = IconChar.ChartPie;
        }

        private void ExecuteShowMaintenanceViewCommand(object obj)
        {
            CurrentChildView = _serviceProvider.GetRequiredService<MaintenanceViewModel>();
            Caption = "Maintenance";
            IconChar = IconChar.Database;
        }
    }
}

