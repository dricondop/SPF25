﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using CommunityToolkit.Mvvm.Input;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using ReactiveUI;
using SkiaSharp;
using Avalonia.Remote.Protocol.Designer;
using System.IO;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

namespace HeatProductionOptimization.ViewModels
{
    public class DataVisualizationViewModel : ViewModelBase
    {
        // Observable collections for chart series and axes
        public ObservableCollection<ISeries> CartesianSeries { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new();
        public ObservableCollection<Axis> YAxes { get; set; } = new();

        // Available data sources and chart types for selection
        public ObservableCollection<string> AvailableDataSources { get; set; } = new()
        {
            "Optimization Results",
            "Heat Demand Data",
            "Electricity Price Data",
            "Production Unit Performance"
        };
        public ObservableCollection<string> AvailableChartTypes { get; set; } = new()
        {
            "Line Chart",
            "Bar Chart",
            "Scatter Plot"
        };
        public ObservableCollection<string> FilteredChartTypes { get; set; } = new();

        // Selected properties
        private string? _selectedDataSource;
        public string SelectedDataSource
        {
            get => _selectedDataSource ?? string.Empty;
            set
            {
                var changed = this.RaiseAndSetIfChanged(ref _selectedDataSource, value);
                UpdateChartTypes();
                SelectedChartType = FilteredChartTypes.FirstOrDefault() ?? string.Empty;
                this.RaisePropertyChanged(nameof(SelectedChartType));
            }
        }

        public string SelectedChartType { get; set; } = "Bar Chart";

        // Chart Settings
        private bool _showLegend = true;
        private bool _showDataLabels = true;
        private bool _showGridLines = true;
        private bool _autoScale = true;
        private bool _enableZoom = true;

        // Prepared data for chart rendering
        private string _preparedYAxisTitle = "";
        private string _preparedXAxisTitle = "";
        private List<double> _preparedValues = new();
        private List<string> _preparedLabels = new();
        
        // Managers
        private readonly AssetManager _assetManager;
        private readonly SourceDataManager _sourceDataManager;

        // Commands
        public ICommand UpdateChartCommand { get; }
        public ICommand SaveChartImageCommand { get; }
        public ICommand ResetZoomCommand { get; }

        // Properties
        public double ChartWidth => CalculateOptimalChartWidth();
        public ZoomAndPanMode ZoomMode => _enableZoom ? ZoomAndPanMode.X : ZoomAndPanMode.None;
        public LegendPosition ChartLegendPosition =>
            ShowLegend ? LegendPosition.Right : LegendPosition.Hidden;

        public bool ShowLegend
        {
            get => _showLegend;
            set
            {
                this.RaiseAndSetIfChanged(ref _showLegend, value);
                this.RaisePropertyChanged(nameof(ChartLegendPosition));
            }
        }

        public bool ShowDataLabels
        {
            get => _showDataLabels;
            set
            {
                this.RaiseAndSetIfChanged(ref _showDataLabels, value);
                UpdateAxisTitles();
            }
        }

        public bool ShowGridLines
        {
            get => _showGridLines;
            set
            {
                this.RaiseAndSetIfChanged(ref _showGridLines, value);
                UpdateGridLines();
            }
        }

        public bool AutoScale
        {
            get => _autoScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _autoScale, value);
                UpdateYAxisLimits();
            }
        }

        public bool EnableZoom
        {
            get => _enableZoom;
            set
            {
                this.RaiseAndSetIfChanged(ref _enableZoom, value);
                this.RaisePropertyChanged(nameof(ZoomMode));
            }
        }


        public DataVisualizationViewModel()
        {
            _assetManager = OptimizerViewModel.SharedAssetManager;
            _sourceDataManager = SourceDataManager.sourceDataManagerInstance;

            SelectedDataSource = "Optimization Results";
            FilteredChartTypes = new ObservableCollection<string>(AvailableChartTypes);

            UpdateChartCommand = new RelayCommand(UpdateChart);
            SaveChartImageCommand = new RelayCommand(SaveChartImage);
            ResetZoomCommand = new RelayCommand(ResetZoom);

            // Initialize default chart
            CartesianSeries.Add(new LineSeries<double> { Values = new List<double> { 0 } });
            XAxes.Add(new Axis { Labels = new[] { "Start" }, Name = "Units" });
            YAxes.Add(new Axis { Name = "Value" });
        }

        // Method to refresh data based on the selected data source
        private void UpdateChart()
        {
            ClearChartData();

            var (startDate, endDate) = GetDateRange();

            switch (SelectedDataSource)
            {
                case "Optimization Results":
                    UpdateOptimizationResultsChart(startDate, endDate);
                    break;
                case "Heat Demand Data":
                    CreateDemandOrPriceChart(startDate, endDate, "Heat Demand", "Heat Demand (MWh)", SKColors.SteelBlue);
                    break;
                case "Electricity Price Data":
                    CreateDemandOrPriceChart(startDate, endDate, "Electricity Price", "Electricity Price (DKK/kWh)", SKColors.DarkOrange);
                    break;
                case "Production Unit Performance":
                    UpdateProductionPerformanceChart();
                    break;
            }

            RefreshChart();
        }

        private void ClearChartData()
        {
            _preparedValues.Clear();
            _preparedLabels.Clear();
            _preparedYAxisTitle = "";
            _preparedXAxisTitle = "";
            CartesianSeries.Clear();
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            DateTime defaultStart = new(2024, 3, 1);
            DateTime defaultEnd = new(2024, 8, 25);

            try
            {
                var optimizerVM = new OptimizerViewModel();
                if (optimizerVM.StartDate.HasValue && optimizerVM.EndDate.HasValue)
                {
                    return (optimizerVM.StartDate.Value.DateTime, optimizerVM.EndDate.Value.DateTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting dates: {ex.Message}");
            }

            return (defaultStart, defaultEnd);
        }

        private void UpdateOptimizationResultsChart(DateTime startDate, DateTime endDate)
        {
            var assets = _assetManager.GetAllAssets().Values.Where(a => a.IsActive).ToList();
            if (assets.Count == 0) return;

            var timestamps = assets
                .SelectMany(a => a.ProducedHeat.Keys)
                .Where(t => t >= startDate && t <= endDate)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            if (timestamps.Count == 0) return;

            _preparedLabels = timestamps.Select(t => t.ToString("dd-MM HH:mm")).ToList();
            _preparedXAxisTitle = "Time";
            _preparedYAxisTitle = "Produced Heat (MWh)";

            var colors = GenerateDistinctColors(assets.Count);
            double pointSize = CalculatePointSize();

            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var values = timestamps.Select(t => asset.ProducedHeat.TryGetValue(t, out var v) ? v ?? 0 : 0).ToList();

                AddSeriesToChart(asset.Name, values, colors[i], pointSize);
            }

            _preparedValues = assets
                .SelectMany(a => a.ProducedHeat
                    .Where(e => e.Key >= startDate && e.Key <= endDate)
                    .Select(e => e.Value ?? 0))
                .ToList();

            SetAxes(_preparedLabels);
            
            this.RaisePropertyChanged(nameof(ZoomMode));
        }

        private void AddSeriesToChart(string name, List<double> values, SKColor color, double pointSize)
        {
            ISeries series = SelectedChartType switch
            {
                "Line Chart" => new LineSeries<double>
                {
                    Name = name,
                    Values = values,
                    GeometrySize = pointSize,
                    Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                    GeometryStroke = new SolidColorPaint(color) { StrokeThickness = 1 },
                    GeometryFill = new SolidColorPaint(color.WithAlpha(180)),
                    LineSmoothness = 0
                },
                "Bar Chart" => new ColumnSeries<double>
                {
                    Name = name,
                    Values = values,
                    MaxBarWidth = 20,
                    Stroke = null,
                    Fill = new SolidColorPaint(color.WithAlpha(200))
                },
                "Scatter Plot" => new ScatterSeries<double>
                {
                    Name = name,
                    Values = values,
                    GeometrySize = pointSize,
                    Stroke = null,
                    Fill = new SolidColorPaint(color.WithAlpha(150)),
                    DataLabelsPaint = null
                },
                _ => throw new NotSupportedException($"Chart type {SelectedChartType} not supported")
            };

            CartesianSeries.Add(series);
        }

        private void CreateDemandOrPriceChart(DateTime startDate, DateTime endDate, string title, string yAxisTitle, SKColor color)
        {
            var optimizerVM = new OptimizerViewModel();
            var records = new List<HeatDemandRecord>();

            if (optimizerVM.UseWinterData) records.AddRange(_sourceDataManager.WinterRecords.Where(r => r.TimeFrom >= startDate && r.TimeFrom <= endDate));
            if (optimizerVM.UseSummerData) records.AddRange(_sourceDataManager.SummerRecords.Where(r => r.TimeFrom >= startDate && r.TimeFrom <= endDate));

            var sorted = records.OrderBy(r => r.TimeFrom).ToList();
            _preparedLabels = sorted.Select(r => r.TimeFrom.ToString("dd-MM HH:mm")).ToList();
            _preparedValues = sorted.Select(r => title == "Heat Demand" ? r.HeatDemand ?? 0 : r.ElectricityPrice ?? 0).ToList();
            _preparedXAxisTitle = "Date and Hour";
            _preparedYAxisTitle = yAxisTitle;

            var seriesColor = new SolidColorPaint(color);
            var pointColor = new SolidColorPaint(color);
            var fillColor = new SolidColorPaint(color.WithAlpha(50));

            ISeries series = SelectedChartType switch
            {
                "Line Chart" => new LineSeries<double>
                {
                    Name = title,
                    Values = _preparedValues,
                    Stroke = seriesColor,
                    GeometryStroke = pointColor,
                    GeometryFill = pointColor,
                    GeometrySize = 4,
                    Fill = fillColor,
                    LineSmoothness = 0 
                },
                "Bar Chart" => new ColumnSeries<double>
                {
                    Name = title,
                    Values = _preparedValues,
                    MaxBarWidth = 20,
                    Stroke = null,
                    Fill = seriesColor
                },
                "Scatter Plot" => new ScatterSeries<double>
                {
                    Name = title,
                    Values = _preparedValues,
                    DataLabelsPaint = null,
                    Fill = pointColor,
                    GeometrySize = 5
                },
                _ => throw new NotSupportedException($"Chart type {SelectedChartType} not supported")
            };

            CartesianSeries.Add(series);
            SetAxes(_preparedLabels);
        }

        private void UpdateProductionPerformanceChart()
        {
            var activeAssets = _assetManager.GetAllAssets().Values.Where(a => a.IsActive).ToList();
            if (!activeAssets.Any()) return;

            _preparedLabels = activeAssets.Select(a => a.Name).ToList();
            _preparedXAxisTitle = "Production Units";
            _preparedYAxisTitle = "Value";

            var metrics = new[]
            {
                ("Heat Produced (MWh)", activeAssets.Select(a => a.ProducedHeat.Values.Sum(v => v ?? 0)).ToList()),
                ("Production Cost (DKK/MWh)", activeAssets.Select(a => a.ProductionCost ?? 0).ToList()),
                ("Fuel Consumption", activeAssets.Select(a => a.FuelConsumption ?? 0).ToList()),
                ("CO₂ Emissions (kg/MWh)", activeAssets.Select(a => a.CO2Emissions ?? 0).ToList())
            };

            foreach (var (name, values) in metrics)
            {
                ISeries series = SelectedChartType switch
                {
                    "Line Chart" => new LineSeries<double> 
                    { 
                        Name = name, 
                        Values = values,
                        LineSmoothness = 0.8
                    },
                    "Bar Chart" => new ColumnSeries<double> { Name = name, Values = values },
                    "Scatter Plot" => new ScatterSeries<double> { Name = name, Values = values },
                    _ => throw new NotSupportedException($"Chart type {SelectedChartType} not supported")
                };

                CartesianSeries.Add(series);
            }

            SetAxes(_preparedLabels);
        }

        private void RefreshChart()
        {
            this.RaisePropertyChanged(nameof(ChartWidth));
            this.RaisePropertyChanged(nameof(CartesianSeries));
            this.RaisePropertyChanged(nameof(XAxes));
            this.RaisePropertyChanged(nameof(YAxes));
        }

        private void SetAxes(List<string> labels)
        {
            XAxes.Clear();
            YAxes.Clear();

            string dateFormat = GetOptimalDateFormat(labels);

            XAxes.Add(new Axis
            {
                Labels = labels.ToArray(),
                Name = ShowDataLabels ? _preparedXAxisTitle : "",
                ShowSeparatorLines = ShowGridLines,
                LabelsRotation = GetOptimalLabelRotation(labels.Count),
                TextSize = 12,
                UnitWidth = 1,
                MinStep = CalculateOptimalMinStep(labels.Count),
                ForceStepToMin = labels.Count < 100,
                Labeler = value => FormatDateLabel(value, dateFormat, labels)
            });

            YAxes.Add(new Axis
            {
                Name = ShowDataLabels ? _preparedYAxisTitle : "",
                ShowSeparatorLines = ShowGridLines,
                MinLimit = AutoScale ? null : 0,
                MaxLimit = AutoScale ? null : (_preparedValues.Count > 0 ? _preparedValues.Max() * 1.1 : null)
            });
        }

        private void UpdateChartTypes()
        {
            FilteredChartTypes.Clear();
            
        
            foreach (var type in AvailableChartTypes)
            {
                FilteredChartTypes.Add(type);
            }

            if (!FilteredChartTypes.Contains(SelectedChartType))
            {
                SelectedChartType = FilteredChartTypes.FirstOrDefault() ?? string.Empty;
                this.RaisePropertyChanged(nameof(SelectedChartType));
            }
        }

        private string GetOptimalDateFormat(List<string> labels)
        {
            if (labels.Count == 0) return "dd-MM HH:mm";

            try
            {
                if (DateTime.TryParse(labels.First(), out DateTime firstDate) &&
                    DateTime.TryParse(labels.Last(), out DateTime lastDate))
                {
                    TimeSpan dateRange = lastDate - firstDate;

                    if (dateRange.TotalDays > 365) return "MMM yyyy"; 
                    if (dateRange.TotalDays > 30) return "dd MMM";    
                    if (dateRange.TotalDays > 2) return "dd-MM HH:mm"; 
                    return "HH:mm"; 
                }
            }
            catch
            {

            }

            return "dd-MM HH:mm";
        }

        private double GetOptimalLabelRotation(int labelCount)
        {
            if (labelCount <= 24) return 0;
            if (labelCount <= 168) return 45;
            return 90;
        }

        // Method to calculate the optimal data agrupation based the on number of data points
        private double CalculateOptimalMinStep(int labelCount)
        {
            if (labelCount <= 24) return 1;
            if (labelCount <= 168) return 3;
            if (labelCount <= 720) return 24;
            return 168;
        }

        private string FormatDateLabel(double value, string dateFormat, List<string> allLabels)
        {
            try
            {
                int index = (int)value;
                if (index >= 0 && index < allLabels.Count)
                {
                    if (DateTime.TryParse(allLabels[index], out DateTime date))
                    {
                        return date.ToString(dateFormat);
                    }
                }
            }
            catch
            {

            }

            return value.ToString();
        }

        private void UpdateAxisTitles()
        {
            if (XAxes.Count > 0)
            {
                XAxes[0].Name = ShowDataLabels ? _preparedXAxisTitle : "";
                this.RaisePropertyChanged(nameof(XAxes));
            }

            if (YAxes.Count > 0)
            {
                YAxes[0].Name = ShowDataLabels ? _preparedYAxisTitle : "";
                this.RaisePropertyChanged(nameof(YAxes));
            }
        }

        private void UpdateGridLines()
        {
            if (XAxes.Count > 0)
                XAxes[0].ShowSeparatorLines = ShowGridLines;

            if (YAxes.Count > 0)
                YAxes[0].ShowSeparatorLines = ShowGridLines;
        }

        private void UpdateYAxisLimits()
        {
            if (YAxes.Count == 0) return;

            YAxes[0].MinLimit = AutoScale ? null : 0;
            YAxes[0].MaxLimit = AutoScale || _preparedValues.Count == 0
                ? null
                : _preparedValues.Max() * 1.1;
        }

        // Method to calculate Chart Width based the on number of data points
        private double CalculateOptimalChartWidth()
        {
            if (_preparedLabels.Count <= 24) return 900; // Base width for small datasets

            // Calculate width based on number of data points
            double baseWidth = 900;
            double additionalWidth = (_preparedLabels.Count - 24) * 20; // 20px per additional point

            // Cap at 3000px to prevent extreme widths
            return Math.Min(baseWidth + additionalWidth, 3000);
        }

        // Method to adapt size of Graph's points based on the number of data points
        private double CalculatePointSize()
        {
            if (_preparedLabels.Count == 0) return 8; // Default size

            double baseSize = 8;
            double widthFactor = ChartWidth / 1000;
            double countFactor = 50.0 / _preparedLabels.Count;

            return Math.Max(3, Math.Min(baseSize * widthFactor * countFactor, 8));
        }

        // Method to generate random colors
        private SKColor[] GenerateDistinctColors(int count)
        {
            var colors = new SKColor[count];
            var hueStep = 360.0 / count;

            for (int i = 0; i < count; i++)
            {
                var hue = i * hueStep;
                colors[i] = SKColor.FromHsl((float)hue, 80, 60);
            }

            return colors;
        }

        // Method to Reset Zoom
        private void ResetZoom()
        {
            if (XAxes.Count > 0)
            {
                XAxes[0].MinLimit = null;
                XAxes[0].MaxLimit = null;
                this.RaisePropertyChanged(nameof(XAxes));
            }

            if (YAxes.Count > 0)
            {
                YAxes[0].MinLimit = AutoScale ? null : 0;
                YAxes[0].MaxLimit = AutoScale ? null : (_preparedValues.Count > 0 ? _preparedValues.Max() * 1.1 : null);
                this.RaisePropertyChanged(nameof(YAxes));
            }
        }
        // Method to save chart as image
        private void SaveChartImage()
        {
            try
            {
                string fileName = $"{SelectedDataSource.Replace(" ", "_")}_{SelectedChartType.Replace(" ", "_")}.png";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);

                var chart = new SKCartesianChart
                {
                    Width = (int)ChartWidth,
                    Height = 600,
                    Series = CartesianSeries,
                    XAxes = XAxes,
                    YAxes = YAxes,
                    Title = new LabelVisual
                    {
                        Text = $"{SelectedDataSource} - {SelectedChartType}",
                        TextSize = 20,
                        Padding = new LiveChartsCore.Drawing.Padding(15),
                        Paint = new SolidColorPaint(SKColors.Black)
                    }
                };

                using (var image = chart.GetImage())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(filePath))
                {
                    data.SaveTo(stream);
                }

                Console.WriteLine($"Chart saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving chart image: {ex.Message}");
            }
        }

        // Method to get chart data for external use
        public ChartData GetChartData()
        {
            return new ChartData
            {
                Series = new List<ISeries>(CartesianSeries),
                XAxes = new List<Axis>(XAxes),
                YAxes = new List<Axis>(YAxes),
                ChartType = SelectedChartType,
                DataSource = SelectedDataSource,
                Values = new List<double>(_preparedValues),
                Labels = new List<string>(_preparedLabels),
                XAxisTitle = _preparedXAxisTitle,
                YAxisTitle = _preparedYAxisTitle,
                ChartWidth = ChartWidth
            };
        }

        // Method to generate all required charts and return their image paths
        public Dictionary<string, string> GenerateAllCharts()
        {
            var chartImages = new Dictionary<string, string>();
            var tempFolder = Path.GetTempPath();

            // 1. Heat Demand Data - Line Chart
            SelectedDataSource = "Heat Demand Data";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string heatDemandPath = Path.Combine(tempFolder, "Heat_Demand_Bar.png");
            SaveSpecificChart(heatDemandPath);
            chartImages.Add("HeatDemand", heatDemandPath);

            // 2. Electricity Price Data - Line Chart
            SelectedDataSource = "Electricity Price Data";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string electricityPricePath = Path.Combine(tempFolder, "Electricity_Price_Bar.png");
            SaveSpecificChart(electricityPricePath);
            chartImages.Add("ElectricityPrice", electricityPricePath);

            // 3. Optimization Results - Bar Chart 
            SelectedDataSource = "Optimization Results";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string optimizationPath = Path.Combine(tempFolder, "Optimization_Bar.png");
            SaveSpecificChart(optimizationPath);
            chartImages.Add("Optimization", optimizationPath);

            // 4. Production Unit Performance - Bar Chart
            SelectedDataSource = "Production Unit Performance";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string productionPath = Path.Combine(tempFolder, "Production_Performance_Bar.png");
            SaveSpecificChart(productionPath);
            chartImages.Add("Production", productionPath);

            return chartImages;
        }

        private void SaveSpecificChart(string filePath)
        {
            try
            {
                var series = CartesianSeries.ToList();
                var xAxis = XAxes.FirstOrDefault() ?? new Axis();
                var yAxis = YAxes.FirstOrDefault() ?? new Axis();

                var chart = new SKCartesianChart
                {
                    Width = (int)ChartWidth,
                    Height = 600,
                    Series = series,
                    XAxes = new[] { xAxis },
                    YAxes = new[] { yAxis },
                    Title = new LabelVisual
                    {
                        Text = $"{SelectedDataSource} - {SelectedChartType}",
                        TextSize = 20,
                        Padding = new LiveChartsCore.Drawing.Padding(15),
                        Paint = new SolidColorPaint(SKColors.Black)
                    }
                };

                if (xAxis.Labels != null && xAxis.Labels.Count > 50)
                {
                    xAxis.LabelsRotation = 45;
                    xAxis.TextSize = 10;
                    xAxis.ShowSeparatorLines = false;
                    chart.Width = Math.Max(xAxis.Labels.Count * 15, 1200);
                }

                using (var image = chart.GetImage())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(filePath))
                {
                    data.SaveTo(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving specific chart: {ex.Message}");
            }
        }

        public class ChartData
        {
            public List<ISeries> Series { get; set; } = new();
            public List<Axis> XAxes { get; set; } = new();
            public List<Axis> YAxes { get; set; } = new();
            public string ChartType { get; set; } = string.Empty;
            public string DataSource { get; set; } = string.Empty;
            public List<double> Values { get; set; } = new();
            public List<string> Labels { get; set; } = new();
            public string XAxisTitle { get; set; } = string.Empty;
            public string YAxisTitle { get; set; } = string.Empty;
            public double ChartWidth { get; set; }
        }
    }
}