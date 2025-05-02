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
    public Dictionary<double, AssetSpecifications> GetObjective(List<AssetSpecifications> Boilers, int[] par, double? ElectricityPrice, List<AssetSpecifications> HeatPumps, List<AssetSpecifications> GasMotors )
    {
        foreach(var hp in HeatPumps)
        {
            hp.ProductionCost = 60;
            hp.ProductionCost += ElectricityPrice;
        }
        
        foreach(var gm in GasMotors)
        {
            gm.ProductionCost = 990;
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
            objective = ((Boilers[i].ProductionCost ?? 0.0) * par[0] + (Boilers[i].CO2Emissions ?? 0.0) * par[1] + (Boilers[i].FuelConsumption ?? 0.0) * par[2])/ n;

            obj[objective] = Boilers[i];
        } 

        return obj;
    }

    public void CalculateUnits(List<AssetSpecifications> Boilers, Dictionary<double,AssetSpecifications> boilerdict, double heat, DateTime hour)
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
        
    }
    public double? CalculateElectricity(List<AssetSpecifications> boilers, Dictionary<DateTime,double?> ElectricityPrice)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        List<AssetSpecifications> HeatPumps = Boilers.Where( n => n.UnitType == "Heat Pump").ToList();
        List<AssetSpecifications> GasMotors =  Boilers.Where( n => n.UnitType == "Motor").ToList();

        double? MotorBenefit = 0;
        double? PumpsCost = 0;
        foreach(var motor in GasMotors)
        {
            foreach(KeyValuePair<DateTime,double?> price in ElectricityPrice)
            {
                if(motor.ProducedHeat.ContainsKey(price.Key))
                {
                    MotorBenefit += motor.ProducedHeat[price.Key] * 0.742857 * price.Value;
                }
            }
        }

        foreach(var pump in HeatPumps)
        {
        foreach(KeyValuePair<DateTime,double?> price in ElectricityPrice)
        {
            if(pump.ProducedHeat.ContainsKey(price.Key))
            {
                PumpsCost += pump.ProducedHeat[price.Key] * price.Value;
            }
        }
        }
        return  MotorBenefit - PumpsCost;
    }

    public void OptimizationAlgorithm(List<AssetSpecifications> boilers, int[] par, double? ElectricityPrice,double heat, DateTime time)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        List<AssetSpecifications> HeatPumps = Boilers.Where( n => n.UnitType == "Heat Pump").ToList();
        List<AssetSpecifications> GasMotors =  Boilers.Where( n => n.UnitType == "Motor").ToList();
        CalculateUnits(Boilers, GetObjective(Boilers, par, ElectricityPrice, HeatPumps, GasMotors), heat, time);
    
        // double? Cost = CalculateTotalCost(Boilers,Electricity);
    }

    public double? CalculateTotalCost(List<AssetSpecifications> boilers, double? Electricity)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        double? TotalCost = 0;
        foreach(var boiler in boilers)
        {
            TotalCost += boiler.ProductionCost * boiler.ProducedHeat.Values.Sum();
        }

        return TotalCost + Electricity;
    }
}
