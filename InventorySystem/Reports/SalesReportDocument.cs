using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace InventorySystem.Reports
{
    public class SalesReportDocument : IDocument
    {
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
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
        }

        private void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(24).SemiBold().FontColor(Color.FromHex("3F51B5"));

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("INVENTORY SYSTEM MB").Style(titleStyle);
                    column.Item().Text(text =>
                    {
                        text.Span("Fecha: ").SemiBold();
                        text.Span($"{DateTime.Now:dd/MM/yyyy}");
                    });
                });

                row.ConstantItem(100).Height(50).Placeholder(); // Placeholder for Logo
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(20);

                // Summary Cards Row
                column.Item().Row(row =>
                {
                    row.Spacing(20);
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Ventas Totales", "$12,450.00", Color.FromHex("EEF2FF"), Color.FromHex("4F46E5")));
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Pedidos", "154", Color.FromHex("F0FDF4"), Color.FromHex("16A34A")));
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Ticket Promedio", "$80.84", Color.FromHex("FFF7ED"), Color.FromHex("EA580C")));
                });

                // Products Table
                column.Item().Text("Desglose de Productos Vendidos").FontSize(14).SemiBold();
                
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#");
                        header.Cell().Element(CellStyle).Text("Producto");
                        header.Cell().Element(CellStyle).Text("Cantidad");
                        header.Cell().Element(CellStyle).Text("Precio Unit.");
                        header.Cell().Element(CellStyle).Text("Total");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black);
                        }
                    });

                    // Sample Data
                    for (int i = 1; i <= 5; i++)
                    {
                        table.Cell().Element(ItemStyle).Text(i.ToString());
                        table.Cell().Element(ItemStyle).Text($"Producto de Ejemplo {i}");
                        table.Cell().Element(ItemStyle).Text("10");
                        table.Cell().Element(ItemStyle).Text("$50.00");
                        table.Cell().Element(ItemStyle).Text("$500.00");

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
