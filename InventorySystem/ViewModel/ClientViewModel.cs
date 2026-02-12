using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using InventorySystem.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class ClientViewModel: ViewModelBase
    {
        public ICommand OpenClientFormCommand { get; }
        public ICommand OpenImportExcelCommand { get; }

        public ClientViewModel()
        {
            OpenClientFormCommand = new ViewModelCommand(ExecuteOpenClientFormCommand);
            OpenImportExcelCommand = new ViewModelCommand(ExecuteOpenImportExcelCommand);
        }

        private void ExecuteOpenImportExcelCommand(object obj)
        {
            var viewModel = new ImportFileViewModel();
            var view = new ImportFileView { DataContext = viewModel };

            bool? result = view.ShowDialog();

            if (result == true)
            {
                string filePath = viewModel.SelectedFilePath;
                // TODO: Call Service to Process Excel File
                // MessageBox.Show($"File selected: {filePath}");
            }
        }

        private void ExecuteOpenClientFormCommand(object obj)
        {
            var viewModel = new ClientFormViewModel();
            var view = new ClientFormView { DataContext = viewModel };
            
            bool? result = view.ShowDialog();

            if (result == true)
            {
                // Refresh list or add the new client to the collection
                // For now, we just mock the success interaction
            }
        }
    }
}
