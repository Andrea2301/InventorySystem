using System;
using System.Windows.Input;
using InventorySystem.ViewModel.Base;
using InventorySystem.Services;
using InventorySystem.Models;
using System.Threading.Tasks;

namespace InventorySystem.ViewModel
{
    public class MaintenanceViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IBusinessSettingService _businessSettingService;

        private string _databasePath;
        private string _databaseSize;
        private bool _isBusy;

        // Business settings fields
        private BusinessSetting _settings;
        private string _companyName;
        private string _taxId;
        private string _address;
        private string _phone;
        private string _email;
        private decimal _taxPercentage;
        private string _currencySymbol;

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

        // Business settings properties
        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(); }
        }

        public string TaxId
        {
            get => _taxId;
            set { _taxId = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public decimal TaxPercentage
        {
            get => _taxPercentage;
            set { _taxPercentage = value; OnPropertyChanged(); }
        }

        public string CurrencySymbol
        {
            get => _currencySymbol;
            set { _currencySymbol = value; OnPropertyChanged(); }
        }

        public class LanguageItem
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public System.Collections.Generic.List<LanguageItem> LanguagesList { get; } = new System.Collections.Generic.List<LanguageItem>
        {
            new LanguageItem { Code = "es", Name = "Español" },
            new LanguageItem { Code = "en", Name = "English" }
        };

        private LanguageItem _selectedLanguage;
        public LanguageItem SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value && value != null)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged(nameof(SelectedLanguage));
                    App.ChangeLanguage(value.Code);
                }
            }
        }

        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        public MaintenanceViewModel(
            IDatabaseService databaseService, 
            IDialogService dialogService, 
            IMessageService messageService,
            IBusinessSettingService businessSettingService)
        {
            _databaseService = databaseService;
            _dialogService = dialogService;
            _messageService = messageService;
            _businessSettingService = businessSettingService;

            _selectedLanguage = LanguagesList.Find(l => l.Code == App.CurrentLanguage) ?? LanguagesList[0];

            BackupCommand = new ViewModelCommand(async _ => await ExecuteBackup());
            RestoreCommand = new ViewModelCommand(async _ => await ExecuteRestore());
            SaveSettingsCommand = new ViewModelCommand(async _ => await ExecuteSaveSettings());

            LoadInfo();
            _ = LoadSettingsAsync();
        }

        private void LoadInfo()
        {
            DatabasePath = _databaseService.GetDatabasePath();
            long bytes = _databaseService.GetDatabaseSize();
            DatabaseSize = $"{(bytes / 1024.0 / 1024.0):F2} MB";
        }

        private async Task LoadSettingsAsync()
        {
            IsBusy = true;
            try
            {
                _settings = await _businessSettingService.GetSettingsAsync();
                CompanyName = _settings.CompanyName;
                TaxId = _settings.TaxId;
                Address = _settings.Address;
                Phone = _settings.Phone;
                Email = _settings.Email;
                TaxPercentage = _settings.TaxPercentage;
                CurrencySymbol = _settings.CurrencySymbol;
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading business settings: {ex.Message}", "Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteSaveSettings()
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                _messageService.ShowWarning(
                    GetStringResource("MaintSettingsCompanyNameRequired", "Company Name is required."),
                    GetStringResource("TitleWarning", "Warning"));
                return;
            }

            IsBusy = true;
            try
            {
                _settings.CompanyName = CompanyName;
                _settings.TaxId = TaxId ?? string.Empty;
                _settings.Address = Address ?? string.Empty;
                _settings.Phone = Phone ?? string.Empty;
                _settings.Email = Email ?? string.Empty;
                _settings.TaxPercentage = TaxPercentage;
                _settings.CurrencySymbol = CurrencySymbol ?? "$";

                bool success = await _businessSettingService.SaveSettingsAsync(_settings);
                if (success)
                {
                    _messageService.ShowInfo(
                        GetStringResource("MaintSettingsSaveSuccess", "Settings saved successfully."),
                        GetStringResource("TitleInformation", "Success"));
                }
                else
                {
                    _messageService.ShowError(
                        GetStringResource("MaintSettingsSaveError", "Error saving settings."),
                        GetStringResource("TitleError", "Error"));
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error: {ex.Message}", "Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetStringResource(string key, string fallback)
        {
            if (System.Windows.Application.Current?.TryFindResource(key) is string resource)
            {
                return resource;
            }
            return fallback;
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
                _messageService.ShowInfo(
                    GetStringResource("MaintBackupSuccess", "The backup has been created successfully."),
                    GetStringResource("TitleInformation", "Backup Successful"));
            }
            else
            {
                _messageService.ShowError(
                    GetStringResource("MaintBackupError", "An error occurred while creating the backup. Make sure the location is accessible."),
                    GetStringResource("TitleError", "Backup Error"));
            }
        }

        private async Task ExecuteRestore()
        {
            bool confirm = _messageService.ShowConfirmation(
                GetStringResource("MaintRestoreWarning", "WARNING: Restoring a backup will replace all current data. This action cannot be undone.\n\nDo you want to continue?"),
                GetStringResource("TitleConfirm", "Confirm Restoration"));

            if (!confirm) return;

            string path = _dialogService.OpenFile("SQLite Database (*.db)|*.db");

            if (string.IsNullOrEmpty(path)) return;

            IsBusy = true;
            bool success = await _databaseService.RestoreAsync(path);
            IsBusy = false;

            if (success)
            {
                _messageService.ShowInfo(
                    GetStringResource("MaintRestoreSuccess", "Database successfully restored.\n\nIMPORTANT: The application must be restarted to apply these changes correctly."),
                    GetStringResource("TitleInformation", "Restoration Success"));
                LoadInfo();
                _ = LoadSettingsAsync();
            }
            else
            {
                _messageService.ShowError(
                    GetStringResource("MaintRestoreError", "Database restoration failed.\n\nThis could be because the file is not a valid backup or it is currently being used by another process."),
                    GetStringResource("TitleError", "Restoration Error"));
            }
        }
    }
}
