using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services;
using InventorySystem.ViewModel.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class SupplierFormViewModel : ViewModelBase, ICloseableViewModel
    {
        private readonly ISupplierService _supplierService;
        private int _id;
        private string _documentNumber;
        private string _firstName;
        private string _lastName;
        private string _companyName;
        private string _email;
        private string _phoneNumber;
        private string _website;
        private string _address;
        private string _category;
        private bool _isActive;
        private DateTime _createdAt;
        private string _title;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(nameof(Title)); } }

        public string DocumentNumber
        {
            get => _documentNumber;
            set { _documentNumber = value; OnPropertyChanged(nameof(DocumentNumber)); ValidateProperty(nameof(DocumentNumber), value); }
        }

        public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(nameof(FirstName)); } }
        public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(nameof(LastName)); } }

        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(nameof(CompanyName)); ValidateProperty(nameof(CompanyName), value); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); ValidateProperty(nameof(Email), value); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); ValidateProperty(nameof(PhoneNumber), value); }
        }

        public string Website { get => _website; set { _website = value; OnPropertyChanged(nameof(Website)); } }
        public string Address { get => _address; set { _address = value; OnPropertyChanged(nameof(Address)); } }
        public string Category { get => _category; set { _category = value; OnPropertyChanged(nameof(Category)); } }
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RequestClose;

        public SupplierFormViewModel(ISupplierService supplierService, Supplier? supplier = null)
        {
            _supplierService = supplierService;
            if (supplier == null)
            {
                Title = "New Supplier";
                _isActive = true;
                _createdAt = DateTime.UtcNow;
            }
            else
            {
                _id = supplier.Id;
                _documentNumber = supplier.DocumentNumber;
                _firstName = supplier.FirstName;
                _lastName = supplier.LastName;
                _companyName = supplier.CompanyName;
                _email = supplier.Email;
                _phoneNumber = supplier.PhoneNumber;
                _website = supplier.Website;
                _address = supplier.Address;
                _category = supplier.Category;
                _isActive = supplier.IsActive;
                _createdAt = supplier.CreatedAt;
                Title = "Edit Supplier";
            }

            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);

            if (supplier != null) ValidateAll();
        }

        private void ValidateProperty(string propertyName, object value)
        {
            ClearErrors(propertyName);
            string valStr = value?.ToString();

            switch (propertyName)
            {
                case nameof(CompanyName):
                    if (string.IsNullOrWhiteSpace(valStr)) AddError(propertyName, "Company Name is required.");
                    break;
                case nameof(DocumentNumber):
                    if (string.IsNullOrWhiteSpace(valStr)) AddError(propertyName, "Document Number is required.");
                    break;
                case nameof(Email):
                    if (!string.IsNullOrWhiteSpace(valStr) && !System.Text.RegularExpressions.Regex.IsMatch(valStr, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Invalid email format.");
                    break;
            }
        }

        private void ValidateAll()
        {
            ValidateProperty(nameof(CompanyName), CompanyName);
            ValidateProperty(nameof(DocumentNumber), DocumentNumber);
            ValidateProperty(nameof(Email), Email);
        }

        private async void ExecuteSaveCommand(object obj)
        {
            ValidateAll();
            if (HasErrors) return;

            try
            {
                var supplier = new Supplier
                {
                    Id = _id,
                    DocumentNumber = DocumentNumber,
                    FirstName = FirstName,
                    LastName = LastName,
                    CompanyName = CompanyName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Website = Website,
                    Address = Address,
                    Category = Category,
                    IsActive = IsActive,
                    CreatedAt = _createdAt
                };

                if (supplier.Id == 0) await _supplierService.AddAsync(supplier);
                else await _supplierService.UpdateAsync(supplier);

                MessageBox.Show("Supplier saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj) => RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
