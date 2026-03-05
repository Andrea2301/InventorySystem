using InventorySystem.Models;
using InventorySystem.Services;
using InventorySystem.Services.Export;
using InventorySystem.ViewModel.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InventorySystem.ViewModel
{
    public class SalesHistoryViewModel : ViewModelBase
    {
        private readonly ISaleService _saleService;
        private readonly IPdfService _pdfService;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
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
                _ = LoadSalesAsync(); 
            }
        }

        public ICommand ViewDetailCommand { get; }
        public ICommand PrintReceiptCommand { get; }

        public SalesHistoryViewModel(ISaleService saleService, IPdfService pdfService, IDialogService dialogService, IMessageService messageService)
        {
            _saleService = saleService;
            _pdfService = pdfService;
            _dialogService = dialogService;
            _messageService = messageService;
            Sales = new ObservableCollection<Sale>();
            ViewDetailCommand = new ViewModelCommand(ExecuteViewDetailCommand);
            PrintReceiptCommand = new ViewModelCommand(ExecutePrintReceiptCommand);
            _ = LoadSalesAsync();
        }

        private async Task LoadSalesAsync()
        {
            try
            {
                IsLoading = true;
                var list = await _saleService.GetAllSalesAsync(SearchText);
                Sales.Clear();
                foreach (var sale in list)
                {
                    Sales.Add(sale);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error loading sales history: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteViewDetailCommand(object obj)
        {
            if (obj is Sale sale)
            {
                _dialogService.ShowDialog(sale);
            }
        }

        private async void ExecutePrintReceiptCommand(object obj)
        {
            if (obj is Sale sale)
            {
                try
                {
                    var sfd = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = $"Factura_{sale.Id:D6}",
                        DefaultExt = ".pdf",
                        Filter = "PDF Documents (.pdf)|*.pdf"
                    };

                    if (sfd.ShowDialog() == true)
                    {
                        IsLoading = true;
                        await _pdfService.GenerateInvoiceAsync(sale, sfd.FileName);
                        _messageService.ShowInfo("Factura generada exitosamente.");
                        
                        // Opcional: Abrir el PDF automáticamente
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    _messageService.ShowError($"Error al generar la factura: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
