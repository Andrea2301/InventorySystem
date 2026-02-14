using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class SupplierFormViewModel : ViewModelBase
    {
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

        public SupplierFormViewModel(Supplier supplier = null)
        {
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

        private void ExecuteSaveCommand(object obj)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    if (Supplier.Id == 0)
                    {
                        db.Suppliers.Add(Supplier);
                    }
                    else
                    {
                        db.Suppliers.Update(Supplier);
                    }
                    db.SaveChanges();
                }

                if (obj is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj)
        {
            if (obj is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}
