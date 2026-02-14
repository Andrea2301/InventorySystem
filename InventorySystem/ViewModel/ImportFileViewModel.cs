using InventorySystem.ViewModel.Base;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class ImportFileViewModel : ViewModelBase
    {
        private string _selectedFilePath;
        private bool _isDragging;

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
                // Optional: Auto-close or enable "Import" button when file is selected
            }
        }

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                _isDragging = value;
                OnPropertyChanged(nameof(IsDragging));
            }
        }

        public ICommand BrowseFileCommand { get; }
        public ICommand ConfirmImportCommand { get; }
        public ICommand CancelCommand { get; }

        public ImportFileViewModel()
        {
            BrowseFileCommand = new ViewModelCommand(ExecuteBrowseFileCommand);
            ConfirmImportCommand = new ViewModelCommand(ExecuteConfirmImport, CanExecuteConfirmImport);
            CancelCommand = new ViewModelCommand(ExecuteCancelCommand);
        }

        private void ExecuteBrowseFileCommand(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel & CSV Files|*.xls;*.xlsx;*.xlsm;*.csv",
                Title = "Select an Excel or CSV File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
        }

        private bool CanExecuteConfirmImport(object obj)
        {
            return !string.IsNullOrEmpty(SelectedFilePath);
        }

        private void ExecuteConfirmImport(object obj)
        {
            // TODO: Process the file or return the path to the caller
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

        // Helper to validate dropped files
        public void HandleDroppedFile(string[] files)
        {
            if (files != null && files.Length > 0)
            {
                string file = files[0];
                string ext = System.IO.Path.GetExtension(file).ToLower();
                if (ext == ".xls" || ext == ".xlsx" || ext == ".xlsm" || ext == ".csv")
                {
                    SelectedFilePath = file;
                }
                else
                {
                    MessageBox.Show("Please select a valid Excel or CSV file.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
