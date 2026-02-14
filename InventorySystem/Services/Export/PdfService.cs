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
        public Task GenerateInvoiceAsync(Sale sale, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Size(PageSizes.A4);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("INVENTORY SYSTEM").FontSize(20).SemiBold().FontColor("#3F51B5");
                                col.Item().Text("Soluciones de Inventario Profesionales").FontSize(10).Italic();
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"FACTURA #{sale.Id:D6}").FontSize(16).SemiBold().AlignRight();
                                col.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy}").AlignRight();
                            });
                        });

                        page.Content().PaddingVertical(25).Column(col =>
                        {
                            // Client Info
                            col.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(clientCol =>
                                {
                                    clientCol.Item().Text("CLIENTE:").SemiBold().FontSize(12);
                                    clientCol.Item().Text(sale.Client?.FullName ?? "Cliente General");
                                    if (!string.IsNullOrEmpty(sale.Client?.Email))
                                        clientCol.Item().Text(sale.Client.Email);
                                });
                            });

                            // Table
                            col.Item().Table(table =>
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
                                    table.Cell().Element(ItemStyle).AlignRight().Text($"{detail.UnitPrice:C2}");
                                    table.Cell().Element(ItemStyle).AlignRight().Text($"{detail.TotalPrice:C2}");

                                    static IContainer ItemStyle(IContainer container)
                                    {
                                        return container.PaddingVertical(5)
                                                        .BorderBottom(1)
                                                        .BorderColor(Colors.Grey.Lighten4);
                                    }
                                }
                            });

                            // Summary
                            col.Item().AlignRight().PaddingTop(20).Column(sumCol =>
                            {
                                sumCol.Item().Text($"TOTAL: {sale.TotalAmount:C2}").FontSize(14).SemiBold();
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
    }
}
