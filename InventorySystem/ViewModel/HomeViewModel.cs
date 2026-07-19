using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using InventorySystem.Services;
using InventorySystem.Services.Export;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        private string _totalSalesToday;
        private string _newClientsToday;
        private string _totalStock;
        private string _totalStockValue;
        private string _criticalStockCount;
        private string _salesTrend;
        private ObservableCollection<Product> _criticalItems;
        private ObservableCollection<TopProductInfo> _topProducts;
        private ObservableCollection<Sale> _recentSales;

        public string TotalSalesToday
        {
            get => _totalSalesToday;
            set { _totalSalesToday = value; OnPropertyChanged(nameof(TotalSalesToday)); }
        }

        public string NewClientsToday
        {
            get => _newClientsToday;
            set { _newClientsToday = value; OnPropertyChanged(nameof(NewClientsToday)); }
        }

        public string TotalStock
        {
            get => _totalStock;
            set { _totalStock = value; OnPropertyChanged(nameof(TotalStock)); }
        }

        public string TotalStockValue
        {
            get => _totalStockValue;
            set { _totalStockValue = value; OnPropertyChanged(nameof(TotalStockValue)); }
        }

        public string CriticalStockCount
        {
            get => _criticalStockCount;
            set { _criticalStockCount = value; OnPropertyChanged(nameof(CriticalStockCount)); }
        }

        public string SalesTrend
        {
            get => _salesTrend;
            set { _salesTrend = value; OnPropertyChanged(nameof(SalesTrend)); }
        }

        public ObservableCollection<Product> CriticalItems
        {
            get => _criticalItems;
            set { _criticalItems = value; OnPropertyChanged(nameof(CriticalItems)); }
        }

        public ObservableCollection<TopProductInfo> TopProducts
        {
            get => _topProducts;
            set { _topProducts = value; OnPropertyChanged(nameof(TopProducts)); }
        }

        public ObservableCollection<Sale> RecentSales
        {
            get => _recentSales;
            set { _recentSales = value; OnPropertyChanged(nameof(RecentSales)); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand SeedDataCommand { get; }
        public ICommand ShowDatabaseStatsCommand { get; }
        public ICommand NavigateToSaleCommand { get; }
        public ICommand NavigateToProductsCommand { get; }
        public ICommand NavigateToClientsCommand { get; }
        public ICommand NavigateToHistoryCommand { get; }
        public ICommand OpenReceiptCommand { get; }

        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageService _messageService;

        public HomeViewModel(
            IServiceProvider serviceProvider,
            IMessageService messageService)
        {
            _serviceProvider = serviceProvider;
            _messageService = messageService;

            CriticalItems = new ObservableCollection<Product>();
            TopProducts = new ObservableCollection<TopProductInfo>();
            RecentSales = new ObservableCollection<Sale>();
            
            RefreshCommand = new ViewModelCommand(_ => _ = LoadDashboardDataAsync());
            SeedDataCommand = new ViewModelCommand(_ => ExecuteSeedDataCommandSync());
            ShowDatabaseStatsCommand = new ViewModelCommand(_ => { });

            // Navigation commands calling MainViewModel commands (resolved lazily to break circular dependency)
            NavigateToSaleCommand = new ViewModelCommand(_ => _serviceProvider.GetRequiredService<MainViewModel>().ShowSaleViewCommand.Execute(null));
            NavigateToProductsCommand = new ViewModelCommand(_ => _serviceProvider.GetRequiredService<MainViewModel>().ShowProductsViewCommand.Execute(null));
            NavigateToClientsCommand = new ViewModelCommand(_ => _serviceProvider.GetRequiredService<MainViewModel>().ShowClientViewCommand.Execute(null));
            NavigateToHistoryCommand = new ViewModelCommand(_ => _serviceProvider.GetRequiredService<MainViewModel>().ShowSalesHistoryViewCommand.Execute(null));
            OpenReceiptCommand = new ViewModelCommand(async (obj) => await ExecuteOpenReceipt(obj));

            // Load dashboard data asynchronously so the window appears immediately
            _ = LoadDashboardDataAsync();
        }

        private void ExecuteSeedDataCommandSync()
        {
            _ = ExecuteSeedDataCommand();
        }

        private async Task ExecuteSeedDataCommand()
        {
            try
            {
                IsLoading = true;
                using var scope = _serviceProvider.CreateScope();
                var seedService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
                await seedService.SeedAsync();
                await LoadDashboardDataAsync();
                _messageService.ShowInfo("Sample data generated successfully! Dashboard refreshed.");
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error generating sample data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public class TopProductInfo
        {
            public string Name { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Category { get; set; } = string.Empty;
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;

                // Run all DB queries on a background thread to keep UI responsive
                var data = await Task.Run(() =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);

                    var salesTodayList = db.Sales.Where(s => s.SaleDate >= today).ToList();
                    var salesTotalToday = salesTodayList.Sum(s => s.TotalAmount);

                    var salesYesterday = db.Sales
                        .Where(s => s.SaleDate >= yesterday && s.SaleDate < today)
                        .ToList()
                        .Sum(s => s.TotalAmount);

                    var newClients = db.Clients.Count(c => c.CreatedAt >= today);
                    var allProducts = db.Products.Where(p => p.IsActive).ToList();

                    var lowStock = allProducts
                        .Where(p => p.Quantity < 5)
                        .OrderBy(p => p.Quantity)
                        .Take(5)
                        .ToList();

                    var topSelling = db.SaleDetails
                        .GroupBy(d => d.ProductId)
                        .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(d => d.Quantity) })
                        .OrderByDescending(x => x.TotalQty)
                        .Take(5)
                        .ToList();

                    var recentList = db.Sales
                        .Include(s => s.Client)
                        .OrderByDescending(s => s.SaleDate)
                        .Take(5)
                        .ToList();

                    return new
                    {
                        SalesTotalToday = salesTotalToday,
                        SalesYesterday = salesYesterday,
                        NewClients = newClients,
                        AllProducts = allProducts,
                        LowStock = lowStock,
                        TopSelling = topSelling,
                        RecentList = recentList
                    };
                });

                // Update UI properties back on the UI thread
                TotalSalesToday = $"${data.SalesTotalToday:N2}";

                if (data.SalesYesterday > 0)
                {
                    var trend = ((data.SalesTotalToday - data.SalesYesterday) / data.SalesYesterday) * 100;
                    SalesTrend = $"{(trend >= 0 ? "+" : "")}{trend:F1}% vs ayer";
                }
                else
                {
                    SalesTrend = "Sin datos de ayer";
                }

                NewClientsToday = data.NewClients.ToString();
                TotalStock = data.AllProducts.Sum(p => p.Quantity).ToString();
                TotalStockValue = $"${data.AllProducts.Sum(p => p.Price * p.Quantity):N0}";

                CriticalItems.Clear();
                foreach (var item in data.LowStock) CriticalItems.Add(item);
                CriticalStockCount = data.LowStock.Count.ToString();

                TopProducts.Clear();
                foreach (var tsp in data.TopSelling)
                {
                    var prod = data.AllProducts.FirstOrDefault(p => p.Id == tsp.ProductId);
                    if (prod != null)
                        TopProducts.Add(new TopProductInfo { Name = prod.Name, Quantity = tsp.TotalQty, Category = prod.Category });
                }

                RecentSales.Clear();
                foreach (var sale in data.RecentList) RecentSales.Add(sale);
            }
            catch (Exception ex)
            {
                TotalSalesToday = "$0.00";
                TotalStock = "0";
                TotalStockValue = "$0.00";
                CriticalStockCount = "0";
                SalesTrend = "Error al cargar";
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex}\n{ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteOpenReceipt(object obj)
        {
            if (obj is Sale sale)
            {
                try
                {
                    IsLoading = true;
                    using var scope = _serviceProvider.CreateScope();
                    var saleService = scope.ServiceProvider.GetRequiredService<ISaleService>();
                    var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();

                    var fullSale = await saleService.GetSaleByIdAsync(sale.Id);
                    if (fullSale == null)
                    {
                        _messageService.ShowError("No se pudo cargar los detalles de la venta seleccionada.");
                        return;
                    }

                    string ticketsDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "InventorySystem", "Tickets");
                    Directory.CreateDirectory(ticketsDir);

                    string fileName = $"Ticket_{fullSale.Id:D6}_{fullSale.SaleDate:yyyyMMdd_HHmmss}.pdf";
                    string filePath = Path.Combine(ticketsDir, fileName);

                    await pdfService.GenerateInvoiceAsync(fullSale, filePath);

                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Error al abrir el ticket PDF: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
