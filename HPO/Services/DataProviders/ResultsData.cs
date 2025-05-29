// HPO/Services/DataProviders/ResultsData.cs
using System;
using System.Collections.Generic;

namespace HeatProductionOptimization.Services.DataProviders
{
    public class ResultsData
    {
        public List<DateTime> TimeStamps { get; set; } = new List<DateTime>();
        public List<double> HeatDemand { get; set; } = new List<double>();
        public List<double> ElectricityPrice { get; set; } = new List<double>();
        public List<Dictionary<string, double>> ProductionData { get; set; } = new List<Dictionary<string, double>>();
        public List<double> TotalCosts { get; set; } = new List<double>();
        public List<double> TotalEmissions { get; set; } = new List<double>();

        public void AddDataPoint(
            DateTime timeStamp, 
            double heatDemand, 
            double electricityPrice, 
            Dictionary<string, double> production, 
            double totalCost, 
            double totalEmission)
        {
            TimeStamps.Add(timeStamp);
            HeatDemand.Add(heatDemand);
            ElectricityPrice.Add(electricityPrice);
            ProductionData.Add(production);
            TotalCosts.Add(totalCost);
            TotalEmissions.Add(totalEmission);
        }

        public void Clear()
        {
            TimeStamps.Clear();
            HeatDemand.Clear();
            ElectricityPrice.Clear();
            ProductionData.Clear();
            TotalCosts.Clear();
            TotalEmissions.Clear();
        }
    }
}