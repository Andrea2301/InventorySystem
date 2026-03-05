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
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
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

        public SupplierViewModel(ISupplierService supplierService, IDialogService dialogService, IMessageService messageService)
        {
            _supplierService = supplierService;
            _dialogService = dialogService;
            _messageService = messageService;

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
                IsLoading = true;
                var list = await _supplierService.GetAllAsync();
                Suppliers = new ObservableCollection<Supplier>(list);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading suppliers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchSuppliersAsync()
        {
            try
            {
                IsLoading = true;
                var list = await _supplierService.SearchAsync(SearchText);
                Suppliers = new ObservableCollection<Supplier>(list);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error searching suppliers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteShowAddSupplierCommand(object obj)
        {
            var viewModel = new SupplierFormViewModel(_supplierService);
            viewModel.RequestClose += (s, e) => _ = LoadSuppliersAsync();
            _dialogService.ShowDialog(viewModel);
        }

        private void ExecuteShowEditSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                var viewModel = new SupplierFormViewModel(_supplierService, supplier);
                viewModel.RequestClose += (s, e) => _ = LoadSuppliersAsync();
                _dialogService.ShowDialog(viewModel);
            }
        }

        private async void ExecuteDeleteSupplierCommand(object obj)
        {
            if (obj is Supplier supplier)
            {
                if (_messageService.ShowConfirmation($"Are you sure you want to delete {supplier.CompanyName}?", "Confirm Delete"))
                {
                    try
                    {
                        IsLoading = true;
                        await _supplierService.DeleteAsync(supplier.Id);
                        await LoadSuppliersAsync();
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Error deleting supplier: {ex.Message}");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }
    }
}
