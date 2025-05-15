using System;
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
    private AssetManager assetManager = SharedAssetManager;

    OptAlgorithm alg = new();
    SourceDataManager sourceDataManager = SourceDataManager.sourceDataManagerInstance;
    private bool _isOptimizationRunning;
    private string _statusMessage = "Ready to optimize";
    public List<AssetSpecifications> boilers;
    
    // Optimization parameters
    private bool _considerProductionCost = true;
    private bool _considerCO2Emissions = true;
    private bool _considerFuelConsumption = true;
    private bool _considerElectricity = true;
    private bool _prioritizeRenewable = false;
    private double _heatNeeded = 0;
    private double? maxHeat = 0;

    // Optimization strategy
    private bool _isCostOptimization = true;

    public OptimizerViewModel() 
    {
        // Get the actual objects from the SharedAssetManager (not a copy!)
        boilers = assetManager.GetAllAssets().Values.ToList();
    }

    
    public double? MaxHeat
    {
        get => AssetManagerViewModel.MaxHeat;
        set => this.RaiseAndSetIfChanged(ref maxHeat, value);
    }
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

    public double HeatNeeded
    {
        get => _heatNeeded;
        set 
        {
            this.RaiseAndSetIfChanged(ref _heatNeeded, value);
        } 
    }

    [RelayCommand]
    public void RunOptimization()
    {
        if (IsOptimizationRunning)
            return;

        Console.WriteLine("RunOptimization started");
        Console.WriteLine("Optimizer AssetManager Hash: " + assetManager.GetHashCode());

        try
        {
            
            IsOptimizationRunning = true;
            StatusMessage = "Optimization in progress...";

            // Date range
            var selectedRange = DateInputWindowViewModel.SelectedDateRange;
            Console.WriteLine($"Selected date range: {selectedRange.StartDate} to {selectedRange.EndDate}");
            Console.WriteLine($"Using Winter data: {selectedRange.UseWinterData}, Summer data: {selectedRange.UseSummerData}");

            DateTime startDate = selectedRange.StartDate;
            DateTime endDate = selectedRange.EndDate;
            bool useWinterData = selectedRange.UseWinterData;
            bool useSummerData = selectedRange.UseSummerData;

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


            foreach (KeyValuePair<DateTime, double?> price in ElectricityPrices)
            {
                alg.OptimizationAlgorithm(boilers, parameters, price.Value, _heatNeeded, price.Key);
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