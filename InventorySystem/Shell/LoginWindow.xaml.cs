using InventorySystem.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.Shell
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnLoginSuccess = OnLoginSuccess;

            Loaded += (s, e) => TxtUsername.Focus();
        }

        private void OnLoginSuccess()
        {
            var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.Password = PwdPassword.Password;
                if (_viewModel.LoginCommand.CanExecute(null))
                    _viewModel.LoginCommand.Execute(null);
            }
        }

        private void PwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PwdPassword.Password;
        }
    }
}
