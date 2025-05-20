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
using HeatProductionOptimization.Models;

namespace HeatProductionOptimization.ViewModels;

public class ResultDataManagerViewModel : ViewModelBase
{
    private ResultsData _resultsData;
    private readonly DataVisualizationViewModel _dataVisualizationVM;

    public ResultsData ResultsData
    {
        get => _resultsData;
        set => this.RaiseAndSetIfChanged(ref _resultsData, value);
    }

    public ResultDataManagerViewModel()
    {
        _resultsData = new ResultsData();
        _dataVisualizationVM = new DataVisualizationViewModel();
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

            _resultsData?.AddDataPoint(timeStamp, heatDemand, electricityPrice, production, totalCost, totalEmission);
        }
    }

    public async Task ExportDataToCsv()
    {
        try
        {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null || ResultsData == null) return;

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel?.StorageProvider?.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Data as CSV",
                SuggestedFileName = $"OptimizationData.csv",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } }
                }
            })!;

            if (file != null)
            {
                await using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream);

                writer.WriteLine("TimeStamp,HeatDemand,ElectricityPrice,TotalCost,TotalEmission,GB,OB,GM,EK,HK");

                int count = Math.Min(
                    ResultsData.TimeStamps?.Count ?? 0,
                    ResultsData.ProductionData?.Count ?? 0
                );

                for (int i = 0; i < count; i++)
                {
                    var prod = ResultsData.ProductionData?[i];

                    string line = $"{ResultsData.TimeStamps?[i]:dd-MM-yyyy HH:mm:ss}," +
                                  $"{ResultsData.HeatDemand?[i]}," +
                                  $"{ResultsData.ElectricityPrice?[i]}," +
                                  $"{ResultsData.TotalCosts?[i]}," +
                                  $"{ResultsData.TotalEmissions?[i]}," +
                                  $"{prod?["GB"] ?? 0}," +
                                  $"{prod?["OB"] ?? 0}," +
                                  $"{prod?["GM"] ?? 0}," +
                                  $"{prod?["EK"] ?? 0}," +
                                  $"{prod?["HK"] ?? 0}";

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
        Dictionary<string, string>? chartImages = null;

        try
        {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            chartImages = _dataVisualizationVM.GenerateAllCharts();

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel?.StorageProvider?.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Report as PDF",
                SuggestedFileName = $"OptimizationReport.pdf",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PDF File") { Patterns = new[] { "*.pdf" } }
                }
            })!;

            if (file != null)
            {
                await using var stream = await file.OpenWriteAsync();
                if (chartImages != null)
                {
                    await PdfReportGenerator.GenerateReport(chartImages, stream);
                    CleanUpTempImages(chartImages);
                }

                var messageBox = new MessageBox("Success", "PDF report generated successfully.");
                await messageBox.ShowDialog(mainWindow);
            }
            else
            {
                if (chartImages != null)
                    CleanUpTempImages(chartImages);
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

    private void CleanUpTempImages(Dictionary<string, string>? chartImages)
    {
        try
        {
            if (chartImages == null) return;

            foreach (var imagePath in chartImages.Values)
            {
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up temp images: {ex.Message}");
        }
    }
}
