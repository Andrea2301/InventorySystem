using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class ProductsViewModel : ViewModelBase
    {
        public ICommand OpenProductFormCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        private System.Collections.ObjectModel.ObservableCollection<Models.Product> _products;
        public System.Collections.ObjectModel.ObservableCollection<Models.Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        public ProductsViewModel()
        {
            OpenProductFormCommand = new ViewModelCommand(ExecuteOpenProductFormCommand);
            EditProductCommand = new ViewModelCommand(ExecuteEditProductCommand);
            DeleteProductCommand = new ViewModelCommand(ExecuteDeleteProductCommand);
            LoadProducts();
        }

        private void ExecuteEditProductCommand(object obj)
        {
            if (obj is Models.Product product)
            {
                var viewModel = new ProductFormViewModel(product);
                var view = new Views.ProductFormView();
                view.DataContext = viewModel;

                viewModel.RequestClose += (s, e) =>
                {
                    view.Close();
                    LoadProducts(); // Refresh list after edit
                };

                view.ShowDialog();
            }
        }

        private void ExecuteDeleteProductCommand(object obj)
        {
            if (obj is Models.Product product)
            {
                var result = System.Windows.MessageBox.Show($"Are you sure you want to delete '{product.Name}'?", 
                                           "Confirm Delete", 
                                           System.Windows.MessageBoxButton.YesNo, 
                                           System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new Data.AppDbContext())
                        {
                            context.Products.Remove(product);
                            context.SaveChanges();
                        }
                        LoadProducts(); // Refresh list
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error deleting product: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = new Data.AppDbContext())
                {
                    // Ensure database is created (optional, depending on migration strategy)
                    context.Database.EnsureCreated();

                    var list = context.Products.ToList();
                    Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>(list);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading products: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>();
            }
        }

        private void ExecuteOpenProductFormCommand(object obj)
        {
            var viewModel = new ProductFormViewModel();
            var view = new Views.ProductFormView { DataContext = viewModel };

            // Subscribe to Close event
            viewModel.RequestClose += (s, e) => 
            {
                view.Close();
                LoadProducts(); // Refresh list after closing
            };
            
            view.ShowDialog();
        }
    }
}
