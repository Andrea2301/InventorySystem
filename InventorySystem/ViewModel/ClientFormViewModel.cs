using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using InventorySystem.Services;

namespace InventorySystem.ViewModel
{
    public class ClientFormViewModel : ViewModelBase
    {
        private readonly IClientService _clientService;
        private Client _client;
        private bool _isEditMode;

        public Client Client
        {
            get => _client;
            set
            {
                _client = value;
                OnPropertyChanged(nameof(Client));
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(nameof(IsEditMode)); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RequestClose;

        public ClientFormViewModel(IClientService clientService, Client? client = null)
        {
            _clientService = clientService;
            IsEditMode = client != null;
            
            if (client == null)
            {
                Client = new Client { IsActive = true };
            }
            else
            {
                // Shallow copy to avoid live updates before save
                Client = new Client
                {
                    Id = client.Id,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Email = client.Email,
                    PhoneNumber = client.PhoneNumber,
                    DocumentNumber = client.DocumentNumber,
                    IsActive = client.IsActive,
                    CreatedAt = client.CreatedAt
                };
            }

            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private async void ExecuteSaveCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Client.FirstName) || string.IsNullOrWhiteSpace(Client.LastName))
            {
                MessageBox.Show("First Name and Last Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    await _clientService.UpdateAsync(Client);
                }
                else
                {
                    await _clientService.AddAsync(Client);
                }

                MessageBox.Show("Client saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object obj)
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
