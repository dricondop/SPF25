using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HeatProductionOptimization.Models.DataModels;

public class AssetSpecification
{
    [JsonIgnore]
    public Dictionary<string, AssetSpecification> _assets;

    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("ID")]
    public string ID { get; set; }

    [JsonPropertyName("Unit Type")]
    public string UnitType { get; set; }

    [JsonPropertyName("Max Heat (MW)")]
    public double MaxHeat { get; set; }

    [JsonPropertyName("Max Electricity (MW)")]
    public double? MaxElectricity { get; set; }

    [JsonPropertyName("Production Cost (DKK/MWh)")]
    public double ProductionCost { get; set; }

    [JsonPropertyName("CO2 Emissions (kg/MWh)")]
    public double? CO2Emissions { get; set; }

    [JsonPropertyName("Fuel Type")]
    public string FuelType { get; set; }

    [JsonPropertyName("Fuel Consumption (MWh fuel/MWh heat)")]
    public double? FuelConsumption { get; set; }
}
