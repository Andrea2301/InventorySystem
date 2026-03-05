using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using InventorySystem.Services;

namespace InventorySystem.ViewModel
{
    public class ClientFormViewModel : ViewModelBase, ICloseableViewModel
    {
        private readonly IClientService _clientService;
        private int _id;
        private string _firstName;
        private string _lastName;
        private string _email;
        private string _phoneNumber;
        private string _documentNumber;
        private string _address;
        private bool _isActive;
        private DateTime _createdAt;
        private bool _isEditMode;

        public bool IsEditMode { get => _isEditMode; set { _isEditMode = value; OnPropertyChanged(nameof(IsEditMode)); } }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); ValidateProperty(nameof(FirstName), value); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); ValidateProperty(nameof(LastName), value); }
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

        public string DocumentNumber
        {
            get => _documentNumber;
            set { _documentNumber = value; OnPropertyChanged(nameof(DocumentNumber)); ValidateProperty(nameof(DocumentNumber), value); }
        }

        public string Address { get => _address; set { _address = value; OnPropertyChanged(nameof(Address)); } }
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(nameof(IsActive)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RequestClose;

        public ClientFormViewModel(IClientService clientService, Client? client = null)
        {
            _clientService = clientService;
            IsEditMode = client != null;
            
            if (client == null)
            {
                _isActive = true;
                _createdAt = DateTime.UtcNow;
            }
            else
            {
                _id = client.Id;
                _firstName = client.FirstName;
                _lastName = client.LastName;
                _email = client.Email;
                _phoneNumber = client.PhoneNumber;
                _documentNumber = client.DocumentNumber;
                _address = client.Address;
                _isActive = client.IsActive;
                _createdAt = client.CreatedAt;
            }

            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);

            if (client != null) ValidateAll();
        }

        private void ValidateProperty(string propertyName, object value)
        {
            ClearErrors(propertyName);
            string valStr = value?.ToString();

            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.IsNullOrWhiteSpace(valStr)) AddError(propertyName, "First name is required.");
                    break;
                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(valStr)) AddError(propertyName, "Last name is required.");
                    break;
                case nameof(DocumentNumber):
                    if (string.IsNullOrWhiteSpace(valStr)) AddError(propertyName, "Document number is required.");
                    break;
                case nameof(Email):
                    if (!string.IsNullOrWhiteSpace(valStr) && !System.Text.RegularExpressions.Regex.IsMatch(valStr, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Invalid email format.");
                    break;
            }
        }

        private void ValidateAll()
        {
            ValidateProperty(nameof(FirstName), FirstName);
            ValidateProperty(nameof(LastName), LastName);
            ValidateProperty(nameof(DocumentNumber), DocumentNumber);
            ValidateProperty(nameof(Email), Email);
        }

        private async void ExecuteSaveCommand(object obj)
        {
            ValidateAll();
            if (HasErrors) return;

            try
            {
                var client = new Client
                {
                    Id = _id,
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    DocumentNumber = DocumentNumber,
                    Address = Address,
                    IsActive = IsActive,
                    CreatedAt = _createdAt
                };

                if (IsEditMode) await _clientService.UpdateAsync(client);
                else await _clientService.AddAsync(client);

                MessageBox.Show("Client saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj) => RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
