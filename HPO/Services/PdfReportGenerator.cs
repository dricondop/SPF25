using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeatProductionOptimization.Services.DataProviders;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HeatProductionOptimization.Services
{
    public static class PdfReportGenerator
    {
        public static async Task GenerateReport(ResultsData data, Stream outputStream)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Encabezado mejorado
                    page.Header()
                        .PaddingBottom(15)
                        .Row(row =>
                        {
                            row.RelativeItem()
                               .Height(50)
                               .Background(Colors.Blue.Darken3)
                               .AlignCenter()
                               .AlignMiddle()
                               .Text("Heat Production Optimization Report")
                               .FontColor(Colors.White)
                               .Bold()
                               .FontSize(18);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Resumen ejecutivo
                            column.Item()
                                .Background(Colors.Grey.Lighten3)
                                .Padding(10)
                                .Column(summaryColumn =>
                                {
                                    summaryColumn.Item().Text("Executive Summary").Bold().FontSize(16);
                                    summaryColumn.Spacing(5);
                                    
                                    summaryColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Average Electricity Price:");
                                        row.RelativeItem().Text($"{data.ElectricityPrice.Average():F2} DKK/MWh").SemiBold();
                                    });
                                    
                                    summaryColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Average Heat Demand:");
                                        row.RelativeItem().Text($"{data.HeatDemand.Average():F2} kWh").SemiBold();
                                    });
                                    
                                    summaryColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Total Emissions:");
                                        row.RelativeItem().Text($"{data.TotalEmissions.Sum():F2} kg CO2").SemiBold();
                                    });
                                    
                                    summaryColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Total Cost:");
                                        row.RelativeItem().Text($"{data.TotalCosts.Sum():F2} DKK").SemiBold();
                                    });
                                });

                            // Gráfico 1: Precio de electricidad
                            column.Item().PageBreak();
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(chartColumn =>
                                {
                                    chartColumn.Item().Text("Electricity Price Over Time").Bold().FontSize(14);
                                    chartColumn.Item().Text("Price (DKK/MWh)").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    chartColumn.Item().Height(200).Image(Placeholders.Image(600, 200));
                                });

                            // Espaciado entre gráficos
                            column.Item().PaddingTop(15);
                            
                            // Gráfico 2: Demanda de calor
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(chartColumn =>
                                {
                                    chartColumn.Item().Text("Heat Demand Over Time").Bold().FontSize(14);
                                    chartColumn.Item().Text("Demand (kWh)").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    chartColumn.Item().Height(200).Image(Placeholders.Image(600, 200));
                                });

                            // Gráfico 3: Distribución de producción
                            column.Item().PageBreak();
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(chartColumn =>
                                {
                                    chartColumn.Item().Text("Production Distribution by Asset").Bold().FontSize(14);
                                    chartColumn.Item().Text("Percentage (%)").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    chartColumn.Item().Height(200).Image(Placeholders.Image(600, 200));
                                });

                            // Espaciado entre gráficos
                            column.Item().PaddingTop(15);
                            
                            // Gráfico 4: Emisiones
                            column.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Column(chartColumn =>
                                {
                                    chartColumn.Item().Text("Emissions Over Time").Bold().FontSize(14);
                                    chartColumn.Item().Text("CO2 Emissions (kg)").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    chartColumn.Item().Height(200).Image(Placeholders.Image(600, 200));
                                });

                            // Notas finales
                            column.Item()
                                .PaddingTop(20)
                                .Text("* For detailed numerical data, please export the CSV file from the application.")
                                .Italic()
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                        });

                    // Pie de página mejorado
                    page.Footer()
                        .Height(30)
                        .Background(Colors.Grey.Lighten4)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(text =>
                        {
                            text.Span($"Report generated on {DateTime.Now:dd-MM-yyyy HH:mm} | ");
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                        
                    // Eliminamos el .FontSize(10) que causaba el problema
                    // y lo movemos dentro del método .Text()
                });
            });

            // Generación del PDF
            await Task.Run(() => 
            {
                document.GeneratePdf(outputStream);
            });
        }
    }
}