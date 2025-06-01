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
                                .Text("ELECTRICAL PERFORMANCE REPORT")
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
                    
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Confidential - ").FontColor(Colors.Grey.Darken1);
                            text.Span("Internal Use Only").Bold().FontColor(Colors.Red.Medium);
                        });
                });

                // Executive Summary page
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
                                              .Text("EXECUTIVE SUMMARY")
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
                                .PaddingBottom(20)
                                .Background(Colors.Grey.Lighten4)
                                .Padding(15)
                                .Column(col =>
                                {
                                    col.Item()
                                       .Text("KEY FINDINGS")
                                       .FontColor(Colors.Blue.Darken3)
                                       .Bold()
                                       .FontSize(14);
                                    
                                    col.Item()
                                       .PaddingTop(10)
                                       .Text("This report presents the analysis of electrical performance and heat production optimization conducted on:")
                                       .FontSize(11);
                                    
                                    col.Item()
                                       .PaddingVertical(5)
                                       .AlignCenter()
                                       .Text($"{DateTime.Now:MMMM dd, yyyy}")
                                       .Bold()
                                       .FontSize(12)
                                       .FontColor(Colors.Blue.Darken3);
                                    
                                    col.Item()
                                       .PaddingTop(10)
                                       .Text("The analysis covers the following aspects:")
                                       .FontSize(11);
                                    
                                    col.Item()
                                       .PaddingLeft(15)
                                       .PaddingTop(5)
                                       .Text("• Heat demand variation analysis")
                                       .FontSize(11);
                                    
                                    col.Item()
                                       .PaddingLeft(15)
                                       .Text("• Electricity price fluctuation impact")
                                       .FontSize(11);
                                    
                                    col.Item()
                                       .PaddingLeft(15)
                                       .Text("• Production unit performance metrics")
                                       .FontSize(11);
                                    
                                    col.Item()
                                       .PaddingLeft(15)
                                       .Text("• Optimization results comparison")
                                       .FontSize(11);
                                    
                                    col.Item()
                                       .PaddingTop(15)
                                       .Text("The following pages contain detailed charts and analysis for each section.")
                                       .Italic()
                                       .FontSize(11);
                                });
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
                                    .Text("The heat demand pattern shows typical consumption behavior with peaks during production hours and lower demand during night time. The maximum recorded demand was X MW at Y time.")
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
                                    .Text("Price peaks were observed during X hours, reaching Y €/MWh. The optimization algorithm successfully avoided operating high-cost units during these periods, resulting in estimated savings of Z €.")
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
                                    .Text("3. OPTIMIZATION RESULTS COMPARISON")
                                    .FontColor(Colors.Blue.Darken3)
                                    .Bold()
                                    .FontSize(14);
                                
                                column.Item()
                                    .PaddingBottom(10)
                                    .Text("Comparison between the optimal solution found and alternative scenarios.")
                                    .FontSize(11)
                                    .Italic();
                                
                                column.Item()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(5)
                                    .Image(chartImages["Optimization"]);
                                
                                column.Item()
                                    .AlignRight()
                                    .Text("Figure 3: Optimization results comparison")
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
                                    .Text("• Total cost reduction: X% compared to baseline")
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingLeft(15)
                                    .Text("• CO2 emissions reduction: Y%")
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingLeft(15)
                                    .Text("• Most utilized unit: Z (A% of total production)")
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
                                    .Text("• Most efficient unit: X (Y% efficiency)")
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingLeft(15)
                                    .Text("• Highest capacity utilization: Z (A% of total capacity)")
                                    .FontSize(11);
                                
                                column.Item()
                                    .PaddingLeft(15)
                                    .Text("• Recommended operational adjustments: ...")
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

                // Closing page
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
                            column.Item().Height(5, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Text("ANALYSIS COMPLETE")
                                .FontColor(Colors.Blue.Darken3)
                                .Bold()
                                .FontSize(24);
                            
                            column.Item().Height(3, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Width(300)
                                .Image(Placeholders.Image);
                            
                            column.Item().Height(3, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Text("Thank you for using")
                                .FontSize(14);
                            
                            column.Item()
                                .AlignCenter()
                                .Text("Heat Production Optimization System")
                                .Bold()
                                .FontSize(16)
                                .FontColor(Colors.Blue.Darken3);
                            
                            column.Item().Height(2, Unit.Centimetre);
                            
                            column.Item()
                                .AlignCenter()
                                .Text("For any questions, please contact:")
                                .FontSize(11)
                                .Italic();
                            
                            column.Item()
                                .AlignCenter()
                                .Text("technical.support@yourcompany.com")
                                .FontSize(12)
                                .FontColor(Colors.Blue.Darken3);
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
                            text.Span("Document ID: ").FontColor(Colors.Grey.Darken1);
                            text.Span(Guid.NewGuid().ToString().Substring(0, 8).ToUpper()).Bold().FontColor(Colors.Blue.Darken3);
                            text.Span(" | ").FontColor(Colors.Grey.Darken1);
                            text.Span("Page ").FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber().Bold().FontColor(Colors.Blue.Darken3);
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