using System;
using System.Threading.Tasks;

namespace InventorySystem.Services
{
    public interface IDialogService
    {
        void ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class;
    }
}
