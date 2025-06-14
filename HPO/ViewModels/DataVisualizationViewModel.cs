using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel.DataAnnotations.Schema;
using DynamicData.Aggregation;
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
        public ObservableCollection<ISeries> CartesianSeries { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new();
        public ObservableCollection<Axis> YAxes { get; set; } = new();

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

        // OPT Data Settings
        private bool _isResultVisible;
        private double? _totalProdCosts = 0;
        private double? _totalCO2 = 0;
        private double? _totalFuel = 0;
        private double? _totalHeatProduced = 0;
        private List<DateTime> _currentOptimizationTimestamps = new();

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

        // Properties for chart options with change notification
        public bool IsResultVisible
        {
            get => _isResultVisible;
            set => this.RaiseAndSetIfChanged(ref _isResultVisible, value);
        }

        public double? TotalProdCosts
        {
            get => _totalProdCosts;
            set => this.RaiseAndSetIfChanged(ref _totalProdCosts, value);
        }

        public double? TotalCO2
        {
            get => _totalCO2;
            set => this.RaiseAndSetIfChanged(ref _totalCO2, value);
        }

        public double? TotalFuel
        {
            get => _totalFuel;
            set => this.RaiseAndSetIfChanged(ref _totalFuel, value);
        }

        public double? TotalHeatProduced
        {
            get => _totalHeatProduced;
            set => this.RaiseAndSetIfChanged(ref _totalHeatProduced, value);
        }    

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

            if (_preparedLabels.Count > 0)
            {
                SetAxes(_preparedLabels);
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

        // Date Range from OptimizerViewModel
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

        // 1.OPT Results
        private void UpdateOptimizationResultsChart(DateTime startDate, DateTime endDate)
        {
            var assets = _assetManager.GetAllAssets().Values.Where(a => a.IsActive).ToList();
            if (assets.Count == 0) return;

            _currentOptimizationTimestamps = assets
                .SelectMany(a => a.ProducedHeat.Keys)
                .Where(t => t >= startDate && t <= endDate)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            if (_currentOptimizationTimestamps.Count == 0) return;

            _preparedLabels = _currentOptimizationTimestamps.Select(t => t.ToString("dd-MM HH:mm")).ToList();
            _preparedXAxisTitle = "Time";
            _preparedYAxisTitle = "Produced Heat (MWh)";

            var calculatedValues = new {
                Costs = OptimizerViewModel.Totalcost ?? 0,
                CO2 = OptimizerViewModel.TotalCO2 ?? 0,
                Fuel = OptimizerViewModel.TotalFuel ?? 0,
                Heat = assets.Where(a => a.IsActive).SelectMany(a => a.ProducedHeat.Values).Sum()
            };

            IsResultVisible = false;

            var colorPairs = GenerateDistinctColorPairs(assets.Count);
            double pointSize = CalculatePointSize();

            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var values = _currentOptimizationTimestamps.Select(t => asset.ProducedHeat.TryGetValue(t, out var v) ? v ?? 0 : 0).ToList();
                AddSeriesToChart(asset.Name, values, colorPairs[i].lineColor, colorPairs[i].fillColor, pointSize);
            }

            _preparedValues = assets
                .SelectMany(a => a.ProducedHeat
                    .Where(e => e.Key >= startDate && e.Key <= endDate)
                    .Select(e => e.Value ?? 0))
                .ToList();

            SetAxes(_preparedLabels);
            this.RaisePropertyChanged(nameof(ZoomMode));
        }

        // Method to stablish chart settings 
        private void AddSeriesToChart(string name, List<double> values, SKColor lineColor, SKColor fillColor, double pointSize)
        {
            ISeries series = SelectedChartType switch
            {
                "Line Chart" => new LineSeries<double>
                {
                    Name = name,
                    Values = values,
                    GeometrySize = pointSize,
                    Stroke = new SolidColorPaint(lineColor) { StrokeThickness = 2 },
                    GeometryStroke = new SolidColorPaint(lineColor) { StrokeThickness = 1 },
                    GeometryFill = new SolidColorPaint(lineColor.WithAlpha(180)),
                    Fill = new SolidColorPaint(fillColor), 
                    LineSmoothness = 0
                },
                "Bar Chart" => new ColumnSeries<double>
                {
                    Name = name,
                    Values = values,
                    MaxBarWidth = 20,
                    Stroke = null,
                    Fill = new SolidColorPaint(lineColor.WithAlpha(200))
                },
                "Scatter Plot" => new ScatterSeries<double>
                {
                    Name = name,
                    Values = values,
                    GeometrySize = pointSize,
                    Stroke = null,
                    Fill = new SolidColorPaint(lineColor.WithAlpha(150)),
                    DataLabelsPaint = null
                },
                _ => throw new NotSupportedException($"Chart type {SelectedChartType} not supported")
            };

            CartesianSeries.Add(series);
        }

        // 2. and 3. Heat Demand and Electricity Price
        private void CreateDemandOrPriceChart(DateTime startDate, DateTime endDate, string title, string yAxisTitle, SKColor color)
        {
            var optimizerVM = new OptimizerViewModel();
            var records = new List<HeatDemandRecord>();
            IsResultVisible = false;

            // Get Dates from OPT Results
            var optimizationTimestamps = _assetManager.GetAllAssets().Values
                .Where(a => a.IsActive)
                .SelectMany(a => a.ProducedHeat.Keys)
                .Where(t => t >= startDate && t <= endDate)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Filter Data
            if (optimizerVM.UseWinterData)
            {
                records.AddRange(_sourceDataManager.WinterRecords
                    .Where(r => optimizationTimestamps.Contains(r.TimeFrom)));
            }
            if (optimizerVM.UseSummerData)
            {
                records.AddRange(_sourceDataManager.SummerRecords
                    .Where(r => optimizationTimestamps.Contains(r.TimeFrom)));
            }

            var sorted = records.OrderBy(r => r.TimeFrom).ToList();
            _preparedLabels = sorted.Select(r => r.TimeFrom.ToString("dd-MM HH:mm")).ToList();
            _preparedValues = sorted.Select(r => title == "Heat Demand" ? r.HeatDemand ?? 0 : r.ElectricityPrice ?? 0).ToList();
            _preparedXAxisTitle = "Time";
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
                    GeometrySize = 2,
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

        // 4. Production Unit Performance
        private void UpdateProductionPerformanceChart()
        {
            var activeAssets = _assetManager.GetAllAssets().Values.Where(a => a.IsActive).ToList();

            if (activeAssets.Any())
            {
                var (startDate, endDate) = GetDateRange();
                var filteredHeat = activeAssets
                    .SelectMany(a => a.ProducedHeat
                        .Where(e => e.Key >= startDate && e.Key <= endDate)
                        .Select(e => e.Value ?? 0));

                TotalProdCosts = OptimizerViewModel.Totalcost ?? 0;
                TotalCO2 = OptimizerViewModel.TotalCO2 ?? 0;
                TotalFuel = OptimizerViewModel.TotalFuel ?? 0;
                TotalHeatProduced = filteredHeat.Sum();
            }
            else
            {
                TotalProdCosts = 0;
                TotalCO2 = 0;
                TotalFuel = 0;
                TotalHeatProduced = 0;
            }

            IsResultVisible = true;

            _preparedLabels = activeAssets.Select(a => a.Name).ToList();
            _preparedXAxisTitle = "Production Units";
            _preparedYAxisTitle = "Value";

            var metrics = new[]
            {
                (Name: "Heat Produced (MWh)", 
                Values: activeAssets.Select(a => a.ProducedHeat.Values.Sum(v => v ?? 0)).ToList(),
                Color: "#8E44AD"),  // Purple
                
                (Name: "Production Cost (thousands DKK)", 
                Values: activeAssets.Select(a => ((a.ProductionCost ?? 0) * a.ProducedHeat.Values.Sum(v => v ?? 0)) / 1000).ToList(), 
                Color: "#0066CC"),  // Blue
                
                (Name: "Fuel Consumption (MWh)", 
                Values: activeAssets.Select(a => (a.FuelConsumption ?? 0) * a.ProducedHeat.Values.Sum(v => v ?? 0)).ToList(),
                Color: "#CC7A00"),  // Orange
                
                (Name: "CO₂ Emissions (ton)", 
                Values: activeAssets.Select(a => ((a.CO2Emissions ?? 0) * a.ProducedHeat.Values.Sum(v => v ?? 0)) / 1000).ToList(), 
                Color: "#2A9D51")   // Green
            };

            foreach (var (name, values, color) in metrics)
            {
                var skColor = SKColor.Parse(color);
                
                ISeries series = SelectedChartType switch
                {
                    "Line Chart" => new LineSeries<double>
                    {
                        Name = name,
                        Values = values,
                        Stroke = new SolidColorPaint(skColor) { StrokeThickness = 2 },
                        GeometryStroke = new SolidColorPaint(skColor) { StrokeThickness = 1 },
                        GeometryFill = new SolidColorPaint(skColor.WithAlpha(180)),
                        Fill = new SolidColorPaint(skColor.WithAlpha(50)),
                        LineSmoothness = 0.8
                    },
                    "Bar Chart" => new ColumnSeries<double> 
                    { 
                        Name = name, 
                        Values = values,
                        Fill = new SolidColorPaint(skColor),
                        Stroke = null
                    },
                    "Scatter Plot" => new ScatterSeries<double> 
                    { 
                        Name = name, 
                        Values = values,
                        Fill = new SolidColorPaint(skColor),
                        GeometrySize = 8
                    },
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
                Labels = ShowDataLabels ? labels.ToArray() : Array.Empty<string>(), 
                Name = ShowDataLabels ? _preparedXAxisTitle : "",
                ShowSeparatorLines = ShowGridLines,
                LabelsRotation = GetOptimalLabelRotation(labels.Count),
                TextSize = 12,
                UnitWidth = 1,
                MinStep = CalculateOptimalMinStep(labels.Count),
                ForceStepToMin = labels.Count < 100,
                Labeler = value => ShowDataLabels ? FormatDateLabel(value, dateFormat, labels) : "" 
            });

            YAxes.Add(new Axis
            {
                Name = ShowDataLabels ? _preparedYAxisTitle : "",
                ShowSeparatorLines = ShowGridLines,
                MinLimit = AutoScale ? null : 0,
                MaxLimit = AutoScale ? null : (_preparedValues.Count > 0 ? _preparedValues.Max() * 1.1 : null),
                Labels = ShowDataLabels ? null : Array.Empty<string>() 
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
            if (labelCount <= 6) return 0;
            if (labelCount <= 168) return 45;
            return 90;
        }

        // Method to calculate the optimal data agrupation based the on number of data points
        private double CalculateOptimalMinStep(int labelCount)
        {
            if (labelCount <= 24) return 1;
            if (labelCount <= 72) return 2;
            if (labelCount <= 96) return 3;
            if (labelCount <= 132) return 4;
            if (labelCount <= 168) return 6;
            if (labelCount <= 360) return 12;
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
                XAxes[0].Labels = ShowDataLabels ? _preparedLabels.ToArray() : Array.Empty<string>();
                this.RaisePropertyChanged(nameof(XAxes));
            }

            if (YAxes.Count > 0)
            {
                YAxes[0].Name = ShowDataLabels ? _preparedYAxisTitle : "";
                YAxes[0].Labels = ShowDataLabels ? null : Array.Empty<string>();
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
        private (SKColor lineColor, SKColor fillColor)[] GenerateDistinctColorPairs(int count)
        {
            var colors = new (SKColor, SKColor)[count];
            var hueStep = 360.0 / count;

            for (int i = 0; i < count; i++)
            {
                var hue = i * hueStep;
                var lineColor = SKColor.FromHsl((float)hue, 95, 25);
                var fillColor = SKColor.FromHsl((float)hue, 95, 75).WithAlpha(80);
                colors[i] = (lineColor, fillColor);
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

                int baseHeight = 900;
                int baseWidth = 1400;
                int calculatedWidth = baseWidth;

                var xAxis = XAxes.FirstOrDefault();
                if (xAxis != null)
                {
                    int labelCount = xAxis.Labels?.Count ?? 0;
                    calculatedWidth = Math.Min(Math.Max(baseWidth, labelCount * 40), 2500);
                    
                    xAxis.TextSize = 14;
                    xAxis.NameTextSize = 16;
                    xAxis.MinStep = CalculateOptimalMinStep(labelCount);
                    xAxis.LabelsRotation = labelCount > 6 ? (labelCount > 30 ? 90 : 45) : 0;
                }

                var chart = new SKCartesianChart
                {
                    Width = calculatedWidth,
                    Height = baseHeight + 60,
                    Series = CartesianSeries,
                    XAxes = XAxes,
                    YAxes = YAxes,
                    LegendPosition = LegendPosition.Right,
                    LegendTextSize = 32,
                    Background = SKColors.White,
                };

                using (var image = chart.GetImage())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(filePath))
                {
                    data.SaveTo(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving chart image: {ex.Message}");
            }
        }

        // Method to generate all required charts and return their image paths
        public Dictionary<string, string> GenerateAllCharts()
        {
            var chartImages = new Dictionary<string, string>();
            var tempFolder = Path.GetTempPath();

        void SaveChartWithSettings(string path)
        {
            try
            {
                var xAxis = XAxes.FirstOrDefault() ?? new Axis();
                var yAxis = YAxes.FirstOrDefault() ?? new Axis();
                
                int labelCount = xAxis.Labels?.Count ?? 0;
                int width = Math.Clamp(1200 + (labelCount * 25), 1000, 2500);
                int height = 900;

                xAxis.TextSize = 28;
                xAxis.NameTextSize = 32;
                xAxis.MinStep = CalculateOptimalMinStep(labelCount);
                xAxis.LabelsRotation = labelCount > 6 ? (labelCount > 30 ? 90 : 45) : 0;
                yAxis.TextSize = 28;
                yAxis.NameTextSize = 32;

                var chart = new SKCartesianChart
                {
                    Width = width,
                    Height = height,
                    Series = CartesianSeries.ToList(),
                    XAxes = new[] { xAxis },
                    YAxes = new[] { yAxis },
                    LegendPosition = LegendPosition.Right,
                    LegendTextSize = 32,
                    Background = SKColors.White,
                };

                using (var image = chart.GetImage())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(path))
                {
                    data.SaveTo(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving chart: {ex.Message}");
            }
        }

            // 1. Heat Demand Data - Bar Chart
            SelectedDataSource = "Heat Demand Data";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string heatDemandPath = Path.Combine(tempFolder, "Heat_Demand_Bar.png");
            SaveChartWithSettings(heatDemandPath);
            chartImages.Add("HeatDemand", heatDemandPath);

            // 2. Electricity Price Data - Bar Chart
            SelectedDataSource = "Electricity Price Data";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string electricityPricePath = Path.Combine(tempFolder, "Electricity_Price_Bar.png");
            SaveChartWithSettings(electricityPricePath);
            chartImages.Add("ElectricityPrice", electricityPricePath);

            // 3. Optimization Results - Bar Chart 
            SelectedDataSource = "Optimization Results";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string optimizationPath = Path.Combine(tempFolder, "Optimization_Bar.png");
            SaveChartWithSettings(optimizationPath);
            chartImages.Add("Optimization", optimizationPath);

            // 4. Production Unit Performance - Bar Chart
            SelectedDataSource = "Production Unit Performance";
            SelectedChartType = "Bar Chart";
            UpdateChart();
            string productionPath = Path.Combine(tempFolder, "Production_Performance_Bar.png");
            SaveChartWithSettings(productionPath);
            chartImages.Add("Production", productionPath);

            return chartImages;
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