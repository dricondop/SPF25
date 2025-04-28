using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using Avalonia.Controls.Platform;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Models;

public class OptAlgorithm
{
    public Dictionary<double, AssetSpecifications> Objective = [];
    
    //The method will ask for the list of specifications and also if the different parameters(par) are to be considered or not.
    public Dictionary<double, AssetSpecifications> GetObjective(List<AssetSpecifications> Boilers, int[] par, double ElectricityPrice, List<AssetSpecifications> HeatPumps, List<AssetSpecifications> GasMotors )
    {
        foreach(var hp in HeatPumps)
        {
            hp.ProductionCost += ElectricityPrice;
        }
        
        foreach(var gm in GasMotors)
        {
            gm.ProductionCost -= ElectricityPrice;
        }

        Dictionary<double, AssetSpecifications> obj = [];
        double objective = 0.0;

        int n = par.Where(n => n == 1 ).Count();
        if (n == 0)
        {
            throw new DivideByZeroException("No parameters selected for optimization.");
        }
        for(int i = 0; i < Boilers.Count; i++)
        {
            objective = ((Boilers[i].ProductionCost ?? 0.0) * par[i] + (Boilers[i].CO2Emissions ?? 0.0) * par[i] + (Boilers[i].FuelConsumption ?? 0.0) * par[i])/ n;

            obj[objective] = Boilers[i];
        } 

        return obj;
    }

    public List<AssetSpecifications> CalculateUnits(List<AssetSpecifications> Boilers, Dictionary<double,AssetSpecifications> boilerdict, double heat, DateTime hour)
    {
        double[] order = boilerdict.Keys.ToArray();

        order =  order.OrderBy(o => o).ToArray();

        double? heatneeded = heat;

        for(int i = 0; i<order.Count(); i++)
        {
            var val = boilerdict[order[i]];
            int[] indexes = Boilers.Select((value, index) => new {value,index}).Where(n=> n.value == val).Select(x=> x.index).ToArray();

            for(int j = 0; j<indexes.Count(); j++)
            {
                if(heatneeded > Boilers[indexes[j]].MaxHeat)
                {
                    Boilers[indexes[j]].ProducedHeat[hour] = Boilers[indexes[j]].MaxHeat;
                    heatneeded -=  Boilers[indexes[j]].MaxHeat;
                }
                else
                {
                    Boilers[indexes[j]].ProducedHeat[hour] = heatneeded;
                    heatneeded = 0;
                }
            }
        }
        return Boilers;
    }
    public double? CalculateElectricity(List<AssetSpecifications> HeatPumps, List<AssetSpecifications> GasMotors, double ElectricityPrice)
    {
        double? ElectricityProduced = GasMotors.SelectMany(n => n.ProducedHeat.Values).Sum() * 0.742857;
        double? ElectricityConsumed = HeatPumps.SelectMany(n => n.ProducedHeat.Values).Sum();
        double? Mwh = ElectricityProduced - ElectricityConsumed;
        return  Mwh * ElectricityPrice;
    }

    public (List<AssetSpecifications>,double?) OptimizationAlgorithm(List<AssetSpecifications> boilers, int[] par, double ElectricityPrice,double heat, DateTime time)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        List<AssetSpecifications> HeatPumps = Boilers.Where( n => n.UnitType == "Heat Pump").ToList();
        List<AssetSpecifications> GasMotors =  Boilers.Where( n => n.UnitType == "Motor").ToList();
        var Units = CalculateUnits(Boilers, GetObjective(Boilers, par, ElectricityPrice, HeatPumps, GasMotors), heat, time);
        var Electricity =  CalculateElectricity(HeatPumps, GasMotors, ElectricityPrice);
        double? Cost = CalculateTotalCost(Boilers,Electricity);

        return (Units, Cost);
    }

    public double? CalculateTotalCost(List<AssetSpecifications> boilers, double? Electricity)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        double? TotalCost = 0;
        foreach(var boiler in boilers)
        {
            TotalCost += boiler.ProductionCost * boiler.ProducedHeat.Values.Sum();
        }
        TotalCost += Electricity; 
        return TotalCost;
    }
}
