using GoHijauBackend.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GoHijauBackend.Application.Services
{
    public class InvoiceDocument : IDocument
    {
        private readonly Invoice _invoice;
        private readonly Organization _org;

        public InvoiceDocument(Invoice invoice, Organization org)
        {
            _invoice = invoice;
            _org = org;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("ATIA ROBOTICS SDN BHD • GoHijau Platform");
                });
            });
        }

        void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INVOICE")
                        .FontSize(22)
                        .Bold()
                        .FontColor(InvoiceTheme.PrimaryGreen);

                    col.Item().Text($"Invoice No: {_invoice.InvoiceId}");
                    col.Item().Text($"Date: {_invoice.CreatedAt:dd MMM yyyy}");
                });

                row.ConstantItem(200).Column(col =>
                {
                    col.Item().AlignRight().Text("ATIA ROBOTICS SDN BHD").Bold();
                    col.Item().AlignRight().Text("Malaysia");
                    col.Item().AlignRight().Text("billing@atia.com");
                });
            });
        }

        void ComposeContent(QuestPDF.Infrastructure.IContainer container)
        {
            container.PaddingVertical(20).Column(col =>
            {
                col.Item().Element(ComposeBillTo);
                col.Item().PaddingVertical(15).Element(ComposeTable);
                col.Item().AlignRight().Element(ComposeTotal);
            });
        }

        void ComposeBillTo(QuestPDF.Infrastructure.IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Bill To").Bold();
                col.Item().Text(_org.OrganizationName);
            });
        }

        void ComposeTable(QuestPDF.Infrastructure.IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Description").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Quantity (KG)").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Rate (RM)").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Amount (RM)").Bold();
                });

                table.Cell().Element(CellStyle).Text("Used Cooking Oil Collection");
                table.Cell().Element(CellStyle).AlignRight().Text(_invoice.OilCollected.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text(_invoice.CollectorRate.ToString("N2"));
                table.Cell().Element(CellStyle).AlignRight().Text(_invoice.TotalAmount.ToString("N2"));

                static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                {
                    return container
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(8);
                }
            });
        }

        void ComposeTotal(QuestPDF.Infrastructure.IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Total:");
                    row.ConstantItem(120).AlignRight()
                        .Text($"RM {_invoice.TotalAmount:N2}")
                        .Bold()
                        .FontSize(14)
                        .FontColor(InvoiceTheme.PrimaryGreen);
                });

                col.Item().PaddingTop(10).Text("Payment due within 14 days.");
            });
        }
    }
}
