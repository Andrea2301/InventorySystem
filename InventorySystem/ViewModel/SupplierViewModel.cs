using InventorySystem.Models;
using InventorySystem.Data;
using InventorySystem.ViewModel.Base;
using InventorySystem.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class SupplierViewModel : ViewModelBase
    {
        private ObservableCollection<Supplier> _suppliers;
        private string _searchText;

        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set
            {
                _suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ExecuteSearchCommand(null);
            }
        }

        public ICommand ShowAddSupplierCommand { get; }
        public ICommand ShowEditSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand SearchCommand { get; }

        public SupplierViewModel()
        {
            ShowAddSupplierCommand = new ViewModelCommand(ExecuteShowAddSupplierCommand);
            ShowEditSupplierCommand = new ViewModelCommand(ExecuteShowEditSupplierCommand);
            DeleteSupplierCommand = new ViewModelCommand(ExecuteDeleteSupplierCommand);
            SearchCommand = new ViewModelCommand(ExecuteSearchCommand);

            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            using (var db = new AppDbContext())
            {
                var list = db.Suppliers.ToList();
                Suppliers = new ObservableCollection<Supplier>(list);
            }
        }

        private void ExecuteShowAddSupplierCommand(object obj)
        {
            var viewModel = new SupplierFormViewModel();
            var view = new SupplierFormView { DataContext = viewModel };

            if (view.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }

        private void ExecuteShowEditSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                var viewModel = new SupplierFormViewModel(supplier);
                var view = new SupplierFormView { DataContext = viewModel };

                if (view.ShowDialog() == true)
                {
                    LoadSuppliers();
                }
            }
        }

        private void ExecuteDeleteSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {supplier.CompanyName}?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new AppDbContext())
                    {
                        db.Suppliers.Remove(supplier);
                        db.SaveChanges();
                    }
                    LoadSuppliers();
                }
            }
        }

        private void ExecuteSearchCommand(object obj)
        {
            using (var db = new AppDbContext())
            {
                var query = db.Suppliers.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(s => 
                        s.CompanyName.Contains(SearchText) || 
                        s.FirstName.Contains(SearchText) || 
                        s.LastName.Contains(SearchText) ||
                        s.Category.Contains(SearchText));
                }

                Suppliers = new ObservableCollection<Supplier>(query.ToList());
            }
        }
    }
}
