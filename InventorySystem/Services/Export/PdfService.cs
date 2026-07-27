using InventorySystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace InventorySystem.Services.Export
{
    public class PdfService : IPdfService
    {
        private readonly IBusinessSettingService _businessSettingService;

        public PdfService(IBusinessSettingService businessSettingService)
        {
            _businessSettingService = businessSettingService;
        }

        public async Task GenerateInvoiceAsync(Sale sale, string filePath)
        {
            var settings = await _businessSettingService.GetSettingsAsync();

            // Compute tax breakdown
            decimal taxPercent = settings.TaxPercentage;
            decimal subtotal = sale.TotalAmount / (1 + (taxPercent / 100));
            decimal taxAmount = sale.TotalAmount - subtotal;
            string currencySymbol = settings.CurrencySymbol;

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Size(PageSizes.A4);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        // Header with dynamic business information
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(settings.CompanyName.ToUpper()).FontSize(20).SemiBold().FontColor("#3F51B5");
                                if (!string.IsNullOrEmpty(settings.TaxId))
                                {
                                    col.Item().Text(settings.TaxId).FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                                }
                                if (!string.IsNullOrEmpty(settings.Address))
                                {
                                    col.Item().Text(settings.Address).FontSize(9).FontColor(Colors.Grey.Darken1);
                                }
                                if (!string.IsNullOrEmpty(settings.Phone) || !string.IsNullOrEmpty(settings.Email))
                                {
                                    string contactInfo = $"{settings.Phone}  {settings.Email}".Trim();
                                    col.Item().Text(contactInfo).FontSize(9).FontColor(Colors.Grey.Darken1);
                                }
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"FACTURA #{sale.Id:D6}").FontSize(16).SemiBold().AlignRight();
                                col.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}").AlignRight();
                                col.Item().Text($"Pago: {sale.PaymentMethod}").AlignRight().FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(25).Column(col =>
                        {
                            // Client Info
                            col.Item().PaddingBottom(15).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
                            {
                                row.RelativeItem().Column(clientCol =>
                                {
                                    clientCol.Item().Text("CLIENTE:").SemiBold().FontSize(10).FontColor(Colors.Grey.Darken2);
                                    clientCol.Item().Text(sale.Client?.FullName ?? "Cliente General").FontSize(12).SemiBold();
                                    if (!string.IsNullOrEmpty(sale.Client?.Email))
                                        clientCol.Item().Text($"Correo: {sale.Client.Email}").FontSize(10);
                                    if (!string.IsNullOrEmpty(sale.Client?.PhoneNumber))
                                        clientCol.Item().Text($"Teléfono: {sale.Client.PhoneNumber}").FontSize(10);
                                });
                            });

                            // Table of Products
                            col.Item().PaddingTop(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Product
                                    columns.RelativeColumn();  // Qty
                                    columns.RelativeColumn();  // Price
                                    columns.RelativeColumn();  // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Producto");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Cant.");
                                    header.Cell().Element(CellStyle).AlignRight().Text("P. Unit");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold())
                                                        .PaddingVertical(5)
                                                        .BorderBottom(1)
                                                        .BorderColor(Colors.Grey.Lighten2);
                                    }
                                });

                                foreach (var detail in sale.SaleDetails)
                                {
                                    table.Cell().Element(ItemStyle).Text(detail.Product?.Name ?? "Producto Desconocido");
                                    table.Cell().Element(ItemStyle).AlignRight().Text(detail.Quantity.ToString());
                                    table.Cell().Element(ItemStyle).AlignRight().Text(FormatPrice(detail.UnitPrice, currencySymbol, sale.Currency));
                                    table.Cell().Element(ItemStyle).AlignRight().Text(FormatPrice(detail.TotalPrice, currencySymbol, sale.Currency));

                                    static IContainer ItemStyle(IContainer container)
                                    {
                                        return container.PaddingVertical(5)
                                                        .BorderBottom(1)
                                                        .BorderColor(Colors.Grey.Lighten4);
                                    }
                                }
                            });

                            // Summary of Pricing & Taxes
                            col.Item().AlignRight().PaddingTop(20).Column(sumCol =>
                            {
                                sumCol.Item().Row(r =>
                                {
                                    r.RelativeItem().AlignRight().Text("Subtotal:").FontColor(Colors.Grey.Darken2);
                                    r.ConstantItem(120).AlignRight().Text(FormatPrice(subtotal, currencySymbol, sale.Currency));
                                });

                                if (taxPercent > 0)
                                {
                                    sumCol.Item().Row(r =>
                                    {
                                        r.RelativeItem().AlignRight().Text($"Impuesto ({taxPercent:F1}%):").FontColor(Colors.Grey.Darken2);
                                        r.ConstantItem(120).AlignRight().Text(FormatPrice(taxAmount, currencySymbol, sale.Currency));
                                    });
                                }

                                sumCol.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().AlignRight().Text("TOTAL:").FontSize(14).SemiBold().FontColor("#3F51B5");
                                    r.ConstantItem(120).AlignRight().Text(FormatPrice(sale.TotalAmount, currencySymbol, sale.Currency)).FontSize(14).SemiBold().FontColor("#3F51B5");
                                });
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private static string FormatPrice(decimal amount, string symbol, string currencyName)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                symbol = "$";
            }
            if (string.IsNullOrEmpty(currencyName))
            {
                currencyName = "COP";
            }
            return $"{symbol} {amount:N2} {currencyName}";
        }
    }
}
