using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        public HomeViewModel()
        {
            Formatter = value => value.ToString("C0");
            CriticalItems = new ObservableCollection<Product>();
            TopProducts = new ObservableCollection<TopProductInfo>();
            LoadDashboardData();
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
                using (var db = new AppDbContext())
                {
                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);

                    // 1. Core KPIs
                    var salesTodayList = db.Sales.Where(s => s.SaleDate >= today).ToList();
                    var salesTotalToday = salesTodayList.Sum(s => s.TotalAmount);
                    TotalSalesToday = $"${salesTotalToday:N2}";

                    var salesYesterday = db.Sales
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

                    NewClientsToday = db.Clients.Count(c => c.CreatedAt >= today).ToString();
                    
                    var allProducts = db.Products.ToList();
                    TotalStock = allProducts.Sum(p => p.Quantity).ToString();
                    TotalStockValue = $"${allProducts.Sum(p => p.Price * p.Quantity):N0}";

                    // 2. Critical Stock (Quantity < 5)
                    var lowStock = allProducts.Where(p => p.Quantity < 5 && p.IsActive).OrderBy(p => p.Quantity).Take(5).ToList();
                    CriticalItems.Clear();
                    foreach (var item in lowStock) CriticalItems.Add(item);
                    CriticalStockCount = lowStock.Count.ToString();

                    // 3. Top Products (Sales-based)
                    var topSelling = db.SaleDetails
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
                    var historicalSales = db.Sales
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
            }
            catch (Exception)
            {
                TotalSalesToday = "$0.00";
                TotalStock = "0";
            }
        }
    }
}
