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
                // Cover page
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Calibri"));

                    page.Header().Height(1, Unit.Centimetre);
                    
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Item().Height(2, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Text("HEAT PRODUCTION OPTIMIZATION REPORT")
                                .FontColor(Colors.Blue.Darken3)
                                .Bold()
                                .FontSize(24);
                            
                            column.Item().Height(3, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Text("Heat Production Optimization System")
                                .FontColor(Colors.Grey.Darken2)
                                .FontSize(18);
                            
                            column.Item().Height(5, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("Generated on ").FontColor(Colors.Grey.Darken1);
                                    text.Span($"{DateTime.Now:MMMM dd, yyyy}").Bold().FontColor(Colors.Blue.Darken3);
                                });
                            
                            column.Item().Height(4, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Width(150)
                                .Image(Placeholders.Image);
                        });
                }); 

                // Heat Demand page
                if (chartImages.ContainsKey("HeatDemand"))
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Calibri"));

                        page.Header()
                            .Height(80)
                            .Background(Colors.White)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(5)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                           .AlignLeft()
                                           .Width(60)
                                           .Image(Placeholders.Image);

                                        row.RelativeItem()
                                           .AlignCenter()
                                           .Column(col =>
                                           {
                                               col.Item()
                                                  .Text("HEAT DEMAND ANALYSIS")
                                                  .FontColor(Colors.Blue.Darken3)
                                                  .Bold()
                                                  .FontSize(18);
                                           });
                                    });

                                column.Item()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Blue.Darken3);
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(15)
                                    .Text("1. HEAT DEMAND VARIATION")
                                    .FontColor(Colors.Blue.Darken3)
                                    .Bold()
                                    .FontSize(14);

                                column.Item()
                                    .PaddingBottom(10)
                                    .Text("The following chart shows the heat demand variation throughout the analyzed period.")
                                    .FontSize(11)
                                    .Italic();

                                column.Item()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Image(chartImages["HeatDemand"]);

                                column.Item()
                                    .AlignRight()
                                    .Text("Figure 1: Heat demand variation chart")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);

                                column.Item()
                                    .PaddingTop(15)
                                    .Background(Colors.Grey.Lighten4)
                                    .Padding(10)
                                    .Text("Analysis:")
                                    .Bold()
                                    .FontSize(11);

                                column.Item()
                                    .PaddingTop(5)
                                    .Text("") // Insert stats here
                                    .FontSize(11);
                            });

                        page.Footer()
                            .Height(30)
                            .Background(Colors.White)
                            .BorderTop(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(text =>
                            {
                                text.Span("Page ").FontColor(Colors.Grey.Darken1);
                                text.CurrentPageNumber().Bold().FontColor(Colors.Blue.Darken3);
                            });
                    });
                }

                // Electricity Price page
                if (chartImages.ContainsKey("ElectricityPrice"))
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Calibri"));

                        page.Header()
                            .Height(80)
                            .Background(Colors.White)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(5)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                           .AlignLeft()
                                           .Width(60)
                                           .Image(Placeholders.Image);
                                        
                                        row.RelativeItem()
                                           .AlignCenter()
                                           .Column(col =>
                                           {
                                               col.Item()
                                                  .Text("ELECTRICITY PRICE ANALYSIS")
                                                  .FontColor(Colors.Blue.Darken3)
                                                  .Bold()
                                                  .FontSize(18);
                                           });
                                    });
                                
                                column.Item()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Blue.Darken3);
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(15)
                                    .Text("2. ELECTRICITY PRICE FLUCTUATION")
                                    .FontColor(Colors.Blue.Darken3)
                                    .Bold()
                                    .FontSize(14);
                                
                                column.Item()
                                    .PaddingBottom(10)
                                    .Text("The following chart shows electricity price variations and their impact on optimization.")
                                    .FontSize(11)
                                    .Italic();
                                
                                column.Item()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Image(chartImages["ElectricityPrice"]);
                                
                                column.Item()
                                    .AlignRight()
                                    .Text("Figure 2: Electricity price variation chart")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                
                                column.Item()
                                    .PaddingTop(15)
                                    .Background(Colors.Grey.Lighten4)
                                    .Padding(10)
                                    .Text("Analysis:")
                                    .Bold()
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingTop(5)
                                    .Text("") // Insert stats here
                                    .FontSize(11);
                            });

                        page.Footer()
                            .Height(30)
                            .Background(Colors.White)
                            .BorderTop(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(text =>
                            {
                                text.Span("Page ").FontColor(Colors.Grey.Darken1);
                                text.CurrentPageNumber().Bold().FontColor(Colors.Blue.Darken3);
                            });
                    });
                }

                // Optimization Results page
                if (chartImages.ContainsKey("Optimization"))
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Calibri"));

                        page.Header()
                            .Height(80)
                            .Background(Colors.White)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(5)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                           .AlignLeft()
                                           .Width(60)
                                           .Image(Placeholders.Image);
                                        
                                        row.RelativeItem()
                                           .AlignCenter()
                                           .Column(col =>
                                           {
                                               col.Item()
                                                  .Text("OPTIMIZATION RESULTS")
                                                  .FontColor(Colors.Blue.Darken3)
                                                  .Bold()
                                                  .FontSize(18);
                                           });
                                    });
                                
                                column.Item()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Blue.Darken3);
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(15)
                                    .Text("3. OPTIMIZATION RESULTS")
                                    .FontColor(Colors.Blue.Darken3)
                                    .Bold()
                                    .FontSize(14);
                                
                                column.Item()
                                    .PaddingBottom(10)
                                    .Text("Optimal distribution of production units in the desired scenario")
                                    .FontSize(11)
                                    .Italic();
                                
                                column.Item()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Image(chartImages["Optimization"]);
                                
                                column.Item()
                                    .AlignRight()
                                    .Text("Figure 3: Optimization results chart")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                
                                column.Item()
                                    .PaddingTop(15)
                                    .Background(Colors.Grey.Lighten4)
                                    .Padding(10)
                                    .Text("Key Outcomes:")
                                    .Bold()
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingLeft(15)
                                    .PaddingTop(5)
                                    .Text("") // Insertar stats aquí
                                    .FontSize(11);
                            });

                        page.Footer()
                            .Height(30)
                            .Background(Colors.White)
                            .BorderTop(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(text =>
                            {
                                text.Span("Page ").FontColor(Colors.Grey.Darken1);
                                text.CurrentPageNumber().Bold().FontColor(Colors.Blue.Darken3);
                            });
                    });
                }

                // Production Performance page
                if (chartImages.ContainsKey("Production"))
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Calibri"));

                        page.Header()
                            .Height(80)
                            .Background(Colors.White)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(5)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                           .AlignLeft()
                                           .Width(60)
                                           .Image(Placeholders.Image);
                                        
                                        row.RelativeItem()
                                           .AlignCenter()
                                           .Column(col =>
                                           {
                                               col.Item()
                                                  .Text("PRODUCTION UNIT PERFORMANCE")
                                                  .FontColor(Colors.Blue.Darken3)
                                                  .Bold()
                                                  .FontSize(18);
                                           });
                                    });
                                
                                column.Item()
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Blue.Darken3);
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Item()
                                    .PaddingBottom(15)
                                    .Text("4. PRODUCTION UNIT PERFORMANCE METRICS")
                                    .FontColor(Colors.Blue.Darken3)
                                    .Bold()
                                    .FontSize(14);
                                
                                column.Item()
                                    .PaddingBottom(10)
                                    .Text("Comparative performance of different production units during the analyzed period.")
                                    .FontSize(11)
                                    .Italic();
                                
                                column.Item()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Image(chartImages["Production"]);
                                
                                column.Item()
                                    .AlignRight()
                                    .Text("Figure 4: Production unit performance chart")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                
                                column.Item()
                                    .PaddingTop(15)
                                    .Background(Colors.Grey.Lighten4)
                                    .Padding(10)
                                    .Text("Performance Highlights:")
                                    .Bold()
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingLeft(15)
                                    .PaddingTop(5)
                                    .Text("")  // Insertar stats aquí
                                    .FontSize(11);
                            });

                        page.Footer()
                            .Height(30)
                            .Background(Colors.White)
                            .BorderTop(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text(text =>
                            {
                                text.Span("Page ").FontColor(Colors.Grey.Darken1);
                                text.CurrentPageNumber().Bold().FontColor(Colors.Blue.Darken3);
                            });
                    });
                }
            });

            await Task.Run(() => 
            {
                document.GeneratePdf(outputStream);
            });
        }
    }
}