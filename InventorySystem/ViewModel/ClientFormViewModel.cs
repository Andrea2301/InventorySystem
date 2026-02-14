using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace InventorySystem.ViewModel
{
    public class ClientFormViewModel : ViewModelBase
    {
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

        public ClientFormViewModel(Client client = null)
        {
            IsEditMode = client != null;
            Client = client ?? new Client { IsActive = true };

            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private void ExecuteSaveCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(Client.FirstName) || string.IsNullOrWhiteSpace(Client.LastName))
            {
                MessageBox.Show("First Name and Last Name are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new AppDbContext())
            {
                if (IsEditMode)
                {
                    db.Clients.Update(Client);
                }
                else
                {
                    db.Clients.Add(Client);
                }
                db.SaveChanges();
            }

            if (obj is Window window)
            {
                window.DialogResult = true;
                window.Close();
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
