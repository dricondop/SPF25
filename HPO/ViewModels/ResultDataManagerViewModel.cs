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
    }

    public async Task ExportDataToCsv()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return;
            }

            var mainWindow = desktopLifetime.MainWindow;
            if (mainWindow == null) return;

            // Get data from optimizer
            var assetManager = OptimizerViewModel.SharedAssetManager;
            if (assetManager == null) return;

            var allAssets = assetManager.GetAllAssets();
            if (allAssets == null) return;

            var activeAssets = allAssets.Values.Where(a => a.IsActive).ToList();
            
            if (activeAssets.Count == 0)
            {
                var messageBox = new MessageBox("Error", "No active production units found in optimizer data.");
                await messageBox.ShowDialog(mainWindow);
                return;
            }

            // Get all timestamps from the active assets
            var timestamps = activeAssets
                .SelectMany(a => a.ProducedHeat?.Keys ?? Enumerable.Empty<DateTime>())
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            if (topLevel == null) return;

            var storageProvider = topLevel.StorageProvider;
            if (storageProvider == null) return;

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
                    var heatDemand = activeAssets.FirstOrDefault()?.ProducedHeat?.TryGetValue(timestamp, out var demand) == true 
                        ? demand ?? 0 
                        : 0;

                    writer.Write($"{timestamp:yyyy-MM-dd HH:mm},{heatDemand}");

                    double totalCost = 0;
                    double totalEmissions = 0;

                    foreach (var asset in activeAssets)
                    {
                        var heatProduction = asset.ProducedHeat?.TryGetValue(timestamp, out var production) == true 
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
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime &&
                desktopLifetime.MainWindow != null)
            {
                await messageBox.ShowDialog(desktopLifetime.MainWindow);
            }
        }
    }

    public async Task GenerateAndSavePdfReport()
    {
        Dictionary<string, string>? chartImages = null;

        try
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return;
            }

            var mainWindow = desktopLifetime.MainWindow;
            if (mainWindow == null) return;

            chartImages = _dataVisualizationVM.GenerateAllCharts();

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            if (topLevel == null)
            {
                CleanUpTempImages(chartImages);
                return;
            }

            var storageProvider = topLevel.StorageProvider;
            if (storageProvider == null)
            {
                CleanUpTempImages(chartImages);
                return;
            }

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
                CleanUpTempImages(chartImages);
            }
        }
        catch (Exception ex)
        {
            var messageBox = new MessageBox("Error", $"Failed to generate PDF report: {ex.Message}");
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime &&
                desktopLifetime.MainWindow != null)
            {
                await messageBox.ShowDialog(desktopLifetime.MainWindow);
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