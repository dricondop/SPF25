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
    public Dictionary<AssetSpecifications, double> GetObjective(List<AssetSpecifications> Boilers, int[] par, double? ElectricityPrice, List<AssetSpecifications> HeatPumps, List<AssetSpecifications> GasMotors)
    {
        Dictionary<AssetSpecifications, double> obj = [];
        double objective = 0.0;

        //Getting Min/Max values to normalize the parameters weight in the optimization 
        double? MinCostValue = Boilers.Min(p => p.ProductionCost);
        double? MaxCostValue = Boilers.Max(p => p.ProductionCost);
        double? MinCo2 = Boilers.Min(c => c.CO2Emissions);
        double? MaxCo2 = Boilers.Max(c => c.CO2Emissions);
        double? MinFuel = Boilers.Min(f => f.FuelConsumption);
        double? MaxFuel = Boilers.Max(f => f.FuelConsumption);

        //This counts the amount of optimization parameters activated
        int n = par.Where(n => n == 1).Count();
        if (n == 0)
        {
            //n is a denominator in the objective function, so if there are no parameters active, an exception must be thrown
            throw new DivideByZeroException("No parameters selected for optimization.");
        }

        foreach (var hp in HeatPumps)
        {
            //Normalization of the Cost, the Co2 and the fuel efficiency
            double? TemporalConsumption = 0.0;
            double? TemporalPumpCost = (hp.ProductionCost - MinCostValue) / (MaxCostValue - MinCostValue);
            TemporalPumpCost += ElectricityPrice; // Electricity prices normalized in the view model are added to the production cost

            //Calculate objective values for every heat pump
            objective = ((TemporalPumpCost ?? 0.0) * par[0] + (TemporalConsumption ?? 0.0) * par[1] + (hp.CO2Emissions ?? 0.0) * par[2]) / n;
            //Console.WriteLine($"Boiler {hp.Name} objective value: {objective}");
            obj[hp] = objective;
        }

        foreach (var gm in GasMotors)
        {
            //Normalization of the Cost, the Co2 and the Consumtion
            double? TemporalConsumption = (gm.FuelConsumption - MinFuel) / (MaxFuel - MinFuel);
            double? TemporalCo2 = (gm.CO2Emissions - MinCo2) / (MaxCo2 - MinCo2);
            double? TemporalMotorCost = (gm.ProductionCost - MinCostValue) / (MaxCostValue - MinCostValue);
            TemporalMotorCost -= ElectricityPrice; // Electricity prices normalized in the view model are substracted to the production cost

            //Calculate objective values for every gas motor
            objective = ((TemporalMotorCost ?? 0.0) * par[0] + (TemporalConsumption ?? 0.0) * par[1] + (TemporalCo2 ?? 0.0) * par[2]) / n;
            //Console.WriteLine($"Boiler {gm.Name} objective value: {objective}");
            obj[gm] = objective;
        }

        for (int i = 0; i < Boilers.Count; i++)
        {
            if (Boilers[i].UnitType == "Boiler")
            {
                //Normalization of the Cost, the Co2 and the Consumtion
                double? TemporalConsumption = (Boilers[i].FuelConsumption - MinFuel) / (MaxFuel - MinFuel);
                double? TemporalCo2 = (Boilers[i].CO2Emissions - MinCo2) / (MaxCo2 - MinCo2);
                double? TemporalBoilerCost = (Boilers[i].ProductionCost - MinCostValue) / (MaxCostValue - MinCostValue);
                objective = ((TemporalBoilerCost ?? 0.0) * par[0] + (TemporalConsumption ?? 0.0) * par[1] + (TemporalCo2 ?? 0.0) * par[2]) / n;
                obj[Boilers[i]] = objective;
                //Console.WriteLine($"Boiler {Boilers[i].Name} objective value: {objective}");
            }
        }
        //obj is a dictionary that has the objective value as a double and the unit as an AssetSpecifications object
        return obj;
    }

    //This method distributes the heat demand across the unit depending on their objective values (profitability)
    public void CalculateUnits(List<AssetSpecifications> Boilers, Dictionary<AssetSpecifications, double> boilerdict, double? heat, DateTime hour)
    {
        //This orders the objective values associated with each unit
        double[] order = boilerdict.Values.OrderBy(o => o).Distinct().ToArray();

        //Heat demand the user sets in the GUI
        double? heatneeded = heat;

        //Foreach value in objective values
        for (int i = 0; i < order.Count(); i++)
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
                    //Checks if the heat needed is more than the Max produced heat of the boiler
                    if (heatneeded > Boilers[indexes[j]].MaxHeat)
                    {
                        Boilers[indexes[j]].ProducedHeat[hour] = Boilers[indexes[j]].MaxHeat;
                        heatneeded -= Boilers[indexes[j]].MaxHeat;
                        Console.WriteLine($"The Boiler:  {Boilers[indexes[j]].Name} produces {Boilers[indexes[j]].MaxHeat}");
                    }
                    else //If not, it means that this boiler can be assigned with the remaining heat and set it to 0
                    {
                        Boilers[indexes[j]].ProducedHeat[hour] = heatneeded;
                        heatneeded = 0;
                    }
                }
            }
        }
    }

    //This method calculates the final impact of electricity in the total costs.
    public double? CalculateElectricity(List<AssetSpecifications> boilers, Dictionary<DateTime, double?> ElectricityPrice)
    {
        //Get every boiler, pump and motor and store them in separate lists as AssetSpecifications objects
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        List<AssetSpecifications> HeatPumps = Boilers.Where(n => n.UnitType == "Heat Pump").ToList();
        List<AssetSpecifications> GasMotors = Boilers.Where(n => n.UnitType == "Motor").ToList();

        double? MotorBenefit = 0;
        double? PumpsCost = 0;
        //Calculates for each gas motor the money of electricity produced
        foreach (var motor in GasMotors)
        {
            //This is because the motor may not have a 1 to 1 heat/electricity production
            double? ratio = 0;

            //Preventing divisions by 0 and null values sneaking in
            if (motor.MaxElectricity != null && motor.MaxElectricity != 0)
            {
                //This ratio is the ratio in which the electricity is produced in comparison with the heat generation
                ratio = motor.MaxHeat / (double)Math.Abs((decimal)motor.MaxElectricity);
                //The absolute value was added in case the user wants the motor to consume electricity instead of generate it
            }
            foreach (KeyValuePair<DateTime, double?> price in ElectricityPrice)
            {
                //This condition is triggered only if any engine has generated heat
                if (motor.ProducedHeat.ContainsKey(price.Key))
                {
                    //The electricity remaining from motors can be sold, here it's added to the benefits
                    MotorBenefit += motor.ProducedHeat[price.Key] * ratio * price.Value;
                }
            }
        }

        //Calculates for each heat pump the money of electricity spent
        foreach (var pump in HeatPumps)
        {
            //This is because the pump may not have a 1 to 1 heat/electricity production
            double? ratio = 0;

            //Preventing divisions by 0 and null values sneaking in
            if (pump.MaxElectricity != null && pump.MaxElectricity != 0)
            {
                //This ratio is the ratio in which the electricity is produced in comparison with the heat generation
                ratio = pump.MaxHeat / (double)Math.Abs((decimal)pump.MaxElectricity);
                //Pumps do not produce electricity, so the max electricity is negative, we have to use absolute values
            }
            foreach (KeyValuePair<DateTime, double?> price in ElectricityPrice)
            {
                if (pump.ProducedHeat.ContainsKey(price.Key))
                {
                    //The pumps use electricity to generate heat, so electricity prices have to added to the final costs
                    PumpsCost += pump.ProducedHeat[price.Key] * ratio * price.Value;
                }
            }
        }
        //Cost - benefit to know whether if you end up winning or losing money in electricity generation/consumption
        return PumpsCost - MotorBenefit;
    }

    public void OptimizationAlgorithm(List<AssetSpecifications> boilers, int[] par, double? ElectricityPrice, double? heat, DateTime time)
    {
        //All the optimization methods should go here, this way we only have to call one sigle method in the view model
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        List<AssetSpecifications> HeatPumps = Boilers.Where(n => n.UnitType == "Heat Pump").ToList();
        List<AssetSpecifications> GasMotors = Boilers.Where(n => n.UnitType == "Motor").ToList();
        CalculateUnits(Boilers, GetObjective(Boilers, par, ElectricityPrice, HeatPumps, GasMotors), heat, time);
    }

    //This is the final calculation after the electricity profit/losses are calculated and returns the final cost
    public double? CalculateTotalCost(List<AssetSpecifications> boilers, double? Electricity)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        double? TotalCost = 0;
        foreach (var boiler in Boilers)
        {
            //For every bit of heat produced by boilers, multiply by the production costs
            TotalCost += boiler.ProductionCost * boiler.ProducedHeat.Values.Sum();
        }

        //Add the total costs to the electricity prices balance
        return TotalCost + Electricity;
    }

    //This is exactly the same but for CO2 emissions
    public double? CalculateTotalCO2(List<AssetSpecifications> boilers)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        double? TotalCO2 = 0;
        foreach (AssetSpecifications unit in Boilers)
        {
            //Null checker for heat pumps that do not have CO2 emissions
            unit.CO2Emissions ??= 0;
            //For every bit of heat produced by every unit, multiply by the CO2 emissions ratio
            TotalCO2 += unit.CO2Emissions * unit.ProducedHeat.Values.Sum();
        }

        return TotalCO2;
    }

    //This method calculates all the fuel used in the optimization
    public double? CalculateTotalFuel(List<AssetSpecifications> boilers)
    {
        List<AssetSpecifications> Boilers = boilers.Where(n => n.IsActive == true).ToList();
        double? TotalFuel = 0;
        foreach (AssetSpecifications unit in Boilers)
        {
            unit.FuelConsumption ??= 0;
            TotalFuel += unit.FuelConsumption * unit.ProducedHeat.Values.Sum();
        }

        return TotalFuel;
    }
}