using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using InventorySystem.Models;

namespace InventorySystem.Reports
{
    public class ClientsReportDocument : IDocument
    {
        private readonly IEnumerable<Client> _clients;

        public ClientsReportDocument(IEnumerable<Client> clients)
        {
            _clients = clients ?? new List<Client>();
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
                    column.Item().Text("CLIENTS DIRECTORY").Style(titleStyle);
                    column.Item().Text(text =>
                    {
                        text.Span("Date: ").SemiBold();
                        text.Span($"{DateTime.Now:MM/dd/yyyy}");
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Report Type: ").SemiBold();
                        text.Span("Customer Base Analysis");
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

                var totalClients = _clients.Count();
                var activeClients = _clients.Count(c => c.IsActive);
                var inactiveClients = totalClients - activeClients;

                // Summary Cards Row
                column.Item().Row(row =>
                {
                    row.Spacing(20);
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Total Clients", totalClients.ToString(), Color.FromHex("EEF2FF"), Color.FromHex("4F46E5")));
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Active", activeClients.ToString(), Color.FromHex("F0FDF4"), Color.FromHex("16A34A")));
                    row.RelativeItem().Element(c => ComposeSummaryCard(c, "Inactive", inactiveClients.ToString(), Color.FromHex("FEF2F2"), Color.FromHex("DC2626")));
                });

                // Clients Table
                column.Item().Text("Client Details").FontSize(14).SemiBold();
                
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("ID");
                        header.Cell().Element(CellStyle).Text("Full Name");
                        header.Cell().Element(CellStyle).Text("Email");
                        header.Cell().Element(CellStyle).Text("Phone");
                        header.Cell().Element(CellStyle).Text("Status");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black);
                        }
                    });

                    foreach (var client in _clients.OrderBy(c => c.LastName))
                    {
                        table.Cell().Element(ItemStyle).Text(client.Id.ToString());
                        table.Cell().Element(ItemStyle).Text(client.FullName);
                        table.Cell().Element(ItemStyle).Text(client.Email);
                        table.Cell().Element(ItemStyle).Text(client.PhoneNumber);
                        table.Cell().Element(ItemStyle).Text(client.IsActive ? "Active" : "Inactive");

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
