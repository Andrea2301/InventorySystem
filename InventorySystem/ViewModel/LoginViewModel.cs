using InventorySystem.Services;
using InventorySystem.ViewModel.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); ClearError(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); ClearError(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(nameof(HasError)); }
        }

        public ICommand LoginCommand { get; }

        public Action? OnLoginSuccess { get; set; }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new ViewModelCommand(_ => ExecuteLoginSync(), _ => !IsLoading);
        }

        private void ExecuteLoginSync()
        {
            _ = ExecuteLoginAsync();
        }

        private async Task ExecuteLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Please enter your username and password.");
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var user = await _authService.LoginAsync(Username, Password);

                if (user == null)
                {
                    ShowError("Invalid credentials. Please try again.");
                    return;
                }

                OnLoginSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError($"Connection error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }
    }
}
