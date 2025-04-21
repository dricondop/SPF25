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

    public void CalculateUnits(List<AssetSpecifications> Boilers, Dictionary<double,AssetSpecifications> boilerdict, double heat)
    {

        
        foreach( var boiler in Boilers)
        {
            boiler.UnitsProduction = 0.0;
        }

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
                    Boilers[indexes[j]].UnitsProduction = Boilers[indexes[j]].MaxHeat;
                    heatneeded -=  Boilers[indexes[j]].MaxHeat;
                }
                else
                {
                    Boilers[indexes[j]].UnitsProduction = heatneeded;
                    heatneeded = 0;
                    return;
                }
            }
        }
    }
    public double? CalculateElectricity(List<AssetSpecifications> HeatPumps, List<AssetSpecifications> GasMotors, double ElectricityPrice)
    {
        double? ElectricityProduced = GasMotors.Sum(n => n.UnitsProduction) * 0.742857;
        double? ElectricityConsumed = HeatPumps.Sum(n => n.UnitsProduction);
        double? Mwh = ElectricityProduced - ElectricityConsumed;
        return  Mwh * ElectricityPrice;
    }

    public void OptimizationAlgorithm(List<AssetSpecifications> boilers, int[] par, double ElectricityPrice,double heat)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        List<AssetSpecifications> HeatPumps = Boilers.Where( n => n.UnitType == "Heat Pump").ToList();
        List<AssetSpecifications> GasMotors =  Boilers.Where( n => n.UnitType == "Motor").ToList();
        CalculateUnits(Boilers, GetObjective(Boilers, par, ElectricityPrice, HeatPumps, GasMotors), heat);
        CalculateElectricity(HeatPumps, GasMotors, ElectricityPrice); 
    }
}
