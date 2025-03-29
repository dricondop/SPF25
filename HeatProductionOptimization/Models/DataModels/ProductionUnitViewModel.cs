namespace HeatProductionOptimization.Models.DataModels;

public class ProductionUnitViewModel
{
    public string Name { get; set; }
    public string Type { get; set; }
    public double MaxHeat { get; set; }
    public double? MaxElectricity { get; set; }
    public double ProductionCost { get; set; }
    public double? CO2Emissions { get; set; }
    public string FuelType { get; set; }
    public double? FuelConsumption { get; set; }
}
