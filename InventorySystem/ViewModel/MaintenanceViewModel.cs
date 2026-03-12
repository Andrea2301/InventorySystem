using System;
using System.Windows.Input;
using InventorySystem.ViewModel.Base;
using InventorySystem.Services;
using System.Threading.Tasks;

namespace InventorySystem.ViewModel
{
    public class MaintenanceViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;

        private string _databasePath;
        private string _databaseSize;
        private bool _isBusy;

        public string DatabasePath
        {
            get => _databasePath;
            set { _databasePath = value; OnPropertyChanged(); }
        }

        public string DatabaseSize
        {
            get => _databaseSize;
            set { _databaseSize = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }

        public MaintenanceViewModel(IDatabaseService databaseService, IDialogService dialogService, IMessageService messageService)
        {
            _databaseService = databaseService;
            _dialogService = dialogService;
            _messageService = messageService;

            BackupCommand = new ViewModelCommand(async _ => await ExecuteBackup());
            RestoreCommand = new ViewModelCommand(async _ => await ExecuteRestore());

            LoadInfo();
        }

        private void LoadInfo()
        {
            DatabasePath = _databaseService.GetDatabasePath();
            long bytes = _databaseService.GetDatabaseSize();
            DatabaseSize = $"{(bytes / 1024.0 / 1024.0):F2} MB";
        }

        private async Task ExecuteBackup()
        {
            string defaultName = $"Backup_Inventory_{DateTime.Now:yyyyMMdd_HHmm}.db";
            string path = _dialogService.SaveFile("SQLite Database (*.db)|*.db", defaultName);

            if (string.IsNullOrEmpty(path)) return;

            IsBusy = true;
            bool success = await _databaseService.BackupAsync(path);
            IsBusy = false;

            if (success)
            {
                _messageService.ShowInfo("The backup has been created successfully.", "Backup Successful");
            }
            else
            {
                _messageService.ShowError("An error occurred while creating the backup. Make sure the location is accessible.", "Backup Error");
            }
        }

        private async Task ExecuteRestore()
        {
            bool confirm = _messageService.ShowConfirmation(
                "WARNING: Restoring a backup will replace all current data. This action cannot be undone.\n\nDo you want to continue?", 
                "Confirm Restoration");

            if (!confirm) return;

            string path = _dialogService.OpenFile("SQLite Database (*.db)|*.db");

            if (string.IsNullOrEmpty(path)) return;

            IsBusy = true;
            bool success = await _databaseService.RestoreAsync(path);
            IsBusy = false;

            if (success)
            {
                _messageService.ShowInfo(
                    "Database successfully restored.\n\nIMPORTANT: The application must be restarted to apply these changes correctly.", 
                    "Restoration Success");
                LoadInfo();
            }
            else
            {
                _messageService.ShowError(
                    "Database restoration failed.\n\nThis could be because the file is not a valid backup or it is currently being used by another process.", 
                    "Restoration Error");
            }
        }
    }
}
