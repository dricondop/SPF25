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

namespace HeatProductionOptimization.ViewModels;

public partial class OptimizerViewModel : ViewModelBase
{
    OptAlgorithm alg = new();
    SourceDataManager sourceDataManager = SourceDataManager.sourceDataManagerInstance;
    AssetManager assetManager = new();
    private bool _isOptimizationRunning;
    private string _statusMessage = "Ready to optimize";
    private readonly IDataRangeProvider _dataRangeProvider; //Accede al Rango de Fechas
    
    // Optimization parameters
    private bool _considerProductionCost = true;
    private bool _considerCO2Emissions = true;
    private bool _considerFuelConsumption = true;
    private bool _considerElectricity = true;
    private bool _prioritizeRenewable = false;
    
    // Optimization strategy
    private bool _isCostOptimization = true;
    DateTime winterStart;
    DateTime winterEnd;
    DateTime summerStart;
    DateTime summerEnd;
    
    public OptimizerViewModel(IDataRangeProvider dataRangeProvider)
    {
        _dataRangeProvider = dataRangeProvider;
        
        (winterStart, winterEnd, summerStart, summerEnd) = _dataRangeProvider.GetSelectedDateRange();
        // Accede al rango desde DataProviders
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
    
    [RelayCommand]
    public void RunOptimization()
    {
        if (IsOptimizationRunning)
            return;
        
        try
        {
            Console.WriteLine("ninjas");
            IsOptimizationRunning = true;
            StatusMessage = "Optimization in progress...";
            
            // Create parameter array for the algorithm
            int[] parameters = new int[3];
            parameters[0] = ConsiderProductionCost ? 1 : 0;
            parameters[1] = ConsiderCO2Emissions ? 1 : 0;
            parameters[2] = ConsiderFuelConsumption ? 1 : 0;
            
            Console.WriteLine("ninjas2");
            Dictionary<int,AssetSpecifications> boilerdict = assetManager.LoadAssetsSpecifications();
            List<AssetSpecifications> boilers = boilerdict.Values.ToList();
            foreach(var boiler in boilers)
            {
                Console.WriteLine("ninjasaaa");
                boiler.ProducedHeat.Clear();
            }
            Console.WriteLine("ninjas3");
            double? cost = 0;
            List<AssetSpecifications> Units = [];
            List<HeatDemandRecord> WinterData = sourceDataManager.WinterRecords;
            List<HeatDemandRecord> SummerData = sourceDataManager.SummerRecords;
            if(WinterData.Count == 0 && SummerData.Count == 0)
            {
                Console.WriteLine("I am a null ninja");
            }
            Console.WriteLine("ninjas4");
            Dictionary<DateTime, double?> ElectricityPrices = WinterData.Where(n => n.TimeTo <= winterEnd).Where(n => n.TimeFrom >= winterStart).Concat(SummerData.Where(n => n.TimeTo <= summerEnd).Where(n => n.TimeFrom >= summerStart)).ToDictionary(v => v.TimeFrom, v => v.ElectricityPrice);
            if(ElectricityPrices.Count == 0)
            {
                Console.WriteLine("I am not null nigga 2");
                foreach(var nigga in ElectricityPrices)
                {
                    Console.WriteLine(nigga.Value);
                }
            }
            Console.WriteLine("ninjas5");
            foreach(KeyValuePair<DateTime,double?> price in ElectricityPrices)
            {
                Console.WriteLine("ninjasrepeat");
                (List<AssetSpecifications> newUnits, double? newcost) = alg.OptimizationAlgorithm(boilers, parameters, price.Value, 10, price.Key);
                Units.AddRange(newUnits);
                cost += newcost;
            }
            foreach(AssetSpecifications unit in Units)
            {
                Console.WriteLine("ninjasrepeat2");
                Console.WriteLine($"Boiler: {unit.Name}, Heat to be produced: {unit.ProducedHeat}");
            }
            
            StatusMessage = "Optimization completed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Optimization failed: {ex.Message}";
        }
        finally
        {
            IsOptimizationRunning = false;
        }
    }
}
