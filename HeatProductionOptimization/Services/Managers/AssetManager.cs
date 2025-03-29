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
            
            // Parse the array of dictionaries and merge them into a single dictionary
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

    /*public bool SaveAssets(IEnumerable<AssetSpecification> assets)
    {
        try
        {
            // Transform the assets back to the required JSON format
            var jsonArray = new List<Dictionary<string, AssetSpecification>>();
            
            foreach (var asset in assets)
            {
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
            var json = JsonSerializer.Serialize(jsonArray, options);
            File.WriteAllText(_assetsFilePath, json);
            
            // Update the internal dictionary
            _assets = assets.ToDictionary(a => a.ID);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving assets to JSON: {ex.Message}");
            return false;
        }
    }*/
}
