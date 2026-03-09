using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Models;

namespace InventorySystem.Reports
{
    public class InventoryReportDocument : IDocument
    {
        private readonly IEnumerable<Product> _products;

        public InventoryReportDocument(IEnumerable<Product> products)
        {
            _products = products ?? new List<Product>();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
        }

        private void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(24).SemiBold().FontColor(Color.FromHex("4F46E5"));

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INVENTORY VALUATION").Style(titleStyle);
                    column.Item().Text(text =>
                    {
                        text.Span("Date: ").SemiBold();
                        text.Span($"{DateTime.Now:MM/dd/yyyy}");
                    });
                });

                row.ConstantItem(100).Height(50).Placeholder();
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(20);

                var totalItems = _products.Count();
                var totalStockValue = _products.Sum(p => p.Price * p.Quantity);
                var lowStockAlerts = _products.Count(p => p.Quantity < 10);

                // Summary Cards Row
                column.Item().Row(row =>
                {
                    row.Spacing(20);
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Products", totalItems.ToString(), Color.FromHex("EEF2FF"), Color.FromHex("4F46E5")));
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Inventory Value", $"${totalStockValue:N2}", Color.FromHex("EEF2FF"), Color.FromHex("4F46E5")));
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Low Stock Alerts", lowStockAlerts.ToString(), Color.FromHex("FEF2F2"), Color.FromHex("DC2626")));
                });

                // Products Table
                column.Item().Text("Current Stock Details").FontSize(14).SemiBold();
                
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("ID");
                        header.Cell().Element(CellStyle).Text("Product Name");
                        header.Cell().Element(CellStyle).Text("Category");
                        header.Cell().Element(CellStyle).Text("Price");
                        header.Cell().Element(CellStyle).Text("Stock");
                        header.Cell().Element(CellStyle).Text("Value");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black);
                        }
                    });

                    foreach (var product in _products.OrderBy(p => p.Quantity))
                    {
                        var rowStyle = product.Quantity < 10 ? TextStyle.Default.FontColor(Colors.Red.Medium) : TextStyle.Default;

                        table.Cell().Element(ItemStyle).Text(product.Id.ToString()).Style(rowStyle);
                        table.Cell().Element(ItemStyle).Text(product.Name).Style(rowStyle);
                        table.Cell().Element(ItemStyle).Text(product.Category).Style(rowStyle);
                        table.Cell().Element(ItemStyle).Text($"${product.Price:N2}").Style(rowStyle);
                        table.Cell().Element(ItemStyle).Text(product.Quantity.ToString()).Style(rowStyle);
                        table.Cell().Element(ItemStyle).Text($"${(product.Price * product.Quantity):N2}").Style(rowStyle);

                        static IContainer ItemStyle(IContainer container)
                        {
                            return container.BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(5);
                        }
                    }
                });
            });
        }

        private void ComposeSummaryCard(IContainer container, string title, string value, string bgColor, string textColor)
        {
            container.Background(bgColor).Padding(15).CornerRadius(8).Column(column =>
            {
                column.Item().Text(title).FontSize(10).FontColor(textColor).SemiBold();
                column.Item().Text(value).FontSize(18).FontColor(textColor).Bold();
            });
        }
    }
}
