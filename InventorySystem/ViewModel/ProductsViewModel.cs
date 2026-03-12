using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InventorySystem.Models;
using InventorySystem.Services;
using InventorySystem.ViewModel.Base;
using InventorySystem.Views;

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
        public ICommand OpenImportExcelCommand { get; }

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
            OpenImportExcelCommand = new ViewModelCommand(ExecuteOpenImportExcelCommand);
            _ = LoadProductsAsync();
        }

        private void ExecuteOpenImportExcelCommand(object obj)
        {
            var viewModel = new ImportFileViewModel();
            var view = new ImportFileView { DataContext = viewModel };

            if (view.ShowDialog() == true)
            {
                _ = ImportProducts(viewModel.SelectedFilePath);
            }
        }

        private async Task ImportProducts(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _messageService.ShowWarning("No file selected.");
                    return;
                }

                IsLoading = true;
                var rows = MiniExcelLibs.MiniExcel.Query(filePath, useHeaderRow: true)
                            .Cast<IDictionary<string, object>>()
                            .ToList();

                if (rows == null || rows.Count == 0)
                {
                    _messageService.ShowWarning("The file is empty or could not be read.");
                    return;
                }

                var productsToImport = new List<Product>();
                foreach (var row in rows)
                {
                    string GetValue(params string[] possibleAliases)
                    {
                        foreach (var alias in possibleAliases)
                        {
                            var matchingKey = row.Keys.FirstOrDefault(k => k.Trim().Equals(alias, StringComparison.OrdinalIgnoreCase));
                            if (matchingKey != null) return row[matchingKey]?.ToString() ?? "";
                        }
                        return "";
                    }

                    var name = GetValue("Name", "Nombre", "Producto");
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    decimal.TryParse(GetValue("Price", "Precio", "Costo"), out decimal price);
                    int.TryParse(GetValue("Quantity", "Stock", "Cantidad"), out int quantity);

                    productsToImport.Add(new Product
                    {
                        Name = name,
                        Description = GetValue("Description", "Descripción", "Detalle"),
                        Category = GetValue("Category", "Categoría", "Rubro") ?? "General",
                        Price = price,
                        Quantity = quantity,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }

                if (productsToImport.Count > 0)
                {
                    await _productService.AddRangeAsync(productsToImport);
                    await LoadProductsAsync();
                    _messageService.ShowInfo($"{productsToImport.Count} products imported successfully.");
                }
                else
                {
                    _messageService.ShowWarning("No valid data found in the file to import.");
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error importing file: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
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
