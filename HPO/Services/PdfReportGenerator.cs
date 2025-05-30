using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Services
{
    public static class PdfReportGenerator
    {
        public static async Task GenerateReport(Dictionary<string, string> chartImages, Stream outputStream)
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
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            if (chartImages.ContainsKey("HeatDemand"))
                            {
                                column.Item()
                                    .PaddingBottom(15)
                                    .AlignCenter()
                                    .Text("Heat Demand Data")
                                    .FontSize(14)
                                    .Bold();
                                
                                column.Item()
                                    .Image(chartImages["HeatDemand"]);
                            }

                            if (chartImages.ContainsKey("ElectricityPrice"))
                            {
                                column.Item()
                                    .PaddingTop(20)
                                    .PaddingBottom(15)
                                    .AlignCenter()
                                    .Text("Electricity Price Data")
                                    .FontSize(14)
                                    .Bold();
                                
                                column.Item()
                                    .Image(chartImages["ElectricityPrice"]);
                            }

                            if (chartImages.ContainsKey("Optimization"))
                            {
                                column.Item()
                                    .PaddingTop(20)
                                    .PaddingBottom(15)
                                    .AlignCenter()
                                    .Text("Optimization Results")
                                    .FontSize(14)
                                    .Bold();
                                
                                column.Item()
                                    .Image(chartImages["Optimization"]);
                            }

                            if (chartImages.ContainsKey("Production"))
                            {
                                column.Item()
                                    .PaddingTop(20)
                                    .PaddingBottom(15)
                                    .AlignCenter()
                                    .Text("Production Unit Performance")
                                    .FontSize(14)
                                    .Bold();
                                
                                column.Item()
                                    .Image(chartImages["Production"]);
                            }
                        });

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
                });
            });

            await Task.Run(() => 
            {
                document.GeneratePdf(outputStream);
            });
        }
    }
}