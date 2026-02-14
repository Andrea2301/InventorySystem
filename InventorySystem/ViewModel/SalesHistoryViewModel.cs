using InventorySystem.Models;
using InventorySystem.ViewModel.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    class SalesHistoryViewModel : ViewModelBase
    {
        private ObservableCollection<Sale> _sales;
        private Sale _selectedSale;
        private string _searchText;

        public ObservableCollection<Sale> Sales
        {
            get => _sales;
            set { _sales = value; OnPropertyChanged(nameof(Sales)); }
        }

        public Sale SelectedSale
        {
            get => _selectedSale;
            set { _selectedSale = value; OnPropertyChanged(nameof(SelectedSale)); }
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                OnPropertyChanged(nameof(SearchText));
                LoadSales(); 
            }
        }

        public ICommand ViewDetailCommand { get; }
        public ICommand PrintReceiptCommand { get; }

        public SalesHistoryViewModel()
        {
            Sales = new ObservableCollection<Sale>();
            ViewDetailCommand = new ViewModelCommand(ExecuteViewDetailCommand);
            PrintReceiptCommand = new ViewModelCommand(ExecutePrintReceiptCommand);
            LoadSales();
        }

        private void LoadSales()
        {
            try
            {
                using (var context = new Data.AppDbContext())
                {
                    // Ensure tables are created
                    context.Database.EnsureCreated();

                    var query = context.Sales
                        .Include(s => s.Client)
                        .Include(s => s.SaleDetails)
                        .ThenInclude(sd => sd.Product)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {

                        if (int.TryParse(SearchText, out int searchId))
                        {
                            query = query.Where(s => s.Id == searchId || 
                                                   (s.Client != null && (s.Client.FirstName.ToLower().Contains(SearchText.ToLower()) || 
                                                                        s.Client.LastName.ToLower().Contains(SearchText.ToLower()))));
                        }
                        else
                        {
                            query = query.Where(s => s.Client != null && (s.Client.FirstName.ToLower().Contains(SearchText.ToLower()) || 
                                                                        s.Client.LastName.ToLower().Contains(SearchText.ToLower())));
                        }
                    }

                    var list = query.OrderByDescending(s => s.SaleDate).ToList();
                    Sales = new ObservableCollection<Sale>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales history: {ex.Message}", "Error");
            }
        }

        private void ExecuteViewDetailCommand(object obj)
        {
            if (obj is Sale sale)
            {
                // We'll open a detail window/popup
                var detailView = new Views.SaleDetailView();
                detailView.DataContext = sale; 
                detailView.ShowDialog();
            }
        }

        private void ExecutePrintReceiptCommand(object obj)
        {
            if (obj is Sale sale)
            {
                // Placeholder for receipt generation
                MessageBox.Show($"Generating receipt for Sale #{sale.Id}...\n(In a real scenario, this would generate a PDF or print to a thermal printer)", 
                                "Receipt Generation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
