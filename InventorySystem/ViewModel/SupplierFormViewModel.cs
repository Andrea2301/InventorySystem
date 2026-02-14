using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services;
using InventorySystem.ViewModel.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class SupplierFormViewModel : ViewModelBase
    {
        private readonly ISupplierService _supplierService;
        private Supplier _supplier;
        private string _title;

        public Supplier Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                OnPropertyChanged(nameof(Supplier));
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RequestClose;

        public SupplierFormViewModel(ISupplierService supplierService, Supplier? supplier = null)
        {
            _supplierService = supplierService;
            if (supplier == null)
            {
                Supplier = new Supplier();
                Title = "New Supplier";
            }
            else
            {
                // Create a copy to avoid immediate changes to the original list
                Supplier = new Supplier
                {
                    Id = supplier.Id,
                    DocumentNumber = supplier.DocumentNumber,
                    FirstName = supplier.FirstName,
                    LastName = supplier.LastName,
                    Address = supplier.Address,
                    CompanyName = supplier.CompanyName,
                    Email = supplier.Email,
                    PhoneNumber = supplier.PhoneNumber,
                    Website = supplier.Website,
                    Category = supplier.Category,
                    IsActive = supplier.IsActive,
                    CreatedAt = supplier.CreatedAt
                };
                Title = "Edit Supplier";
            }

            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private async void ExecuteSaveCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Supplier.CompanyName))
            {
                MessageBox.Show("Company Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (Supplier.Id == 0)
                {
                    await _supplierService.AddAsync(Supplier);
                }
                else
                {
                    await _supplierService.UpdateAsync(Supplier);
                }

                MessageBox.Show("Supplier saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
