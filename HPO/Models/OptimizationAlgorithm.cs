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
    
    //This methods calculates the objective values for every active unit in Production_Units.json
    public Dictionary<AssetSpecifications, double> GetObjective(List<AssetSpecifications> Boilers, int[] par, double? ElectricityPrice, List<AssetSpecifications> HeatPumps, List<AssetSpecifications> GasMotors )
    {
        Dictionary<AssetSpecifications, double> obj = [];
        double objective = 0.0;

        //Getting Min/Max values to normalize the parameters weight in the optimization 
        double? MinCostValue = Boilers.Min(p => p.ProductionCost);
        double? MaxCostValue = Boilers.Max(p => p.ProductionCost);
        double? MinCo2 = Boilers.Min(c  => c.CO2Emissions);
        double? MaxCo2 = Boilers.Max(c => c.CO2Emissions);
        double? MinFuel = Boilers.Min(f => f.FuelConsumption);
        double? MaxFuel = Boilers.Max(f => f.FuelConsumption);

        //This counts the amount of optimization parameters activated
        int n = par.Where(n => n == 1 ).Count();
        if (n == 0)
        {
            //n is a denominator in the objective function, so if there are no parameters active, an exception must be thrown
            throw new DivideByZeroException("No parameters selected for optimization.");
        }
        
        foreach(var hp in HeatPumps)
        {
            //Normalization of the Cost, the Co2 and the fuel efficiency
            double? TemporalConsumption = 0.0;
            double? TemporalPumpCost = (hp.ProductionCost-MinCostValue)/(MaxCostValue-MinCostValue);
            TemporalPumpCost += ElectricityPrice; // Electricity prices normalized in the view model are added to the production cost

            //Calculate objective values for every heat pump
            objective = ((TemporalPumpCost ?? 0.0) * par[0] + (TemporalConsumption ?? 0.0) * par[1] + (hp.CO2Emissions ?? 0.0) * par[2])/ n;
            //Console.WriteLine($"Boiler {hp.Name} objective value: {objective}");
            obj[hp] = objective;
        }
        
        foreach(var gm in GasMotors)
        {
            //Normalization of the Cost, the Co2 and the Consumtion
            double? TemporalConsumption = (gm.FuelConsumption - MinFuel) / (MaxFuel - MinFuel);
            double? TemporalCo2 = (gm.CO2Emissions - MinCo2) / (MaxCo2 - MinCo2);
            double? TemporalMotorCost = (gm.ProductionCost -MinCostValue) / (MaxCostValue - MinCostValue);
            TemporalMotorCost -= ElectricityPrice; // Electricity prices normalized in the view model are substracted to the production cost
            
            //Calculate objective values for every gas motor
            objective = ((TemporalMotorCost ?? 0.0) * par[0] + (TemporalConsumption ?? 0.0) * par[1] + ( TemporalCo2 ?? 0.0) * par[2])/ n;
            //Console.WriteLine($"Boiler {gm.Name} objective value: {objective}");
            obj[gm] = objective;
        }

        for(int i = 0; i < Boilers.Count; i++)
        {
            if(Boilers[i].UnitType == "Boiler")
            {
                //Normalization of the Cost, the Co2 and the Consumtion
                double? TemporalConsumption = (Boilers[i].FuelConsumption - MinFuel) / (MaxFuel - MinFuel);
                double? TemporalCo2 = (Boilers[i].CO2Emissions - MinCo2) / (MaxCo2 - MinCo2);
                double? TemporalBoilerCost = (Boilers[i].ProductionCost - MinCostValue) / (MaxCostValue - MinCostValue);
                objective = ((TemporalBoilerCost ?? 0.0) * par[0] + (TemporalConsumption?? 0.0) * par[1] + ( TemporalCo2?? 0.0) * par[2])/ n;
                obj[Boilers[i]] = objective;
                //Console.WriteLine($"Boiler {Boilers[i].Name} objective value: {objective}");
            }
        } 
        //obj is a dictionary that has the objective value as a double and the unit as an AssetSpecifications object
        return obj;
    }

    //This method distributes the heat demand across the unit depending on their objective values (profitability)
    public void CalculateUnits(List<AssetSpecifications> Boilers, Dictionary<AssetSpecifications,double> boilerdict, double? heat, DateTime hour)
    {
        //This orders the objective values associated with each unit
        double[] order = boilerdict.Values.OrderBy(o => o).Distinct().ToArray();

        //Heat demand the user sets in the GUI
        double? heatneeded = heat;

        //Foreach value in objective values
        for(int i = 0; i<order.Count(); i++)
        {
            // Gets the unit corresponding to the current objective value 
            var Val = boilerdict.Where(v => v.Value == order[i]).Select(k => k.Key).ToList();
            // Necessary because there could be multiple units with the same objective value
            foreach (var val in Val)
            {
                int[] indexes = Boilers.Select((value, index) => new { value, index }).Where(n => n.value == val).Select(x => x.index).ToArray();

                //The for i in range distributes the heat throughout the units in indexes
                for (int j = 0; j < indexes.Count(); j++)
                {
                    if (heatneeded > Boilers[indexes[j]].MaxHeat)
                    {
                        Boilers[indexes[j]].ProducedHeat[hour] = Boilers[indexes[j]].MaxHeat;
                        heatneeded -= Boilers[indexes[j]].MaxHeat;
                        Console.WriteLine($"The Boiler:  {Boilers[indexes[j]].Name} is produces {Boilers[indexes[j]].MaxHeat}");
                    }
                    else
                    {
                        Boilers[indexes[j]].ProducedHeat[hour] = heatneeded;
                        heatneeded = 0;
                    }
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
        return  PumpsCost - MotorBenefit;
    }

    public void OptimizationAlgorithm(List<AssetSpecifications> boilers, int[] par, double? ElectricityPrice,double? heat, DateTime time)
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
