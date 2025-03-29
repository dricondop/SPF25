using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Services.Managers;

public class AssetManager
{
    private readonly string _productionUnitsFilePath;
    private Dictionary<string, AssetSpecification> _productionUnits;

    public AssetManager(string productionUnitsFilePath = "Resources/Data/Product_Units.json")
    {
        _productionUnitsFilePath = productionUnitsFilePath;
        _productionUnits = LoadAssetSpecifications();
    }

    public Dictionary<string, AssetSpecification> LoadAssetSpecifications()
    {
        var assets = new Dictionary<string, AssetSpecification>();

        if (!File.Exists(_productionUnitsFilePath))
        {
            Console.WriteLine($"Error: file not found at path: {_productionUnitsFilePath}");
            return assets;
        }

        try
        {
            var json = File.ReadAllText(_productionUnitsFilePath);

            var asset_dictionary = JsonSerializer.Deserialize<Dictionary<string, AssetSpecification>>(json);
            return asset_dictionary;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading Json: {ex.Message}");
        }

        return assets;
    }

    //We would have to set the value in other place by creatin an instance of this class, 
    //then use the LoadAssetSpecifications() to set the _productionUnits and then use the GetAssetSpecification() method
    public AssetSpecification GetAssetSpecification(string name)
    {
        return _productionUnits.TryGetValue(name, out var spec) ? spec : null;
    }
}
