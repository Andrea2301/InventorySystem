using System.Windows;

namespace InventorySystem.Services
{
    public interface IMessageService
    {
        void ShowInfo(string message, string title = "Information");
        void ShowWarning(string message, string title = "Warning");
        void ShowError(string message, string title = "Error");
        bool ShowConfirmation(string message, string title = "Confirm");
    }

    public class MessageService : IMessageService
    {
        public void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
