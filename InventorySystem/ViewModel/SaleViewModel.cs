using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class SaleViewModel : ViewModelBase
    {
        private System.Collections.ObjectModel.ObservableCollection<Models.Product> _products;
        private System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> _cart;
        private string _searchText;
        private decimal _total;

        public System.Collections.ObjectModel.ObservableCollection<Models.Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(nameof(Products)); }
        }

        public System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> Cart
        {
            get => _cart;
            set { _cart = value; OnPropertyChanged(nameof(Cart)); }
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                OnPropertyChanged(nameof(SearchText));
                FilterProducts();
            }
        }

        public decimal Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(nameof(Total)); }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public SaleViewModel()
        {
            Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>();
            Cart = new System.Collections.ObjectModel.ObservableCollection<CartItemViewModel>();
            Cart.CollectionChanged += (s, e) => CalculateTotal();

            AddToCartCommand = new ViewModelCommand(ExecuteAddToCartCommand);
            RemoveFromCartCommand = new ViewModelCommand(ExecuteRemoveFromCartCommand);
            CheckoutCommand = new ViewModelCommand(ExecuteCheckoutCommand, CanExecuteCheckout);

            LoadProducts();
        }

        private void LoadProducts()
        {
            using (var context = new Data.AppDbContext())
            {
                var list = context.Products.Where(p => p.IsActive && p.Quantity > 0).ToList();
                Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>(list);
            }
        }

        private void FilterProducts()
        {
            using (var context = new Data.AppDbContext())
            {
                var query = context.Products.Where(p => p.IsActive && p.Quantity > 0);
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(SearchText.ToLower()));
                }
                Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>(query.ToList());
            }
        }

        private void ExecuteAddToCartCommand(object obj)
        {
            if (obj is Models.Product product)
            {
                var existing = Cart.FirstOrDefault(c => c.Product.Id == product.Id);
                if (existing != null)
                {
                    existing.Quantity++;
                    CalculateTotal();
                }
                else
                {
                    var cartItem = new CartItemViewModel(product);
                    cartItem.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(CartItemViewModel.Quantity)) CalculateTotal(); };
                    Cart.Add(cartItem);
                }
            }
        }

        private void ExecuteRemoveFromCartCommand(object obj)
        {
            if (obj is CartItemViewModel item)
            {
                Cart.Remove(item);
            }
        }

        private void CalculateTotal()
        {
            Total = Cart.Sum(i => i.Subtotal);
        }

        private bool CanExecuteCheckout(object obj) => Cart.Count > 0;

        private void ExecuteCheckoutCommand(object obj)
        {
            try
            {
                using (var context = new Data.AppDbContext())
                {
                    // Ensure tables are created
                    context.Database.EnsureCreated();

                    var sale = new Models.Sale
                    {
                        SaleDate = DateTime.Now,
                        TotalAmount = Total,
                        // For now we'll use a placeholder client if needed, or null if schema allows
                        ClientId = context.Clients.FirstOrDefault()?.Id ?? 0 
                    };

                    foreach (var item in Cart)
                    {
                        var detail = new Models.SaleDetail
                        {
                            ProductId = item.Product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price,
                            TotalPrice = item.Subtotal
                        };
                        sale.SaleDetails.Add(detail);

                        // Update Stock
                        var dbProduct = context.Products.Find(item.Product.Id);
                        if (dbProduct != null)
                        {
                            dbProduct.Quantity -= item.Quantity;
                        }
                    }

                    context.Sales.Add(sale);
                    context.SaveChanges();
                }

                System.Windows.MessageBox.Show("Sale finalized successfully!", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Cart.Clear();
                LoadProducts(); // Refresh stock in view
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }
    }
}
