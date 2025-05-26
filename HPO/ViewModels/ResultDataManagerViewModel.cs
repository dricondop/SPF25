using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public async Task ExportDataToCsv()
    {
        try
        {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null) return;

            // Get data from optimizer
            var assetManager = OptimizerViewModel.SharedAssetManager;
            var activeAssets = assetManager.GetAllAssets().Values.Where(a => a.IsActive).ToList();
            
            if (activeAssets.Count == 0)
            {
                var messageBox = new MessageBox("Error", "No active production units found in optimizer data.");
                await messageBox.ShowDialog(mainWindow);
                return;
            }

            // Get all timestamps from the active assets
            var timestamps = activeAssets
                .SelectMany(a => a.ProducedHeat.Keys)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel?.StorageProvider?.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Data as CSV",
                SuggestedFileName = $"OptimizationData_{DateTime.Now:yyyyMMddHHmmss}.csv",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CSV File") { Patterns = new[] { "*.csv" } }
                }
            })!;

            if (file != null)
            {
                await using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream);

                // Write header
                writer.Write("Timestamp,Heat Demand");
                foreach (var asset in activeAssets)
                {
                    writer.Write($",{asset.Name} Heat Production");
                    writer.Write($",{asset.Name} Cost");
                    writer.Write($",{asset.Name} Emissions");
                }
                writer.WriteLine(",Total Cost,Total Emissions");

                // Write data rows
                foreach (var timestamp in timestamps)
                {
                    // Get heat demand (assuming it's available in one of the assets)
                    var heatDemand = activeAssets.FirstOrDefault()?.ProducedHeat.TryGetValue(timestamp, out var demand) == true 
                        ? demand ?? 0 
                        : 0;

                    writer.Write($"{timestamp:yyyy-MM-dd HH:mm},{heatDemand}");

                    double totalCost = 0;
                    double totalEmissions = 0;

                    foreach (var asset in activeAssets)
                    {
                        var heatProduction = asset.ProducedHeat.TryGetValue(timestamp, out var production) 
                            ? production ?? 0 
                            : 0;
                        var cost = asset.ProductionCost ?? 0;
                        var emissions = asset.CO2Emissions ?? 0;

                        writer.Write($",{heatProduction},{cost},{emissions}");

                        totalCost += cost;
                        totalEmissions += emissions;
                    }

                    writer.WriteLine($",{totalCost},{totalEmissions}");
                }

                var messageBox = new MessageBox("Success", "Optimizer data exported successfully to CSV file.");
                await messageBox.ShowDialog(mainWindow);
            }
        }
        catch (Exception ex)
        {
            var messageBox = new MessageBox("Error", $"Failed to export optimizer data: {ex.Message}");
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
