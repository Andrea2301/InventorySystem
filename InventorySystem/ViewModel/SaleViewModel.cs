using InventorySystem.Services;
using InventorySystem.Services.Export;
using InventorySystem.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly IPdfService _pdfService;
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

        public SaleViewModel(ISaleService saleService, IProductService productService, IClientService clientService, IDialogService dialogService, IMessageService messageService, IPdfService pdfService)
        {
            _saleService = saleService;
            _productService = productService;
            _clientService = clientService;
            _dialogService = dialogService;
            _messageService = messageService;
            _pdfService = pdfService;
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
                var checkoutVm = new CheckoutViewModel(Total, SelectedClient, _messageService);
                _dialogService.ShowDialog(checkoutVm);

                if (!checkoutVm.IsPaymentSuccessful)
                {
                    return; // Cobro cancelado
                }

                IsLoading = true;
                var sale = new Models.Sale
                {
                    SaleDate = DateTime.Now,
                    TotalAmount = Total,
                    ClientId = SelectedClient?.Id ?? 0,
                    PaymentMethod = checkoutVm.PaymentMethod,
                    AmountPaid = checkoutVm.PaymentMethod == "Efectivo" ? checkoutVm.CashReceived : Total,
                    ChangeDue = checkoutVm.PaymentMethod == "Efectivo" ? checkoutVm.ChangeDue : 0,
                    Currency = checkoutVm.SelectedCurrency
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

                bool openPdf = _messageService.ShowConfirmation("¡Venta finalizada con éxito!\n\n¿Desea abrir o imprimir el ticket PDF de la venta?", "Venta Exitosa");
                if (openPdf)
                {
                    await GenerateAndOpenReceiptAsync(sale);
                }

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
        private async Task GenerateAndOpenReceiptAsync(Models.Sale sale)
        {
            try
            {
                // Recargar la venta con sus relaciones (cliente, detalles y productos) para el PDF
                var fullSale = await _saleService.GetSaleByIdAsync(sale.Id);
                if (fullSale == null) return;

                // Carpeta Tickets en el directorio local de la app
                string ticketsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "InventorySystem", "Tickets");
                Directory.CreateDirectory(ticketsDir);

                string fileName = $"Ticket_{fullSale.Id:D6}_{fullSale.SaleDate:yyyyMMdd_HHmmss}.pdf";
                string filePath = Path.Combine(ticketsDir, fileName);

                await _pdfService.GenerateInvoiceAsync(fullSale, filePath);

                // Abrir con el visor PDF predeterminado del sistema
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"No se pudo generar el ticket PDF: {ex.Message}");
            }
        }
    }
}
