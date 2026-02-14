using InventorySystem.ViewModel.Base;
using InventorySystem.Reports;
using System;
using System.Windows.Input;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.Win32;
using System.Diagnostics;

namespace InventorySystem.ViewModel
{
    class InformsViewModel : ViewModelBase
    {
        public ICommand GenerateSalesReportCommand { get; }

        public InformsViewModel()
        {
            // Initialize QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;

            GenerateSalesReportCommand = new ViewModelCommand(ExecuteGenerateSalesReportCommand);
        }

        private void ExecuteGenerateSalesReportCommand(object obj)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var document = new SalesReportDocument();
                document.GeneratePdf(saveFileDialog.FileName);

                Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
            }
        }
    }
}
