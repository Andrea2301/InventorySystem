using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class ClientFormViewModel : ViewModelBase
    {
        private Client _client;

        public Client Client
        {
            get => _client;
            set
            {
                _client = value;
                OnPropertyChanged(nameof(Client));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ClientFormViewModel(Client client = null)
        {
            // If editing, use existing client; otherwise create new
            Client = client ?? new Client();

            SaveCommand = new ViewModelCommand(ExecuteSaveCommand);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private void ExecuteSaveCommand(object obj)
        {
            // TODO: Validate and Save to Database/Repository
            // For now, just close the window with DialogResult = true
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
