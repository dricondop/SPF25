using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using CommunityToolkit.Mvvm.Input;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using ReactiveUI;
using SkiaSharp;
using Avalonia.Remote.Protocol.Designer;

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


        // Selected data source and chart type
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
                if (string.IsNullOrEmpty(changed)) this.RaisePropertyChanged(nameof(SelectedDataSource));
            }
        }

        public string SelectedChartType { get; set; }
        private bool _showLegend = true;
        private bool _showDataLabels = true;
        private bool _showGridLines = true;
        private bool _autoScale = true;

        // Prepared data for chart rendering
        private string _preparedYAxisTitle = "";
        private string _preparedXAxisTitle = "";
        private List<double> _preparedValues = new();
        private List<string> _preparedLabels = new();
        public double ChartWidth => Math.Max(_preparedLabels.Count * 45, 900); // Minimum width of 900 pixels
        private readonly AssetManager _assetManager;
        private readonly SourceDataManager _sourceDataManager;

        // Property to determine legend position based on the ShowLegend flag
        public LiveChartsCore.Measure.LegendPosition ChartLegendPosition =>
            ShowLegend ? LiveChartsCore.Measure.LegendPosition.Right : LiveChartsCore.Measure.LegendPosition.Hidden;


        // Properties for chart options with change notification
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
                XAxes = new ObservableCollection<Axis>(XAxes.ToList());
                YAxes = new ObservableCollection<Axis>(YAxes.ToList());
                this.RaisePropertyChanged(nameof(XAxes));
                this.RaisePropertyChanged(nameof(YAxes));
            }
        }
        public bool ShowGridLines
        {
            get => _showGridLines;
            set
            {
                this.RaiseAndSetIfChanged(ref _showGridLines, value);
                UpdateGridLines();
                XAxes = new ObservableCollection<Axis>(XAxes.ToList());
                YAxes = new ObservableCollection<Axis>(YAxes.ToList());
                this.RaisePropertyChanged(nameof(XAxes));
                this.RaisePropertyChanged(nameof(YAxes));
            }
        }
        public bool AutoScale
        {
            get => _autoScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _autoScale, value);
                UpdateYAxisLimits();
                YAxes = new ObservableCollection<Axis>(YAxes.ToList());
                this.RaisePropertyChanged(nameof(YAxes));
            }
        }

        public ICommand UpdateChartCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand PrintReportCommand { get; }
        public ICommand CompareResultsCommand { get; }

        public DataVisualizationViewModel()
        {
            _assetManager = OptimizerViewModel.SharedAssetManager;
            _sourceDataManager = SourceDataManager.sourceDataManagerInstance;

            SelectedDataSource = "Optimization Results";
            SelectedChartType = "Line Chart";

            UpdateChartCommand = new RelayCommand(UpdateChart);
            ExportDataCommand = new RelayCommand(ExportData);
            PrintReportCommand = new RelayCommand(PrintReport);
            CompareResultsCommand = new RelayCommand(CompareResults);
            FilteredChartTypes = new ObservableCollection<string>(AvailableChartTypes);

            // Initialize default chart data
            CartesianSeries.Add(new LineSeries<double> { Values = new List<double> { 0 } });
            XAxes.Add(new Axis { Labels = new[] { "Start" }, Name = "Units" });
            YAxes.Add(new Axis { Name = "Value" });
            UpdateChartTypes();
        }

        // Method to refresh data based on the selected data source
        private void UpdateChart()
        {
            _preparedValues.Clear();
            _preparedLabels.Clear();
            _preparedYAxisTitle = "";
            _preparedXAxisTitle = "";
            CartesianSeries.Clear();

            DateTime startDate = new DateTime(2024, 3, 1, 0, 0, 0); 
            DateTime endDate = new DateTime(2024, 8, 25, 0, 0, 0); 

            try
            {
                var optimizerVM = new OptimizerViewModel();
                if (optimizerVM.StartDate.HasValue && optimizerVM.EndDate.HasValue)
                {
                    startDate = optimizerVM.StartDate.Value.DateTime;
                    endDate = optimizerVM.EndDate.Value.DateTime;
                }
                else
                {
                    Console.WriteLine("No dates available in the Optimizer");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }


            Console.WriteLine($"Selected data source: {SelectedDataSource}");
            if (SelectedDataSource == "Optimization Results")
            {
                Console.WriteLine($"Asset Manager has {OptimizerViewModel.SharedAssetManager.GetAllAssets().Count} total assets");
                var assets = OptimizerViewModel.SharedAssetManager.GetAllAssets().Values
                    .Where(a => a.IsActive)
                    .ToList();
        
                if (assets.Count == 0)
                {
                    Console.WriteLine("No active assets found.");
                    return;
                }


                var timestamps = assets
                    .SelectMany(a => a.ProducedHeat.Keys)
                    .Where(t => t >= startDate && t <= endDate)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();


                if (timestamps.Count == 0)
                {
                    Console.WriteLine("No data available for the selected date range.");
                    return;
                }

                _preparedLabels = timestamps.Select(t => t.ToString("MM-dd HH:mm")).ToList();
                this.RaisePropertyChanged(nameof(ChartWidth));
                _preparedXAxisTitle = "Time";
                _preparedYAxisTitle = "Produced Heat (MWh)";

                foreach (var a in assets)
                {
                    var values = timestamps.Select(t => a.ProducedHeat.TryGetValue(t, out var v) ? v ?? 0 : 0).ToList();

                    CartesianSeries.Add(new LineSeries<double>
                    {
                        Name = a.Name,
                        Values = values,
                    });
                }

                _preparedValues = assets
                    .SelectMany(a => a.ProducedHeat
                        .Where(e => e.Key >= startDate && e.Key <= endDate)
                        .Select(e => e.Value ?? 0))
                    .ToList();

                SetAxes(_preparedLabels);
                this.RaisePropertyChanged(nameof(CartesianSeries));
                this.RaisePropertyChanged(nameof(XAxes));
                this.RaisePropertyChanged(nameof(YAxes));// Update axes
                SetAxes(_preparedLabels);

                switch (SelectedChartType)
                {
                    case "Line Chart":
                        CreateLineChart();
                        break;
                    case "Bar Chart":
                        CreateBarChart(_preparedValues, _preparedLabels);
                        break;
                    case "Scatter Plot":
                        CreateScatterChart(_preparedValues, _preparedLabels);
                        break;
                }

                this.RaisePropertyChanged(nameof(CartesianSeries));
                this.RaisePropertyChanged(nameof(XAxes));
                this.RaisePropertyChanged(nameof(YAxes));
            }

            else if (SelectedDataSource == "Heat Demand Data")
            {
                DateTime start = startDate;
                DateTime end = endDate;
                if (SelectedDataSource == "Heat Demand Data")
                {
                    var optimizerVM = new OptimizerViewModel();
                    var winter = optimizerVM.UseWinterData;
                    var summer = optimizerVM.UseSummerData;
                    var records = new List<HeatDemandRecord>();
                    if (winter) records.AddRange(_sourceDataManager.WinterRecords.Where(r => r.TimeFrom >= startDate && r.TimeFrom <= endDate));
                    if (summer) records.AddRange(_sourceDataManager.SummerRecords.Where(r => r.TimeFrom >= startDate && r.TimeFrom <= endDate));
                    var sorted = records.OrderBy(r => r.TimeFrom).ToList();
                    _preparedLabels = sorted.Select(r => r.TimeFrom.ToString("MM-dd HH:mm")).ToList();
                    _preparedValues = sorted.Select(r => r.HeatDemand ?? 0).ToList();
                    _preparedXAxisTitle = "Date and Hour";
                    _preparedYAxisTitle = "Heat Demand (MWh)";

                    CartesianSeries.Clear();

                    switch (SelectedChartType)
                    {
                        case "Line Chart":
                            CartesianSeries.Add(new LineSeries<double>
                            {
                                Name = "Heat Demand",
                                Values = _preparedValues
                            });
                            break;
                        case "Bar Chart":
                            CartesianSeries.Add(new ColumnSeries<double>
                            {
                                Name = "Heat Demand",
                                Values = _preparedValues,
                                MaxBarWidth = 20,
                                Stroke = null
                            });
                            break;
                        case "Scatter Plot":
                            CartesianSeries.Add(new ScatterSeries<double>
                            {
                                Name = "Heat Demand",
                                Values = _preparedValues,
                                DataLabelsPaint = null
                            });
                            break;
                    }
                    this.RaisePropertyChanged(nameof(ChartWidth));
                    SetAxes(_preparedLabels);
                    this.RaisePropertyChanged(nameof(CartesianSeries));
                    this.RaisePropertyChanged(nameof(XAxes));
                    this.RaisePropertyChanged(nameof(YAxes));
                }
            }

            else if (SelectedDataSource == "Electricity Price Data")
            {
                DateTime start = startDate;
                DateTime end = endDate;
                var optimizerVM = new OptimizerViewModel();
                var winter = optimizerVM.UseWinterData;
                var summer = optimizerVM.UseSummerData;
                var records = new List<HeatDemandRecord>();
                if (winter) records.AddRange(_sourceDataManager.WinterRecords.Where(r => r.TimeFrom >= start && r.TimeFrom <= end));
                if (summer) records.AddRange(_sourceDataManager.SummerRecords.Where(r => r.TimeFrom >= start && r.TimeFrom <= end));
                var sorted = records.OrderBy(r => r.TimeFrom).ToList();
                _preparedLabels = sorted.Select(r => r.TimeFrom.ToString("MM-dd HH:mm")).ToList();
                _preparedValues = sorted.Select(r => r.ElectricityPrice ?? 0).ToList();
                _preparedXAxisTitle = "Date and Hour";
                _preparedYAxisTitle = "Electricity Price (DKK/kWh)";

                CartesianSeries.Clear();

                switch (SelectedChartType)
                {
                    case "Line Chart":
                        CartesianSeries.Add(new LineSeries<double>
                        {
                            Name = "Electricity Price",
                            Values = _preparedValues
                        });
                        break;
                    case "Bar Chart":
                        CartesianSeries.Add(new ColumnSeries<double>
                        {
                            Name = "Electricity Price",
                            Values = _preparedValues,
                            MaxBarWidth = 20,
                            Stroke = null
                        });
                        break;
                    case "Scatter Plot":
                        CartesianSeries.Add(new ScatterSeries<double>
                        {
                            Name = "Electricity Price",
                            Values = _preparedValues,
                            DataLabelsPaint = null
                        });
                        break;
                }
                this.RaisePropertyChanged(nameof(ChartWidth));
                SetAxes(_preparedLabels);
                this.RaisePropertyChanged(nameof(CartesianSeries));
                this.RaisePropertyChanged(nameof(XAxes));
                this.RaisePropertyChanged(nameof(YAxes));
            }

            else if (SelectedDataSource == "Production Unit Performance")
            {
                var activeAssets = OptimizerViewModel.SharedAssetManager
                    .GetAllAssets().Values
                    .Where(a => a.IsActive)
                    .ToList();

                if (!activeAssets.Any())
                {
                    Console.WriteLine("No active production units available.");
                    return;
                }

                _preparedLabels = activeAssets.Select(a => a.Name).ToList();
                this.RaisePropertyChanged(nameof(ChartWidth));
                _preparedXAxisTitle = "Production Units";
                _preparedYAxisTitle = "Value";
                var heatValues = activeAssets.Select(a => a.ProducedHeat.Values.Sum(v => v ?? 0)).ToList();
                var costValues = activeAssets.Select(a => a.ProductionCost ?? 0).ToList();
                var fuelValues = activeAssets.Select(a => a.FuelConsumption ?? 0).ToList();
                var co2Values = activeAssets.Select(a => a.CO2Emissions ?? 0).ToList();

                CartesianSeries.Clear();

                switch (SelectedChartType)
                {
                    case "Line Chart":
                        CartesianSeries.Add(new LineSeries<double> { Name = "Heat Produced (MWh)", Values = heatValues });
                        CartesianSeries.Add(new LineSeries<double> { Name = "Production Cost (DKK/MWh)", Values = costValues });
                        CartesianSeries.Add(new LineSeries<double> { Name = "Fuel Consumption", Values = fuelValues });
                        CartesianSeries.Add(new LineSeries<double> { Name = "CO₂ Emissions (kg/MWh)", Values = co2Values });
                        break;
                    case "Bar Chart":
                        CartesianSeries.Add(new ColumnSeries<double> { Name = "Heat Produced (MWh)", Values = heatValues });
                        CartesianSeries.Add(new ColumnSeries<double> { Name = "Production Cost (DKK/MWh)", Values = costValues });
                        CartesianSeries.Add(new ColumnSeries<double> { Name = "Fuel Consumption", Values = fuelValues });
                        CartesianSeries.Add(new ColumnSeries<double> { Name = "CO₂ Emissions (kg/MWh)", Values = co2Values });
                        break;
                    case "Scatter Plot":
                        CartesianSeries.Add(new ScatterSeries<double> { Name = "Heat Produced (MWh)", Values = heatValues });
                        CartesianSeries.Add(new ScatterSeries<double> { Name = "Production Cost (DKK/MWh)", Values = costValues });
                        CartesianSeries.Add(new ScatterSeries<double> { Name = "Fuel Consumption", Values = fuelValues });
                        CartesianSeries.Add(new ScatterSeries<double> { Name = "CO₂ Emissions (kg/MWh)", Values = co2Values });
                        break;
                }

                SetAxes(_preparedLabels);
                this.RaisePropertyChanged(nameof(CartesianSeries));
                this.RaisePropertyChanged(nameof(XAxes));
                this.RaisePropertyChanged(nameof(YAxes));
            }
        }

        private void ExportData() => Console.WriteLine("Exporting data...");
        private void PrintReport() => Console.WriteLine("Printing report...");
        private void CompareResults() => Console.WriteLine("Comparing results...");

        // Methods to create different types of charts
        private void CreateLineChart()
        {
            DateTime startDate = new DateTime(2024, 3, 1, 0, 0, 0); 
            DateTime endDate = new DateTime(2024, 8, 25, 0, 0, 0); 

            try
            {
                var optimizerVM = new OptimizerViewModel();
                if (optimizerVM.StartDate.HasValue && optimizerVM.EndDate.HasValue)
                {
                    startDate = optimizerVM.StartDate.Value.DateTime;
                    endDate = optimizerVM.EndDate.Value.DateTime;
                }
                else
                {
                    Console.WriteLine("No dates available in the Optimizer");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }

            CartesianSeries.Clear();
            var start = startDate;
            var end = endDate;

            if (SelectedDataSource == "Optimization Results")
            {
                var assets = OptimizerViewModel.SharedAssetManager.GetAllAssets().Values
                    .Where(a => a.IsActive).ToList();

                var timestamps = assets
                    .SelectMany(a => a.ProducedHeat.Keys)
                    .Where(t => t >= start && t <= end)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                _preparedLabels = timestamps.Select(t => t.ToString("MM-dd HH:mm")).ToList();
                _preparedXAxisTitle = "Time";
                _preparedYAxisTitle = "Produced Heat (MWh)";

                foreach (var a in assets)
                {
                    var values = timestamps
                        .Select(t => a.ProducedHeat.TryGetValue(t, out var v) ? v ?? 0 : 0)
                        .ToList();

                    CartesianSeries.Add(new LineSeries<double>
                    {
                        Name = a.Name,
                        Values = values
                    });
                }
                SetAxes(_preparedLabels);
            }
            else if (SelectedDataSource == "Heat Demand Data")
            {
                var winter = DateInputWindowViewModel.SelectedDateRange.UseWinterData;
                var summer = DateInputWindowViewModel.SelectedDateRange.UseSummerData;

                var records = new List<HeatDemandRecord>();
                if (winter) records.AddRange(_sourceDataManager.WinterRecords.Where(r => r.TimeFrom >= start && r.TimeFrom <= end));
                if (summer) records.AddRange(_sourceDataManager.SummerRecords.Where(r => r.TimeFrom >= start && r.TimeFrom <= end));

                var sorted = records.OrderBy(r => r.TimeFrom).ToList();
                _preparedLabels = sorted.Select(r => r.TimeFrom.ToString("MM-dd HH:mm")).ToList();
                _preparedValues = sorted.Select(r => r.HeatDemand ?? 0).ToList();
                _preparedXAxisTitle = "Date and Hour";
                _preparedYAxisTitle = "Heat Demand (MWh)";

                CartesianSeries.Add(new LineSeries<double>
                {
                    Name = "Heat Demand",
                    Values = _preparedValues
                });

                SetAxes(_preparedLabels);
            }

            this.RaisePropertyChanged(nameof(CartesianSeries));
        }

        private void CreateBarChart(List<double> values, List<string> labels)
        {
            DateTime startDate = new DateTime(2024, 3, 1, 0, 0, 0); 
            DateTime endDate = new DateTime(2024, 8, 25, 0, 0, 0); 

            try
            {
                var optimizerVM = new OptimizerViewModel();
                if (optimizerVM.StartDate.HasValue && optimizerVM.EndDate.HasValue)
                {
                    startDate = optimizerVM.StartDate.Value.DateTime;
                    endDate = optimizerVM.EndDate.Value.DateTime;
                }
                else
                {
                    Console.WriteLine("No dates available in the Optimizer");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }

            CartesianSeries.Clear();
            var start = startDate;
            var end = endDate;

            if (SelectedDataSource == "Optimization Results")
            {
                var assets = OptimizerViewModel.SharedAssetManager.GetAllAssets().Values
                    .Where(a => a.IsActive).ToList();

                var timestamps = assets
                    .SelectMany(a => a.ProducedHeat.Keys)
                    .Where(t => t >= start && t <= end)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                _preparedLabels = timestamps.Select(t => t.ToString("MM-dd HH:mm")).ToList();
                _preparedXAxisTitle = "Time";
                _preparedYAxisTitle = "Produced Heat (MWh)";

                foreach (var a in assets)
                {
                    var valuesPerDate = timestamps
                        .Select(t => a.ProducedHeat.TryGetValue(t, out var v) ? v ?? 0 : 0)
                        .ToList();

                    CartesianSeries.Add(new ColumnSeries<double>
                    {
                        Name = a.Name,
                        Values = valuesPerDate,
                        MaxBarWidth = 20,
                        Stroke = null
                    });
                }

                SetAxes(_preparedLabels);
            }
            else if (SelectedDataSource == "Heat Demand Data")
            {
                _preparedXAxisTitle = "Date and Hour";
                _preparedYAxisTitle = "Heat Demand (MWh)";

                CartesianSeries.Add(new ColumnSeries<double>
                {
                    Name = "Heat Demand",
                    Values = values,
                    MaxBarWidth = 20,
                    Stroke = null
                });

                SetAxes(labels);
            }

            this.RaisePropertyChanged(nameof(CartesianSeries));
        }

        private void CreateScatterChart(List<double> values, List<string> labels)
        {
            DateTime startDate = new DateTime(2024, 3, 1, 0, 0, 0); 
            DateTime endDate = new DateTime(2024, 8, 25, 0, 0, 0); 

            try
            {
                var optimizerVM = new OptimizerViewModel();
                if (optimizerVM.StartDate.HasValue && optimizerVM.EndDate.HasValue)
                {
                    startDate = optimizerVM.StartDate.Value.DateTime;
                    endDate = optimizerVM.EndDate.Value.DateTime;
                }
                else
                {
                    Console.WriteLine("No dates available in the Optimizer");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }

            CartesianSeries.Clear();
            var start = startDate;
            var end = endDate;   

            if (SelectedDataSource == "Optimization Results")
            {
                var assets = OptimizerViewModel.SharedAssetManager.GetAllAssets().Values
                    .Where(a => a.IsActive).ToList();

                var timestamps = assets
                    .SelectMany(a => a.ProducedHeat.Keys)
                    .Where(t => t >= start && t <= end)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                _preparedLabels = timestamps.Select(t => t.ToString("MM-dd HH:mm")).ToList();
                _preparedXAxisTitle = "Time";
                _preparedYAxisTitle = "Produced Heat (MWh)";

                foreach (var a in assets)
                {
                    var valuesPerDate = timestamps
                        .Select(t => a.ProducedHeat.TryGetValue(t, out var v) ? v ?? 0 : 0)
                        .ToList();

                    CartesianSeries.Add(new ScatterSeries<double>
                    {
                        Name = a.Name,
                        Values = valuesPerDate,
                        DataLabelsPaint = null
                    });
                }

                SetAxes(_preparedLabels);
            }
            else if (SelectedDataSource == "Heat Demand Data")
            {
                _preparedXAxisTitle = "Date and Hour";
                _preparedYAxisTitle = "Heat Demand (MWh)";

                CartesianSeries.Add(new ScatterSeries<double>
                {
                    Name = "Heat Demand",
                    Values = values,
                    DataLabelsPaint = null
                });

                SetAxes(labels);
            }

            this.RaisePropertyChanged(nameof(CartesianSeries));
        }

        private void SetAxes(List<string> labels)
        {
            XAxes.Clear();

            XAxes.Add(new Axis
            {
                Labels = labels.ToArray(),
                Name = ShowDataLabels ? _preparedXAxisTitle : "",
                ShowSeparatorLines = ShowGridLines,
                LabelsRotation = 90,
                TextSize = 12,
                UnitWidth = 1,
                MinStep = 1,
                ForceStepToMin = true,
                MinLimit = -1,
                Padding = new LiveChartsCore.Drawing.Padding(30, 0, 0, 0)
            });

            YAxes.Clear();
            YAxes.Add(new Axis
            {
                Name = ShowDataLabels ? _preparedYAxisTitle : "",
                ShowSeparatorLines = ShowGridLines,
                MinLimit = AutoScale ? null : 0,
                MaxLimit = AutoScale ? null : (_preparedValues.Count > 0 ? _preparedValues.Max() * 1.1 : (double?)null)
            });
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

        private void UpdateChartTypes()
        {
            FilteredChartTypes.Clear();

            if (SelectedDataSource == "Optimization Results")
            {
                FilteredChartTypes.Add("Line Chart");
                FilteredChartTypes.Add("Bar Chart");
                FilteredChartTypes.Add("Scatter Plot");
            }
            else if (SelectedDataSource == "Heat Demand Data")
            {
                FilteredChartTypes.Add("Line Chart");
                FilteredChartTypes.Add("Bar Chart");
                FilteredChartTypes.Add("Scatter Plot");
            }
            else if (SelectedDataSource == "Electricity Price Data")
            {
                FilteredChartTypes.Add("Line Chart");
                FilteredChartTypes.Add("Bar Chart");
                FilteredChartTypes.Add("Scatter Plot");
            }
            else if (SelectedDataSource == "Production Unit Performance")
            {
                FilteredChartTypes.Add("Line Chart");
                FilteredChartTypes.Add("Bar Chart");
                FilteredChartTypes.Add("Scatter Plot");
            }
            else
            {
                // For other data sources, show all chart types
                foreach (var type in AvailableChartTypes)
                    FilteredChartTypes.Add(type);
            }
            // Ensure selected chart type is still valid
            if (!FilteredChartTypes.Contains(SelectedChartType))
            {
                SelectedChartType = FilteredChartTypes.FirstOrDefault() ?? string.Empty;
                this.RaisePropertyChanged(nameof(SelectedChartType));
            }
        }
    }
}