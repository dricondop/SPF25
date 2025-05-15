using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using HeatProductionOptimization.Services;
using HeatProductionOptimization.Services.DataProviders;
using HeatProductionOptimization.Controls;
using ReactiveUI;

namespace HeatProductionOptimization.ViewModels;

public class ResultDataManagerViewModel : ViewModelBase
{
    private ResultsData _resultsData;
        
        public ResultsData ResultsData
        {
            get => _resultsData;
            set => this.RaiseAndSetIfChanged(ref _resultsData, value);
        }

        public ResultDataManagerViewModel()
        {
            _resultsData = new ResultsData();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var rand = new Random();
            var productionUnits = new List<string> { "GB", "OB", "GM", "EK", "HK" };

            for (int i = 0; i < 24; i++)
            {
                var timeStamp = DateTime.Now.Date.AddHours(i);
                var heatDemand = 100 + rand.NextDouble() * 200;
                var electricityPrice = 30 + rand.NextDouble() * 50;

                var production = new Dictionary<string, double>();
                foreach (var unit in productionUnits)
                {
                    production[unit] = rand.NextDouble() * 100;
                }

                var totalCost = 5000 + rand.NextDouble() * 10000;
                var totalEmission = 100 + rand.NextDouble() * 200;

                ResultsData.AddDataPoint(timeStamp, heatDemand, electricityPrice, production, totalCost, totalEmission);
            }
        }

        public async Task ExportDataToCsv()
        {
            try
            {
                var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (mainWindow == null) return;

                var topLevel = TopLevel.GetTopLevel(mainWindow);
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Data as CSV",
                    SuggestedFileName = $"OptimizationData.csv",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } }
                    }
                });

                if (file != null)
                {
                    await using var stream = await file.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    
                    writer.WriteLine("TimeStamp,HeatDemand,ElectricityPrice,TotalCost,TotalEmission,GB,OB,GM,EK,HK");
                    
                    for (int i = 0; i < ResultsData.TimeStamps.Count; i++)
                    {
                        var line = $"{ResultsData.TimeStamps[i]:yyyy-MM-dd HH:mm:ss}," +
                                   $"{ResultsData.HeatDemand[i]}," +
                                   $"{ResultsData.ElectricityPrice[i]}," +
                                   $"{ResultsData.TotalCosts[i]}," +
                                   $"{ResultsData.TotalEmissions[i]}," +
                                   $"{ResultsData.ProductionData[i]["GB"]}," +
                                   $"{ResultsData.ProductionData[i]["OB"]}," +
                                   $"{ResultsData.ProductionData[i]["GM"]}," +
                                   $"{ResultsData.ProductionData[i]["EK"]}," +
                                   $"{ResultsData.ProductionData[i]["HK"]}";
                        
                        writer.WriteLine(line);
                    }
                    
                    var messageBox = new MessageBox("Success", "Data exported successfully to CSV file.");
                    await messageBox.ShowDialog(mainWindow);
                }
            }
            catch (Exception ex)
            {
                var messageBox = new MessageBox("Error", $"Failed to export data: {ex.Message}");
                var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (mainWindow != null)
                {
                    await messageBox.ShowDialog(mainWindow);
                }
            }
        }

        public async Task GenerateAndSavePdfReport()
        {
            try
            {
                var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (mainWindow == null) return;

                var topLevel = TopLevel.GetTopLevel(mainWindow);
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Report as PDF",
                    SuggestedFileName = $"OptimizationReport.pdf",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("PDF File") { Patterns = new[] { "*.pdf" } }
                    }
                });

                if (file != null)
                {
                    await using var stream = await file.OpenWriteAsync();
                    await PdfReportGenerator.GenerateReport(ResultsData, stream);
                    
                    var messageBox = new MessageBox("Success", "PDF report generated successfully.");
                    await messageBox.ShowDialog(mainWindow);
                }
            }
            catch (Exception ex)
            {
                var messageBox = new MessageBox("Error", $"Failed to generate PDF report: {ex.Message}");
                var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (mainWindow != null)
                {
                    await messageBox.ShowDialog(mainWindow);
                }
            }
        }
}
