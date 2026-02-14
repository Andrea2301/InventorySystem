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
        private readonly IServiceProvider _serviceProvider;

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

        public ProductsViewModel(IProductService productService, IServiceProvider serviceProvider)
        {
            _productService = productService;
            _serviceProvider = serviceProvider;

            OpenProductFormCommand = new ViewModelCommand(ExecuteOpenProductFormCommand);
            EditProductCommand = new ViewModelCommand(ExecuteEditProductCommand);
            DeleteProductCommand = new ViewModelCommand(ExecuteDeleteProductCommand);
            _ = LoadProductsAsync();
        }

        private async void ExecuteEditProductCommand(object obj)
        {
            if (obj is Product product)
            {
                var viewModel = new ProductFormViewModel(_productService, product);
                var view = new Views.ProductFormView { DataContext = viewModel };

                viewModel.RequestClose += (s, e) =>
                {
                    view.Close();
                    _ = LoadProductsAsync();
                };

                view.ShowDialog();
            }
        }

        private async void ExecuteDeleteProductCommand(object obj)
        {
            if (obj is Product product)
            {
                var result = System.Windows.MessageBox.Show($"Are you sure you want to delete '{product.Name}'?", 
                                           "Confirm Delete", 
                                           System.Windows.MessageBoxButton.YesNo, 
                                           System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        await _productService.DeleteAsync(product.Id);
                        await LoadProductsAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error deleting product: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var list = await _productService.GetAllAsync();
                Products = new ObservableCollection<Product>(list);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading products: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Products = new ObservableCollection<Product>();
            }
        }

        private void ExecuteOpenProductFormCommand(object obj)
        {
            var viewModel = new ProductFormViewModel(_productService);
            var view = new Views.ProductFormView { DataContext = viewModel };

            viewModel.RequestClose += (s, e) => 
            {
                view.Close();
                _ = LoadProductsAsync();
            };
            
            view.ShowDialog();
        }
    }
}
