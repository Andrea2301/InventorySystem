using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Windows;

namespace InventorySystem.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _mappings;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _mappings = new Dictionary<Type, Type>();
            RegisterDialogs();
        }

        private void RegisterDialogs()
        {
            // Register mappings between ViewModels and Views
            _mappings.Add(typeof(ViewModel.ProductFormViewModel), typeof(Views.ProductFormView));
            _mappings.Add(typeof(ViewModel.ClientFormViewModel), typeof(Views.ClientFormView));
            _mappings.Add(typeof(ViewModel.SupplierFormViewModel), typeof(Views.SupplierFormView));
            _mappings.Add(typeof(Models.Sale), typeof(Views.SaleDetailView));
            _mappings.Add(typeof(ViewModel.ImportFileViewModel), typeof(Views.ImportFileView));
            
        }

        public void ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            if (!_mappings.TryGetValue(typeof(TViewModel), out var viewType))
            {
                throw new ArgumentException($"No view registered for ViewModel type {typeof(TViewModel)}");
            }

            var view = (Window)Activator.CreateInstance(viewType)!;
            view.DataContext = viewModel;

            // Handle closing if the ViewModel has a RequestClose event or similar
            if (viewModel is ICloseableViewModel closeable)
            {
                closeable.RequestClose += (s, e) => view.Close();
            }

            view.ShowDialog();
        }
        
        public string OpenFile(string filter, string title = "Select File")
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Title = title
            };
            
            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }

        public string SaveFile(string filter, string defaultFileName = "", string title = "Save File")
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName,
                Title = title
            };
            
            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }
    }
}
