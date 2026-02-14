using InventorySystem.Models;
using InventorySystem.Data;
using InventorySystem.ViewModel.Base;
using InventorySystem.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using InventorySystem.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.ViewModel
{
    public class SupplierViewModel : ViewModelBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IServiceProvider _serviceProvider;
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
                _ = SearchSuppliersAsync();
            }
        }

        public ICommand ShowAddSupplierCommand { get; }
        public ICommand ShowEditSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand SearchCommand { get; }

        public SupplierViewModel(ISupplierService supplierService, IServiceProvider serviceProvider)
        {
            _supplierService = supplierService;
            _serviceProvider = serviceProvider;

            ShowAddSupplierCommand = new ViewModelCommand(ExecuteShowAddSupplierCommand);
            ShowEditSupplierCommand = new ViewModelCommand(ExecuteShowEditSupplierCommand);
            DeleteSupplierCommand = new ViewModelCommand(ExecuteDeleteSupplierCommand);
            SearchCommand = new ViewModelCommand(async _ => await SearchSuppliersAsync());

            _ = LoadSuppliersAsync();
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var list = await _supplierService.GetAllAsync();
                Suppliers = new ObservableCollection<Supplier>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SearchSuppliersAsync()
        {
            try
            {
                var list = await _supplierService.SearchAsync(SearchText);
                Suppliers = new ObservableCollection<Supplier>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteShowAddSupplierCommand(object obj)
        {
            var viewModel = _serviceProvider.GetRequiredService<SupplierFormViewModel>();
            var view = new SupplierFormView { DataContext = viewModel };

            viewModel.RequestClose += (s, e) => 
            {
                view.Close();
                _ = LoadSuppliersAsync();
            };

            view.ShowDialog();
        }

        private void ExecuteShowEditSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                var viewModel = new SupplierFormViewModel(_supplierService, supplier);
                var view = new SupplierFormView { DataContext = viewModel };

                viewModel.RequestClose += (s, e) => 
                {
                    view.Close();
                    _ = LoadSuppliersAsync();
                };

                view.ShowDialog();
            }
        }

        private async void ExecuteDeleteSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {supplier.CompanyName}?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _supplierService.DeleteAsync(supplier.Id);
                        await LoadSuppliersAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
