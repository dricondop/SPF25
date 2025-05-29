using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HeatProductionOptimization.Models.DataModels;

public class AssetSpecifications
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ID")]
    public int ID { get; set; } 
    
    [JsonPropertyName("IsActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("Unit Type")]
    public string UnitType { get; set; } = string.Empty;

    [JsonPropertyName("Max Heat (MW)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MaxHeat { get; set; }

    [JsonPropertyName("Max Electricity (MW)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MaxElectricity { get; set; }

    [JsonPropertyName("Production Cost (DKK/MWh)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ProductionCost { get; set; }

    [JsonPropertyName("CO2 Emissions (kg/MWh)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CO2Emissions { get; set; }

    [JsonPropertyName("Fuel Type")]
    public string FuelType { get; set; } = string.Empty;

    [JsonPropertyName("Fuel Consumption (MWh fuel/MWh heat)")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FuelConsumption { get; set; }


    [JsonIgnore]
    public Dictionary<DateTime, double?> ProducedHeat = [];

    public static AssetSpecifications CreateBoiler(int id, string? name = null)
    {
        return new AssetSpecifications
        {
            Name = name ?? $"Boiler {id}",
            ID = id,
            UnitType = "Boiler",
            IsActive = true,
            MaxHeat = 4.0,
            ProductionCost = 500.0,
            CO2Emissions = 150.0,
            FuelType = "Gas",
            FuelConsumption = 0.9,
            ProducedHeat = new Dictionary<DateTime, double?> { { DateTime.MinValue, 0.0 } }
        };
    }

    public static AssetSpecifications CreateMotor(int id, string? name = null)
    {
        return new AssetSpecifications
        {
            Name = name ?? $"Motor {id}",
            ID = id,
            UnitType = "Motor",
            IsActive = true,
            MaxHeat = 3.5,
            ProductionCost = 800.0,
            CO2Emissions = 400.0,
            FuelType = "Gas",
            FuelConsumption = 1.8,
            ProducedHeat = new Dictionary<DateTime, double?> { { DateTime.MinValue, 0.0 } }
        };
    }

    public static AssetSpecifications CreateHeatPump(int id, string? name = null)
    {
        return new AssetSpecifications
        {
            Name = name ?? $"Heat Pump {id}",
            ID = id,
            UnitType = "Heat Pump",
            IsActive = true,
            MaxHeat = 6.0,
            MaxElectricity = -6.0,
            ProductionCost = 60.0,
            ProducedHeat = new Dictionary<DateTime, double?> { { DateTime.MinValue, 0.0 } }
        };
    }

    public static AssetSpecifications CreateForType(string unitType, int id, string? name = null)
    {
        switch (unitType?.Trim())
        {
            case "Boiler":
                return CreateBoiler(id, name);
            case "Motor":
                return CreateMotor(id, name);
            case "Heat Pump":
                return CreateHeatPump(id, name);
            default:
                return CreateBoiler(id, name);
        }
    }
}
