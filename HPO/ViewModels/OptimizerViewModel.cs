﻿using System;
using ReactiveUI;
using HeatProductionOptimization.Models;
using System.Collections.Generic;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.Models.DataModels;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HeatProductionOptimization.ViewModels;

public partial class OptimizerViewModel : ViewModelBase
{
    public static AssetManager SharedAssetManager = new AssetManager();
    public AssetManager assetManager = SharedAssetManager;

    OptAlgorithm alg = new();
    SourceDataManager sourceDataManager = SourceDataManager.sourceDataManagerInstance;
    private bool _isOptimizationRunning;
    private string _statusMessage = "Ready to optimize";
    public List<AssetSpecifications> boilers;
    public static double? Totalcost;
    public static double? TotalCO2;
    public static double? TotalFuel;
    public static double? TotalHeat;
    
    // Optimization parameters
    private bool _considerProductionCost = true;
    private bool _considerCO2Emissions = true;
    private bool _considerFuelConsumption = true;
    private bool _considerElectricity = true;
    private bool _prioritizeRenewable = false;
    private double _heatNeeded = 0;
    private bool heatDemandEnabled = true;
    private double? maxHeat = 0;
    private bool csvHeatDemand = false;

    // Optimization strategy
    private bool _isCostOptimization = true;

    
    public double? MaxHeat
    {
        get => AssetManagerViewModel.MaxHeat;
        set => this.RaiseAndSetIfChanged(ref maxHeat, value);
    }

    public bool HeatDemandEnabled
    {
        get => heatDemandEnabled;
        set => this.RaiseAndSetIfChanged(ref heatDemandEnabled, value);
    }
    
    public bool CsvHeatDemand
    {
        get => csvHeatDemand;
        set
        {
            this.RaiseAndSetIfChanged(ref csvHeatDemand, value);
            DisableOptimization();
            HeatDemandEnabled = !HeatDemandEnabled;
        } 
    }   

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
        boilers = assetManager.GetAllAssets().Values.ToList();
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
        set
        {
            this.RaiseAndSetIfChanged(ref _statusMessage, value);
            this.RaisePropertyChanged(nameof(StatusMessageColor));
        }
    }

    public string DateStatusMessage
    {
        get => _dateStatusMessage;
        set
        {
            this.RaiseAndSetIfChanged(ref _dateStatusMessage, value);
            this.RaisePropertyChanged(nameof(DateStatusMessageColor));
        }
    }

    public string StatusMessageColor
    {
        get
        {
            if (_statusMessage.StartsWith("Error") || _statusMessage.Contains("fail") || 
                _statusMessage.Contains("Cannot") || _statusMessage.Contains("Unable") || _statusMessage.Contains("No data"))
                return "#FF5252"; 
            else if (_statusMessage.Contains("successfully") || _statusMessage.StartsWith("Success"))
                return "#4CAF50"; 
            else if (_statusMessage.Contains("WARNING") || _statusMessage.Contains("progress") || 
                    _statusMessage.StartsWith("not selected"))
                return "#FFC107"; 
            else
                return "#9E9E9E"; 
        }
    }

    public string DateStatusMessageColor
    {
        get
        {
            if (_dateStatusMessage.StartsWith("Error") || _dateStatusMessage.Contains("must") || 
                _dateStatusMessage.Contains("outside"))
                return "#FF5252"; 
            else if (_dateStatusMessage.Contains("successfully"))
                return "#4CAF50"; 
            else if (_dateStatusMessage.Contains("warning") || _dateStatusMessage.Contains("Please") || 
                    _dateStatusMessage.Contains("outside"))
                return "#FFC107"; 
            else
                return "#9E9E9E"; 
        }
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

    public double HeatNeeded
    {
        get => _heatNeeded;
        set 
        {
            this.RaiseAndSetIfChanged(ref _heatNeeded, value);
        } 
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

    public bool CanRunOptimization
    {
        get => _canRunOptimization;
        private set => this.RaiseAndSetIfChanged(ref _canRunOptimization, value);
    }

    public bool ShowDateSelection => UseWinterData ^ UseSummerData;

    //This method checks if the total heat that all assets can produce is less than the max value in the CSV heat demand data 
    //to disble the option of running the optimizer
    public void DisableOptimization()
    {
        List<HeatDemandRecord> WinterData = sourceDataManager.WinterRecords;
        List<HeatDemandRecord> SummerData = sourceDataManager.SummerRecords;
        double? WinterMax = WinterData.Max(w => w.HeatDemand);
        double? SummerMax = SummerData.Max(w => w.HeatDemand);
        if (MaxHeat < WinterMax && UseWinterData)
        {
            CanRunOptimization = false;
            StatusMessage = "Unable to optimize, the units do not have enough power to generate the heat needed";
            return;
        }
        else if (MaxHeat >= WinterMax && UseWinterData)
        {
            CanRunOptimization = true;
            return;
        }
        else if (MaxHeat < SummerMax && UseSummerData)
        {
            CanRunOptimization = false;
            StatusMessage = "Unable to optimize, the units do not have enough power to generate the heat needed";
            return;
        }
        else if (MaxHeat >= SummerMax && UseSummerData)
        {
            CanRunOptimization = true;
            return;
        }
        else
        {
            CanRunOptimization = true;
            return;
        }
    }

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
                        outOfRangeMessage += $"Winter data: {winterStart:dd-MM-yyyy} to {winterEnd:dd-MM-yyyy}\n";
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
                        outOfRangeMessage += $"Summer data: {summerStart:dd-MM-yyyy} to {summerEnd:dd-MM-yyyy}\n";
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
            ? $"{winterRecordsDisplay.Min(r => r.TimeFrom):dd-MM-yyyy} to {winterRecordsDisplay.Max(r => r.TimeTo):dd-MM-yyyy}"
            : "No data";

        string summerRangeText = summerRecordsDisplay.Any()
            ? $"{summerRecordsDisplay.Min(r => r.TimeFrom):dd-MM-yyyy} to {summerRecordsDisplay.Max(r => r.TimeTo):dd-MM-yyyy}"
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

    public void LoadCsvHeat()
    {
        
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
                if (!StartDate.HasValue || !EndDate.HasValue)
                {
                    StatusMessage = "Start date or end date is not selected.";
                    IsOptimizationRunning = false;
                    return;
                }
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

            // Update the actual SharedAssetManager so its contents get modified
            foreach (var pair in boilerdict)
            {
                assetManager.GetAllAssets()[pair.Key] = pair.Value;
            }

            // Get the actual objects from the SharedAssetManager (not a copy!)
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

            //Getting min and max Electricity price for the normalization
            double? minElecprice = ElectricityPrices.Values.Min();
            Console.WriteLine(minElecprice + "minelecprice");
            double? maxElecprice = ElectricityPrices.Values.Max();
            Console.WriteLine(maxElecprice + "maxelecprice");
            
            Dictionary<DateTime, double?> HeatDemand = allData.ToDictionary(v => v.TimeFrom, v => v.HeatDemand);
            foreach (KeyValuePair<DateTime, double?> price in ElectricityPrices)
            {
                double? CurrentHeat;
                if (heatDemandEnabled == true)
                {
                    CurrentHeat = HeatNeeded;
                }
                else
                { 
                    CurrentHeat = HeatDemand[price.Key];
                }
                double? normElecprice = (price.Value - minElecprice) / (maxElecprice - minElecprice);
                alg.OptimizationAlgorithm(boilers, parameters, normElecprice, CurrentHeat, price.Key);
            }

            double? Electricity = alg.CalculateElectricity(boilers, ElectricityPrices);
            Totalcost = alg.CalculateTotalCost(boilers, Electricity);
            TotalCO2 = alg.CalculateTotalCO2(boilers);
            TotalFuel = alg.CalculateTotalFuel(boilers);
            TotalHeat= boilers.Where(a=> a.IsActive).SelectMany(d=> d.ProducedHeat.Values).Sum();

            Console.WriteLine($"Optimization complete. Total cost: {Totalcost}, Total CO2: {TotalCO2} Total units: {boilers.Count}");

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