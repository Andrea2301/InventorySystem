using InventorySystem.Services;
using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class SaleViewModel : ViewModelBase
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly IClientService _clientService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private System.Collections.ObjectModel.ObservableCollection<Models.Product> _products;
        private System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> _cart;
        private System.Collections.ObjectModel.ObservableCollection<Models.Client> _clients;
        private Models.Client _selectedClient;
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

        public System.Collections.ObjectModel.ObservableCollection<Models.Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(nameof(Clients)); }
        }

        public Models.Client SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(nameof(SelectedClient)); }
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                OnPropertyChanged(nameof(SearchText));
                _ = FilterProductsAsync();
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

        public SaleViewModel(ISaleService saleService, IProductService productService, IClientService clientService, IDialogService dialogService, IMessageService messageService)
        {
            _saleService = saleService;
            _productService = productService;
            _clientService = clientService;
            _dialogService = dialogService;
            _messageService = messageService;
            Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>();
            Cart = new System.Collections.ObjectModel.ObservableCollection<CartItemViewModel>();
            Clients = new System.Collections.ObjectModel.ObservableCollection<Models.Client>();
            Cart.CollectionChanged += (s, e) => CalculateTotal();

            AddToCartCommand = new ViewModelCommand(ExecuteAddToCartCommand);
            RemoveFromCartCommand = new ViewModelCommand(ExecuteRemoveFromCartCommand);
            CheckoutCommand = new ViewModelCommand(ExecuteCheckoutCommand, CanExecuteCheckout);

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
             IsLoading = true;
             try 
             {
                await Task.WhenAll(LoadProductsAsync(), LoadClientsAsync());
             }
             finally 
             {
                IsLoading = false;
             }
        }

        private async Task LoadClientsAsync()
        {
            try
            {
                var list = await _clientService.GetAllAsync();
                Clients = new System.Collections.ObjectModel.ObservableCollection<Models.Client>(list);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading clients: {ex.Message}");
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var list = await _productService.GetAllAsync();
                var activeProducts = list.Where(p => p.IsActive && p.Quantity > 0).ToList();
                Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>(activeProducts);
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading products: {ex.Message}");
            }
        }

        private async Task FilterProductsAsync()
        {
            try
            {
                var list = await _productService.GetAllAsync();
                var query = list.Where(p => p.IsActive && p.Quantity > 0);
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }
                
                Products = new System.Collections.ObjectModel.ObservableCollection<Models.Product>(query.ToList());
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error filtering products: {ex.Message}");
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

        private async void ExecuteCheckoutCommand(object obj)
        {
            try
            {
                IsLoading = true;
                var sale = new Models.Sale
                {
                    SaleDate = DateTime.Now,
                    TotalAmount = Total,
                    ClientId = SelectedClient?.Id ?? 0 // Use selected client or let service handle default
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
                }

                await _saleService.CreateSaleAsync(sale);

                _messageService.ShowInfo("¡Venta finalizada con éxito!");
                Cart.Clear();
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error al finalizar la venta: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
