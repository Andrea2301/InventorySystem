using InventorySystem.ViewModel.Base;
using InventorySystem.Reports;
using InventorySystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.Win32;
using System.Diagnostics;

namespace InventorySystem.ViewModel
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly ISaleService _saleService;
        private readonly IClientService _clientService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly IMessageService _messageService;

        public ICommand GenerateSalesReportCommand { get; }
        public ICommand GenerateClientsReportCommand { get; }
        public ICommand GenerateSuppliersReportCommand { get; }
        public ICommand GenerateInventoryReportCommand { get; }

        public ReportsViewModel(
            ISaleService saleService, 
            IClientService clientService, 
            ISupplierService supplierService, 
            IProductService productService,
            IMessageService messageService)
        {
            _saleService = saleService;
            _clientService = clientService;
            _supplierService = supplierService;
            _productService = productService;
            _messageService = messageService;

            // Initialize QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;

            GenerateSalesReportCommand = new ViewModelCommand(async _ => await ExecuteGenerateSalesReportCommand());
            GenerateClientsReportCommand = new ViewModelCommand(async _ => await ExecuteGenerateClientsReportCommand());
            GenerateSuppliersReportCommand = new ViewModelCommand(async _ => await ExecuteGenerateSuppliersReportCommand());
            GenerateInventoryReportCommand = new ViewModelCommand(async _ => await ExecuteGenerateInventoryReportCommand());
        }

        private async Task ExecuteGenerateSalesReportCommand()
        {
            try 
            {
                IsLoading = true;
                var sales = await _saleService.GetAllSalesAsync();
                
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Sales_Report_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var document = new SalesReportDocument(sales);
                    document.GeneratePdf(saveFileDialog.FileName);
                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error generating sales report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteGenerateClientsReportCommand()
        {
            try
            {
                IsLoading = true;
                var clients = await _clientService.GetAllAsync();

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Clients_Report_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var document = new ClientsReportDocument(clients);
                    document.GeneratePdf(saveFileDialog.FileName);
                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error generating clients report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteGenerateSuppliersReportCommand()
        {
            try
            {
                IsLoading = true;
                var suppliers = await _supplierService.GetAllAsync();

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Suppliers_Report_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var document = new SuppliersReportDocument(suppliers);
                    document.GeneratePdf(saveFileDialog.FileName);
                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error generating suppliers report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteGenerateInventoryReportCommand()
        {
            try
            {
                IsLoading = true;
                var products = await _productService.GetAllAsync();

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Inventory_Report_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var document = new InventoryReportDocument(products);
                    document.GeneratePdf(saveFileDialog.FileName);
                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError($"Error generating inventory report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
