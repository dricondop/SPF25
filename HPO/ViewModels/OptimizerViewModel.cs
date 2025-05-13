﻿using System;
using ReactiveUI;
using System.Threading.Tasks;
using HeatProductionOptimization.Models;
using System.Collections.Generic;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.Models.DataModels;
using Avalonia.Interactivity;
using Avalonia.Controls;
using System.Linq;
using HeatProductionOptimization.Services.DataProviders;
using CommunityToolkit.Mvvm.Input;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.ViewModels;

public partial class OptimizerViewModel : ViewModelBase
{
    public static AssetManager SharedAssetManager = new AssetManager();
    private AssetManager assetManager = SharedAssetManager;

    OptAlgorithm alg = new();
    SourceDataManager sourceDataManager = SourceDataManager.sourceDataManagerInstance;
    private bool _isOptimizationRunning;
    private string _statusMessage = "Ready to optimize";

    // Optimization parameters
    private bool _considerProductionCost = true;
    private bool _considerCO2Emissions = true;
    private bool _considerFuelConsumption = true;
    private bool _considerElectricity = true;
    private bool _prioritizeRenewable = false;

    // Optimization strategy
    private bool _isCostOptimization = true;

    // Date selection properties from DateInputWindowViewModel
    private DateTimeOffset? _startDate;
    private DateTimeOffset? _endDate;
    private int _startHour;
    private int _endHour;
    private string _dateStatusMessage = string.Empty;
    private bool _canRunOptimization;
    private bool _useWinterData = true;
    private bool _useSummerData = true;
    private List<int> _hourOptions;
    
    public OptimizerViewModel() 
    {
        _hourOptions = Enumerable.Range(0, 24).ToList();
        UpdateDefaultDates();

        this.WhenAnyValue(
            x => x.UseWinterData,
            x => x.UseSummerData)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ShowDateSelection));
                UpdateDefaultDates();
            });
    }
    
    // Original properties
    public bool IsOptimizationRunning
    {
        get => _isOptimizationRunning;
        set => this.RaiseAndSetIfChanged(ref _isOptimizationRunning, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool ConsiderProductionCost
    {
        get => _considerProductionCost;
        set => this.RaiseAndSetIfChanged(ref _considerProductionCost, value);
    }

    public bool ConsiderCO2Emissions
    {
        get => _considerCO2Emissions;
        set => this.RaiseAndSetIfChanged(ref _considerCO2Emissions, value);
    }

    public bool ConsiderFuelConsumption
    {
        get => _considerFuelConsumption;
        set => this.RaiseAndSetIfChanged(ref _considerFuelConsumption, value);
    }

    public bool ConsiderElectricity
    {
        get => _considerElectricity;
        set => this.RaiseAndSetIfChanged(ref _considerElectricity, value);
    }

    public bool PrioritizeRenewable
    {
        get => _prioritizeRenewable;
        set => this.RaiseAndSetIfChanged(ref _prioritizeRenewable, value);
    }

    public bool IsCostOptimization
    {
        get => _isCostOptimization;
        set => this.RaiseAndSetIfChanged(ref _isCostOptimization, value);
    }

    // Date selection properties
    public List<int> HourOptions => _hourOptions;

    public bool UseWinterData
    {
        get => _useWinterData;
        set => this.RaiseAndSetIfChanged(ref _useWinterData, value);
    }

    public bool UseSummerData
    {
        get => _useSummerData;
        set => this.RaiseAndSetIfChanged(ref _useSummerData, value);
    }

    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _startDate, value);
            ValidateDates();
        }
    }

    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _endDate, value);
            ValidateDates();
        }
    }

    public int StartHour
    {
        get => _startHour;
        set
        {
            this.RaiseAndSetIfChanged(ref _startHour, value);
            ValidateDates();
        }
    }

    public int EndHour
    {
        get => _endHour;
        set
        {
            this.RaiseAndSetIfChanged(ref _endHour, value);
            ValidateDates();
        }
    }

    public string DateStatusMessage
    {
        get => _dateStatusMessage;
        private set => this.RaiseAndSetIfChanged(ref _dateStatusMessage, value);
    }

    public bool CanRunOptimization
    {
        get => _canRunOptimization;
        private set => this.RaiseAndSetIfChanged(ref _canRunOptimization, value);
    }

    public bool ShowDateSelection => UseWinterData ^ UseSummerData;

    // Date selection methods
    private void UpdateDefaultDates()
    {
        var (startDate, endDate) = GetAvailableDataRange();
        
        // Check if dates are valid before assigning
        if (startDate == DateTime.MinValue || startDate == DateTime.MaxValue)
            startDate = DateTime.Today;
        
        if (endDate == DateTime.MinValue || endDate == DateTime.MaxValue)
            endDate = DateTime.Today.AddDays(1);
        
        // Use constructor with explicit date
        StartDate = new DateTimeOffset(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, TimeSpan.Zero);
        EndDate = new DateTimeOffset(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0, TimeSpan.Zero);
        
        ValidateDates();
    }

    private (DateTime start, DateTime end) GetAvailableDataRange()
    {
        DateTime minDate = DateTime.MaxValue;
        DateTime maxDate = DateTime.MinValue;
        bool hasData = false;

        if (UseWinterData)
        {
            // Get winter data range from source data manager
            var winterRecords = sourceDataManager.WinterRecords;
            if (winterRecords.Any())
            {
                var winterStart = winterRecords.Min(r => r.TimeFrom);
                var winterEnd = winterRecords.Max(r => r.TimeTo);
                if (winterStart < minDate) minDate = winterStart;
                if (winterEnd > maxDate) maxDate = winterEnd;
                hasData = true;
            }
        }

        if (UseSummerData)
        {
            // Get summer data range from source data manager
            var summerRecords = sourceDataManager.SummerRecords;
            if (summerRecords.Any())
            {
                var summerStart = summerRecords.Min(r => r.TimeFrom);
                var summerEnd = summerRecords.Max(r => r.TimeTo);
                if (summerStart < minDate) minDate = summerStart;
                if (summerEnd > maxDate) maxDate = summerEnd;
                hasData = true;
            }
        }

        if (!hasData)
        {
            minDate = DateTime.Today;
            maxDate = DateTime.Today.AddDays(1);
        }
        
        if (minDate == DateTime.MinValue || minDate == DateTime.MaxValue)
            minDate = DateTime.Today;
        
        if (maxDate == DateTime.MinValue || maxDate == DateTime.MaxValue)
            maxDate = DateTime.Today.AddDays(1);
        
        return (minDate, maxDate);
    }

    private void ValidateDates()
    {
        if (!UseWinterData && !UseSummerData)
        {
            DateStatusMessage = "Please select at least one data group";
            CanRunOptimization = false;
            return;
        }

        if (ShowDateSelection)
        {
            if (!StartDate.HasValue || !EndDate.HasValue)
            {
                DateStatusMessage = "Please select both dates";
                CanRunOptimization = false;
                return;
            }

            var start = StartDate.Value.DateTime.AddHours(StartHour);
            var end = EndDate.Value.DateTime.AddHours(EndHour);

            if (start >= end)
            {
                DateStatusMessage = "Error: End date must be after start date";
                CanRunOptimization = false;
                return;
            }

            bool isValid = true;
            string outOfRangeMessage = string.Empty;

            if (UseWinterData)
            {
                var winterRecords = sourceDataManager.WinterRecords;
                if (winterRecords.Any())
                {
                    var winterStart = winterRecords.Min(r => r.TimeFrom);
                    var winterEnd = winterRecords.Max(r => r.TimeTo);
                    
                    if (start < winterStart || end > winterEnd)
                    {
                        outOfRangeMessage += $"Winter data: {winterStart:yyyy-MM-dd} to {winterEnd:yyyy-MM-dd}\n";
                        isValid = false;
                    }
                }
                else
                {
                    outOfRangeMessage += "No winter data available\n";
                    isValid = false;
                }
            }

            if (UseSummerData)
            {
                var summerRecords = sourceDataManager.SummerRecords;
                if (summerRecords.Any())
                {
                    var summerStart = summerRecords.Min(r => r.TimeFrom);
                    var summerEnd = summerRecords.Max(r => r.TimeTo);
                    
                    if (start < summerStart || end > summerEnd)
                    {
                        outOfRangeMessage += $"Summer data: {summerStart:yyyy-MM-dd} to {summerEnd:yyyy-MM-dd}\n";
                        isValid = false;
                    }
                }
                else
                {
                    outOfRangeMessage += "No summer data available\n";
                    isValid = false;
                }
            }

            if (!isValid)
            {
                DateStatusMessage = $"Selected range is outside available data:\n{outOfRangeMessage}";
                CanRunOptimization = false;
                return;
            }
        }

        var winterRecordsDisplay = sourceDataManager.WinterRecords;
        var summerRecordsDisplay = sourceDataManager.SummerRecords;
        
        string winterRangeText = winterRecordsDisplay.Any() 
            ? $"{winterRecordsDisplay.Min(r => r.TimeFrom):yyyy-MM-dd} to {winterRecordsDisplay.Max(r => r.TimeTo):yyyy-MM-dd}"
            : "No data";

        string summerRangeText = summerRecordsDisplay.Any()
            ? $"{summerRecordsDisplay.Min(r => r.TimeFrom):yyyy-MM-dd} to {summerRecordsDisplay.Max(r => r.TimeTo):yyyy-MM-dd}"
            : "No data";

        if (UseWinterData && !UseSummerData)
        {
            DateStatusMessage = $"Winter data range: {winterRangeText}";
        }
        else if (!UseWinterData && UseSummerData)
        {
            DateStatusMessage = $"Summer data range: {summerRangeText}";
        }
        else
        {
            DateStatusMessage = $"Available data ranges: Winter: {winterRangeText}, Summer: {summerRangeText}";
        }

        CanRunOptimization = true;
    }
    
    [RelayCommand]
    public void RunOptimization()
    {
        if (IsOptimizationRunning)
            return;
        
        // First validate the dates
        ValidateDates();
        if (!CanRunOptimization)
        {
            StatusMessage = "Cannot run optimization - please fix date selection issues";
            return;
        }
        
        Console.WriteLine("RunOptimization started");
        Console.WriteLine("Optimizer AssetManager Hash: " + assetManager.GetHashCode());


        try
        {
            IsOptimizationRunning = true;
            StatusMessage = "Optimization in progress...";

            // Date range
            DateTime startDate, endDate;
            bool useWinterData = UseWinterData;
            bool useSummerData = UseSummerData;
            
            if (ShowDateSelection)
            {
                startDate = StartDate.Value.DateTime.AddHours(StartHour);
                endDate = EndDate.Value.DateTime.AddHours(EndHour);
            }
            else
            {
                var (start, end) = GetAvailableDataRange();
                startDate = start;
                endDate = end;
            }
            
            Console.WriteLine($"Selected date range: {startDate} to {endDate}");
            Console.WriteLine($"Using Winter data: {useWinterData}, Summer data: {useSummerData}");

            // Create parameter array for the algorithm
            int[] parameters = new int[3];
            parameters[0] = ConsiderProductionCost ? 1 : 0;
            parameters[1] = ConsiderCO2Emissions ? 1 : 0;
            parameters[2] = ConsiderFuelConsumption ? 1 : 0;
            Console.WriteLine($"Parameters: Production Cost={parameters[0]}, CO2={parameters[1]}, Fuel={parameters[2]}");

            Console.WriteLine("Loading asset specifications...");
            Dictionary<int, AssetSpecifications> boilerdict = assetManager.LoadAssetsSpecifications();

            // ✅ Update the actual SharedAssetManager so its contents get modified
            foreach (var pair in boilerdict)
            {
                assetManager.GetAllAssets()[pair.Key] = pair.Value;
            }

            // ✅ Get the actual objects from the SharedAssetManager (not a copy!)
            List<AssetSpecifications> boilers = assetManager.GetAllAssets().Values.ToList();

            Console.WriteLine($"Loaded {boilers.Count} boilers");

            foreach (var boiler in boilers)
            {
                boiler.ProducedHeat.Clear();
            }
            foreach (var boiler in boilers)
            {
                Console.WriteLine($"Boiler: {boiler.Name}, ProducedHeat Count: {boiler.ProducedHeat.Count}");
                foreach (var entry in boiler.ProducedHeat)
                {
                    Console.WriteLine($"  Hour: {entry.Key}, Heat Produced: {entry.Value}");
                }
            }

            List<HeatDemandRecord> WinterData = sourceDataManager.WinterRecords;
            List<HeatDemandRecord> SummerData = sourceDataManager.SummerRecords;
            Console.WriteLine($"Data loaded - Winter records: {WinterData.Count}, Summer records: {SummerData.Count}");

            // Filter data 
            Console.WriteLine("Filtering data for selected date range...");
            List<HeatDemandRecord> filteredWinterData = useWinterData ?
                WinterData.Where(r => r.TimeFrom >= startDate && r.TimeTo <= endDate).ToList() :
                new List<HeatDemandRecord>();

            List<HeatDemandRecord> filteredSummerData = useSummerData ?
                SummerData.Where(r => r.TimeFrom >= startDate && r.TimeTo <= endDate).ToList() :
                new List<HeatDemandRecord>();

            Console.WriteLine($"Filtered data: Winter={filteredWinterData.Count}, Summer={filteredSummerData.Count}");

            if (filteredWinterData.Count == 0 && filteredSummerData.Count == 0)
            {
                Console.WriteLine("WARNING: No data found for the selected date range!");
                StatusMessage = "No data found for the selected date range.";
                IsOptimizationRunning = false;
                return;
            }

            List<HeatDemandRecord> allData = filteredWinterData.Concat(filteredSummerData).ToList();
            Console.WriteLine($"Combined filtered data: {allData.Count} records");

            Dictionary<DateTime, double?> ElectricityPrices = allData.ToDictionary(v => v.TimeFrom, v => v.ElectricityPrice);
            Console.WriteLine($"Electricity price entries: {ElectricityPrices.Count}");

            Console.WriteLine("Starting optimization loop...");


            foreach (KeyValuePair<DateTime, double?> price in ElectricityPrices)
            {
                alg.OptimizationAlgorithm(boilers, parameters, price.Value, 10, price.Key);
            }

            double? Electricity = alg.CalculateElectricity(boilers, ElectricityPrices);
            double? Totalcost = alg.CalculateTotalCost(boilers, Electricity);


            Console.WriteLine($"Optimization complete. Total cost: {Totalcost}, Total units: {boilers.Count}");

            StatusMessage = "Optimization completed successfully";
            foreach (var boiler in boilers)
            {
                Console.WriteLine($"Boiler: {boiler.Name}, IsActive: {boiler.IsActive}");
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            StatusMessage = $"Optimization failed: {ex.Message}";
        }
        finally
        {
            IsOptimizationRunning = false;
            Console.WriteLine("RunOptimization finished");
        }
    }
}