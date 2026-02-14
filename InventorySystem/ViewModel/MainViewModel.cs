using FontAwesome.Sharp;
using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        
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
        public ICommand ShowInformsViewCommand { get; }

        public MainViewModel()
        {
            // Initialize QuestPDF license globally
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowClientViewCommand = new ViewModelCommand(ExecuteShowClientViewCommand);
            ShowProductsViewCommand = new ViewModelCommand(ExecuteShowProductsViewCommand);
            ShowSaleViewCommand = new ViewModelCommand(ExecuteShowSaleViewCommand); 
            ShowSalesHistoryViewCommand = new ViewModelCommand(ExecuteShowSalesHistoryViewCommand);
            ShowSupplierViewCommand = new ViewModelCommand(ExecuteShowSupplierViewCommand);
            ShowInformsViewCommand = new ViewModelCommand(ExecuteShowInformsViewCommand);
            //Default view
            ExecuteShowHomeViewCommand(null);
           


        }


        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Dashboard";
            IconChar = IconChar.Home;
        }

        private void ExecuteShowClientViewCommand(object obj)
        {
            CurrentChildView = new ClientViewModel();
            Caption = "Clients";
            IconChar = IconChar.UserGroup; 
        }


        private void ExecuteShowProductsViewCommand(object obj)
        {
            CurrentChildView = new ProductsViewModel();
            Caption = "Products";
            IconChar = IconChar.Boxes;
        }

        private void ExecuteShowSaleViewCommand(object obj)
        {
            CurrentChildView = new SaleViewModel();
            Caption = "Sales";
            IconChar = IconChar.FileInvoiceDollar;
        }

        private void ExecuteShowSalesHistoryViewCommand(object obj)
        {
            CurrentChildView = new SalesHistoryViewModel();
            Caption = "Sales History";
            IconChar = IconChar.History;
        }

        private void ExecuteShowSupplierViewCommand(object obj)
        {
            CurrentChildView = new SupplierViewModel();
            Caption = "Suppliers";
            IconChar = IconChar.Truck;
        }

        private void ExecuteShowInformsViewCommand(object obj)
        {
            CurrentChildView = new InformsViewModel();
            Caption = "Informs";
            IconChar = IconChar.FilePen;
        }

    }
}

