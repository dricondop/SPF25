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
        _assetsFilePath = Path.GetFullPath(assetsFilePath);
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

    public Dictionary<string, AssetSpecification> GetAllAssets()
    {
        return _assets;
    }

    public AssetSpecification GetAssetSpecification(string name)
    {
        return _assets.TryGetValue(name, out var spec) ? spec : null;
    }
    
    public string GetFilePath()
    {
        return _assetsFilePath;
    }

    public bool SaveAssets(IEnumerable<AssetSpecification> assets)
    {
        try
        {
            var jsonArray = new List<Dictionary<string, AssetSpecification>>();
            
            foreach (var asset in assets)
            {
                if (string.IsNullOrEmpty(asset.ID))
                {
                    Console.WriteLine("Warning: Skipping asset with null or empty ID");
                    continue;
                }
                
                var assetDict = new Dictionary<string, AssetSpecification>
                {
                    { asset.Name, asset }
                };
                jsonArray.Add(assetDict);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            
            string json = JsonSerializer.Serialize(jsonArray, options);
            
            string directory = Path.GetDirectoryName(_assetsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(_assetsFilePath, json);
            
            UpdateAssetDictionary(assets);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving assets to JSON: {ex.Message}");
            return false;
        }
    }

        private void UpdateAssetDictionary(IEnumerable<AssetSpecification> assets)
    {
        var newAssets = new Dictionary<string, AssetSpecification>();
        
        foreach (var asset in assets)
        {
            if (!string.IsNullOrEmpty(asset.Name))
            {
                newAssets[asset.Name] = asset;
            }
        }
        
        _assets = newAssets;
    }

    public AssetSpecification CreateNewUnit()
    {
        string defaultUnitType = "Boiler";
        
        string newId = GenerateUniqueId(defaultUnitType);
        
        var newUnit = new AssetSpecification
        {
            Name = newId,
            ID = newId,
            UnitType = defaultUnitType,
            IsActive = true,
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
        string[] words = unitType.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        string prefix = words.Length > 0 ? words[0].Substring(0, 1).ToUpper() : "U";
        if (words.Length > 1)
        {
            prefix += words[1].Substring(0, 1).ToUpper();
        }
        
        int count = 1;
        
        foreach (var asset in _assets.Values)
        {
            if (asset.ID != null && asset.ID.Length >= 2)
            {
                string assetPrefix = asset.ID.Substring(0, Math.Min(2, asset.ID.Length));
                if (assetPrefix == prefix)
                {
                    count++;
                }
            }
        }
        
        string newId = $"{prefix}{count}";
        
        while (_assets.ContainsKey(newId))
        {
            count++;
            newId = $"{prefix}{count}";
        }
        
        return newId;
    }
}
