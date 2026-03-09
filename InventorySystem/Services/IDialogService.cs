using System;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IDialogService
    {
        void ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class;
        string OpenFile(string filter, string title = "Select File");
        string SaveFile(string filter, string defaultFileName = "", string title = "Save File");
    }
}
