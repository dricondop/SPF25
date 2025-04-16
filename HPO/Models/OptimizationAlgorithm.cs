using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using Avalonia.Controls.Platform;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Models;

public class OptAlgorithm
{
    public Dictionary<double, AssetSpecification> Objective = [];
    
    //The method will ask for the list of specifications and also if the different parameters(par) are to be considered or not.
    public Dictionary<double, AssetSpecification> GetObjective(List<AssetSpecification> boilers, int[] par, double ElectricityPrice)
    {
        List<AssetSpecification> HeatPumps = boilers.Where( n => n.UnitType == "Motor").ToList();
        HeatPumps = HeatPumps.Select(n => {n.ProductionCost += ElectricityPrice; return n;}).ToList();
        List<AssetSpecification> GasMotors =  boilers.Where( n => n.UnitType == "Heat Pump").ToList();
        GasMotors = GasMotors.Select(n => {n.ProductionCost -= ElectricityPrice; return n;}).ToList();

        Dictionary<double, AssetSpecification> obj = [];
        double objective = 0.0;
        int n = par.Where(n => n == 1 ).Count();
        if (n == 0)
        {
            throw new DivideByZeroException("No parameters selected for optimization.");
        }
        for(int i = 0; i < boilers.Count; i++)
        {
            objective = ((boilers[i].ProductionCost ?? 0.0) * par[i] + (boilers[i].CO2Emissions ?? 0.0) * par[i] + (boilers[i].FuelConsumption ?? 0.0) * par[i])/ n;

            obj[objective] = boilers[i];
        } 

        return obj ?? [];
    }

    public void CalculateUnits(List<AssetSpecification> boilers, Dictionary<double,AssetSpecification> boilerdict, double heat)
    {
        foreach( var boiler in boilers)
        {
            boiler.UnitsProduction = 0.0;
        }

        double[] order = boilerdict.Keys.ToArray();

        order =  order.OrderBy(o => o).ToArray();

        double? heatneeded = heat;

        for(int i = 0; i<order.Count(); i++)
        {
            var val = boilerdict[order[i]];
            int[] indexes = boilers.Select((value, index) => new {value,index}).Where(n=> n.value == val).Select(x=> x.index).ToArray();

            for(int j = 0; j<indexes.Count(); j++)
            {
                if(heatneeded > boilers[indexes[j]].MaxHeat)
                {
                    boilers[indexes[j]].UnitsProduction = boilers[indexes[j]].MaxHeat;
                    heatneeded -=  boilers[indexes[j]].MaxHeat;
                }
                else
                {
                    boilers[indexes[j]].UnitsProduction = heatneeded;
                    heatneeded = 0;
                    return;
                }
            }
        }
    }
    public void CalculateElectricity(List<AssetSpecification> HeatPumps, List<AssetSpecification> GasMotors, double ElectricityPrice)
    {
        double? ElectricityProduced = GasMotors.Sum(n => n.UnitsProduction)*0.742857;
        double? ElectricityConsumed = HeatPumps.Sum(n => n.UnitsProduction);
        double? Mwh = ElectricityProduced - ElectricityConsumed;
        double? Cost_Benefit = Mwh * ElectricityPrice;
    }
}
