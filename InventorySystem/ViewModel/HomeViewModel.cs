using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using InventorySystem.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using InventorySystem.Helpers;

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
        private SeriesCollection _saleSeries;
        private SeriesCollection _categorySeries;
        private ObservableCollection<Product> _criticalItems;
        private ObservableCollection<TopProductInfo> _topProducts;

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

        public SeriesCollection SaleSeries
        {
            get => _saleSeries;
            set { _saleSeries = value; OnPropertyChanged(nameof(SaleSeries)); }
        }

        public SeriesCollection CategorySeries
        {
            get => _categorySeries;
            set { _categorySeries = value; OnPropertyChanged(nameof(CategorySeries)); }
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

        public Func<double, string> Formatter { get; set; }
        public ICommand RefreshCommand { get; }
        public ICommand SeedDataCommand { get; }
        public ICommand ShowDatabaseStatsCommand { get; }
        public ICommand TestCommand { get; }

        private readonly AppDbContext _db;
        private readonly ISeedDataService _seedService;
        private readonly IMessageService _messageService;

        public HomeViewModel(AppDbContext db, ISeedDataService seedService, IMessageService messageService)
        {
            _db = db;
            _seedService = seedService;
            _messageService = messageService;
            Formatter = value => value.ToString("C0");
            CriticalItems = new ObservableCollection<Product>();
            TopProducts = new ObservableCollection<TopProductInfo>();
            
            RefreshCommand = new ViewModelCommand(_ => LoadDashboardData());
            SeedDataCommand = new ViewModelCommand(_ => ExecuteSeedDataCommandSync());
            ShowDatabaseStatsCommand = new ViewModelCommand(_ => DatabaseDiagnostics.ShowDatabaseStats(_db));
            TestCommand = new ViewModelCommand(_ => _messageService.ShowInfo("Test button works!"));
            
            LoadDashboardData();
        }

        private void ExecuteSeedDataCommandSync()
        {
            // Fire and forget pattern for async operation
            _ = ExecuteSeedDataCommand();
        }

        private async Task ExecuteSeedDataCommand()
        {
            try
            {
                IsLoading = true;
                await _seedService.SeedAsync();
                LoadDashboardData();
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

        private void LoadDashboardData()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);

                // 1. Core KPIs
                var salesTodayList = _db.Sales.Where(s => s.SaleDate >= today).ToList();
                var salesTotalToday = salesTodayList.Sum(s => s.TotalAmount);
                TotalSalesToday = $"${salesTotalToday:N2}";

                var salesYesterday = _db.Sales
                    .Where(s => s.SaleDate >= yesterday && s.SaleDate < today)
                    .ToList()
                    .Sum(s => s.TotalAmount);

                if (salesYesterday > 0)
                {
                    var trend = ((salesTotalToday - salesYesterday) / salesYesterday) * 100;
                    SalesTrend = $"{(trend >= 0 ? "+" : "")}{trend:F1}% vs yesterday";
                }
                else
                {
                    SalesTrend = "No data from yesterday";
                }

                NewClientsToday = _db.Clients.Count(c => c.CreatedAt >= today).ToString();
                
                var allProducts = _db.Products.ToList();
                TotalStock = allProducts.Sum(p => p.Quantity).ToString();
                TotalStockValue = $"${allProducts.Sum(p => p.Price * p.Quantity):N0}";

                // Debug: Log counts
                System.Diagnostics.Debug.WriteLine($"Dashboard Load - Products: {allProducts.Count}, Sales Today: {salesTodayList.Count}, Clients: {_db.Clients.Count()}");

                // 2. Critical Stock (Quantity < 5)
                var lowStock = allProducts.Where(p => p.Quantity < 5 && p.IsActive).OrderBy(p => p.Quantity).Take(5).ToList();
                CriticalItems.Clear();
                foreach (var item in lowStock) CriticalItems.Add(item);
                CriticalStockCount = lowStock.Count.ToString();

                // 3. Top Products (Sales-based)
                var topSelling = _db.SaleDetails
                    .GroupBy(d => d.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(d => d.Quantity) })
                    .OrderByDescending(x => x.TotalQty)
                    .Take(5)
                    .ToList();

                TopProducts.Clear();
                foreach (var tsp in topSelling)
                {
                    var prod = allProducts.FirstOrDefault(p => p.Id == tsp.ProductId);
                    if (prod != null)
                    {
                        TopProducts.Add(new TopProductInfo { Name = prod.Name, Quantity = tsp.TotalQty, Category = prod.Category });
                    }
                }

                // 4. Sales History Chart
                var sixMonthsAgo = today.AddMonths(-5);
                var historicalSales = _db.Sales
                    .Where(s => s.SaleDate >= sixMonthsAgo)
                    .ToList()
                    .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                    .Select(g => new { Date = g.Key, Amount = g.Sum(s => s.TotalAmount) })
                    .OrderBy(g => g.Date.Year).ThenBy(g => g.Date.Month)
                    .ToList();

                if (historicalSales.Count == 0)
                {
                    SaleSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Sample Revenue",
                            Values = new ChartValues<decimal> { 1200, 1500, 1100, 1800, 2100, 1900 },
                            PointGeometry = DefaultGeometries.Circle,
                            Stroke = System.Windows.Media.Brushes.DodgerBlue,
                            Fill = new System.Windows.Media.LinearGradientBrush(System.Windows.Media.Colors.LightBlue, System.Windows.Media.Colors.Transparent, 90)
                        }
                    };
                }
                else
                {
                    SaleSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Revenue",
                            Values = new ChartValues<decimal>(historicalSales.Select(x => x.Amount)),
                            Stroke = System.Windows.Media.Brushes.Indigo,
                            Fill = new System.Windows.Media.LinearGradientBrush(System.Windows.Media.Colors.Indigo, System.Windows.Media.Colors.Transparent, 90)
                        }
                    };
                }
            }
            catch (Exception)
            {
                TotalSalesToday = "$0.00";
                TotalStock = "0";
                TotalStockValue = "$0.00";
                CriticalStockCount = "0";
                SalesTrend = "Error loading data";
                
                // Fallback chart on error
                SaleSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Sample Data",
                        Values = new ChartValues<decimal> { 10, 20, 15, 25, 30 },
                        Stroke = System.Windows.Media.Brushes.Gray
                    }
                };
            }
        }
    }
}
