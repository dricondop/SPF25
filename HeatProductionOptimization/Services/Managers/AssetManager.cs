using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Services.Managers;

public class AssetManager
{
    private readonly string _assetsFilePath;
    private Dictionary<string, AssetSpecification> _assets;

    public AssetManager(string assetsFilePath = "Resources/Data/Production_Units.json")
    {
        _assetsFilePath = assetsFilePath;
        _assets = LoadAssetsSpecifications();
    }

    public Dictionary<string, AssetSpecification> LoadAssetsSpecifications()
    {
        var assets = new Dictionary<string, AssetSpecification>();

        if (!File.Exists(_assetsFilePath))
        {
            Console.WriteLine($"Error: file not found at path: {_assetsFilePath}");
            return assets;
        }

        try
        {
            var json = File.ReadAllText(_assetsFilePath);
            
            var jsonArray = JsonSerializer.Deserialize<List<Dictionary<string, AssetSpecification>>>(json);
            if (jsonArray != null)
            {
                foreach (var dict in jsonArray)
                {
                    foreach (var kvp in dict)
                    {
                        // Set ID and Name properties
                        kvp.Value.ID = kvp.Key;
                        kvp.Value.Name = kvp.Key;
                        assets[kvp.Key] = kvp.Value;
                    }
                }
            }
            return assets;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading Json: {ex.Message}");
        }

        return assets;
    }

    public AssetSpecification GetAssetSpecification(string name)
    {
        return _assets.TryGetValue(name, out var spec) ? spec : null;
    }

    public Dictionary<string, AssetSpecification> GetAllAssets()
    {
        return _assets;
    }
    
    public bool SaveAssets(IEnumerable<AssetSpecification> assets)
    {
        try
        {
            // Transform the assets back to the required JSON format
            var jsonArray = new List<Dictionary<string, AssetSpecification>>();
            
            foreach (var asset in assets)
            {
                // Skip assets with null or empty ID
                if (string.IsNullOrEmpty(asset.ID))
                {
                    Console.WriteLine("Warning: Skipping asset with null or empty ID");
                    continue;
                }
                
                // Ensure Name matches ID for consistency
                asset.Name = asset.ID;
                
                // Create a dictionary entry for this asset
                var assetDict = new Dictionary<string, AssetSpecification>
                {
                    { asset.ID, asset }
                };
                jsonArray.Add(assetDict);
            }
            
            // Define serialization options
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            
            // Serialize to JSON and write to file
            string json = JsonSerializer.Serialize(jsonArray, options);
            
            // Make sure directory exists
            string directory = Path.GetDirectoryName(_assetsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(_assetsFilePath, json);
            
            _assets = assets.ToDictionary(a => a.ID);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving assets to JSON: {ex.Message}");
            return false;
        }
    }

    public AssetSpecification CreateNewUnit()
    {
        string defaultUnitType = "Boiler";
        
        string newId = GenerateUniqueId(defaultUnitType);
        
        var newUnit = new AssetSpecification
        {
            ID = newId,
            Name = newId,
            UnitType = defaultUnitType,
            MaxHeat = 1.0,
            ProductionCost = 1.0,
            CO2Emissions = 1.0,
            FuelType = "Fuel Type",
            FuelConsumption = 1.0
        };
        
        _assets[newId] = newUnit;
        
        return newUnit;
    }
    
    private string GenerateUniqueId(string unitType)
    {
        // Split the unit type into words
        string[] words = unitType.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Generate the prefix: first char of first word + (first char of second word if exists)
        string prefix = words.Length > 0 ? words[0].Substring(0, 1).ToUpper() : "U";
        if (words.Length > 1)
        {
            prefix += words[1].Substring(0, 1).ToUpper();
        }
        
        // Count the existing units of this type
        int count = 1;
        foreach (var asset in _assets.Values)
        {
            if (asset.UnitType.Equals(unitType, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }
        
        // Generate the new ID
        string newId = $"{prefix}{count}";
        
        // Make sure the ID is unique
        while (_assets.ContainsKey(newId))
        {
            count++;
            newId = $"{prefix}{count}";
        }
        
        return newId;
    }
}
