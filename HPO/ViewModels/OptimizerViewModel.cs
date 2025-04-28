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

namespace HeatProductionOptimization.ViewModels;

public class OptimizerViewModel : ViewModelBase
{
    OptAlgorithm alg = new();
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
    
    public OptimizerViewModel(IDataRangeProvider dataRangeProvider)
    {
        _dataRangeProvider = dataRangeProvider;
        
        var (startDate, endDate) = _dataRangeProvider.GetSelectedDateRange();
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
    
    public async void RunOptimization()
    {
        if (IsOptimizationRunning)
            return;
        
        try
        {
            IsOptimizationRunning = true;
            StatusMessage = "Optimization in progress...";
            
            // Create parameter array for the algorithm
            int[] parameters = new int[3];
            parameters[0] = ConsiderProductionCost ? 1 : 0;
            parameters[1] = ConsiderCO2Emissions ? 1 : 0;
            parameters[2] = ConsiderFuelConsumption ? 1 : 0;
            
            // This would be replaced with actual data in a real implementation
            await Task.Delay(2000); // Simulate processing time

            Dictionary<int,AssetSpecifications> boilerdict = assetManager.LoadAssetsSpecifications();
            List<AssetSpecifications> boilers = boilerdict.Values.ToList();
            foreach(var boiler in boilers)
            {
                boiler.ProducedHeat.Clear();
            }
            
            /* This is a placeholder for what the optimization in the viewModel will look like later, don't worry about it
            Dictionary<DateTime, double> electricityPrices = dynamicElectricityPrices.CurrentElectricityPrice();
            List<AssetSpecifications> Units=[];
            double? cost = 0;
            foreach(KeyValuePair<DateTime,double> price in electricityPrices)
            {
                //TODO: Change this 10 static value for the observable property that is going to get the heat needed from the UI.
                (Units, double ? newcost) = alg.OptimizationAlgorithm(boilers, parameters, price.Value, 10, price.Key);
                cost += newcost; 
            }
            */
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
