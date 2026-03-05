using InventorySystem.ViewModel.Base;
using InventorySystem.Services;
using InventorySystem.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

namespace InventorySystem.ViewModel
{
    public class ProductsViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;

        public ICommand OpenProductFormCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        public ProductsViewModel(IProductService productService, IDialogService dialogService, IMessageService messageService)
        {
            _productService = productService;
            _dialogService = dialogService;
            _messageService = messageService;

            OpenProductFormCommand = new ViewModelCommand(ExecuteOpenProductFormCommand);
            EditProductCommand = new ViewModelCommand(ExecuteEditProductCommand);
            DeleteProductCommand = new ViewModelCommand(ExecuteDeleteProductCommand);
            _ = LoadProductsAsync();
        }

        private void ExecuteEditProductCommand(object obj)
        {
            if (obj is Product product)
            {
                var viewModel = new ProductFormViewModel(_productService, product);
                viewModel.RequestClose += (s, e) => _ = LoadProductsAsync();
                _dialogService.ShowDialog(viewModel);
            }
        }

        private async void ExecuteDeleteProductCommand(object obj)
        {
            if (obj is Product product)
            {
                if (_messageService.ShowConfirmation($"Are you sure you want to delete '{product.Name}'?", "Confirm Delete"))
                {
                    try
                    {
                        IsLoading = true;
                        await _productService.DeleteAsync(product.Id);
                        await LoadProductsAsync();
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Error deleting product: {ex.Message}");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                var list = await _productService.GetAllAsync();
                Products = new ObservableCollection<Product>(list);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading products: {ex.Message}");
                Products = new ObservableCollection<Product>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteOpenProductFormCommand(object obj)
        {
            var viewModel = new ProductFormViewModel(_productService);
            viewModel.RequestClose += (s, e) => _ = LoadProductsAsync();
            _dialogService.ShowDialog(viewModel);
        }
    }
}
