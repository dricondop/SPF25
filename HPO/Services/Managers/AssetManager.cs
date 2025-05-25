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
    public readonly string _assetsFilePath;
    private Dictionary<int, AssetSpecifications> _assets;
    public static int _nextAvailableId = 1;

    public AssetManager()
    {
        _assetsFilePath = GetFilePath();
        _assets = LoadAssetsSpecifications();
    }

    public Dictionary<int, AssetSpecifications> LoadAssetsSpecifications()
    {
        var assets = new Dictionary<int, AssetSpecifications>();
        _nextAvailableId = 1;

        if (!File.Exists(_assetsFilePath))
        {
            Console.WriteLine($"Error: file not found at path: {_assetsFilePath}");
            return assets;
        }

        try
        {
            var json = File.ReadAllText(_assetsFilePath);

            var jsonArray = JsonSerializer.Deserialize<List<Dictionary<string, AssetSpecifications>>>(json);
            if (jsonArray != null)
            {
                foreach (var dict in jsonArray)
                {
                    foreach (var kvp in dict)
                    {
                        // Convert string ID to int or generate a new one
                        int assetId;
                        assetId = _nextAvailableId++;

                        // Set ID and Name properties
                        kvp.Value.ID = assetId;

                        // If name is empty, use the unit type + ID
                        if (string.IsNullOrEmpty(kvp.Value.Name))
                        {
                            kvp.Value.Name = $"{kvp.Value.UnitType} {assetId}";
                        }

                        assets[assetId] = kvp.Value;
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

    public Dictionary<int, AssetSpecifications> GetAllAssets()
    {
        return _assets;
    }

    public AssetSpecifications? GetAssetSpecifications(int id)
    {
        return _assets.TryGetValue(id, out var spec) ? spec : null;
    }

    public string GetFilePath()
    {
        try
        {
            var locations = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Data", "Production_Units.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Data", "Production_Units.json"),
                Path.Combine(Environment.CurrentDirectory, "Resources", "Data", "Production_Units.json")
            };

            string? FilePath = locations.FirstOrDefault(File.Exists);
            if (FilePath == null)
            {
                Console.WriteLine("No file path found");
                return string.Empty;
            }
            return FilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return string.Empty;
        }
    }

    public bool SaveAssets(IEnumerable<AssetSpecifications> assets)
    {
        try
        {
            var jsonArray = new List<Dictionary<string, AssetSpecifications>>();

            foreach (var asset in assets)
            {

                var assetDict = new Dictionary<string, AssetSpecifications>
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

            string directory = Path.GetDirectoryName(_assetsFilePath) ?? string.Empty;
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
    private void UpdateAssetDictionary(IEnumerable<AssetSpecifications> assets)
    {
        var newAssets = new Dictionary<int, AssetSpecifications>();
        // Reset next available ID based on the maximum existing ID + 1
        // Handle the case where the input list might be empty
        _nextAvailableId = assets.Any() ? assets.Max(a => a.ID) + 1 : 1;

        foreach (var asset in assets)
        {
            // Assuming IDs should be positive. If an asset has an invalid ID (e.g., 0 or less), 
            // or if it somehow duplicates an ID already processed (though Dictionary handles this),
            // we might need a strategy. Here, we trust incoming IDs if they are valid (>0).
            // If an asset comes with ID <= 0, it implies it's new or needs an ID assigned.
            if (asset.ID > 0)
            {
                // Use the existing valid ID
                if (!newAssets.ContainsKey(asset.ID))
                {
                    newAssets[asset.ID] = asset;
                    // Ensure _nextAvailableId is always ahead of the highest seen ID
                    _nextAvailableId = Math.Max(_nextAvailableId, asset.ID + 1);
                }
                else
                {
                    // Handle duplicate ID scenario if necessary (e.g., log a warning)
                    Console.WriteLine($"Warning: Duplicate asset ID {asset.ID} encountered during update. Skipping duplicate.");
                }
            }
            else
            {
                // Assign a new ID if the current one is invalid (0 or less)
                int newId = _nextAvailableId++;
                asset.ID = newId; // Update the asset's ID property
                newAssets[newId] = asset;
            }
        }

        // Replace the old dictionary with the newly constructed one
        _assets = newAssets;
    }


    public AssetSpecifications CreateNewUnit(string unitType = "Boiler")
    {
        int newId = _nextAvailableId++;

        // Create the appropriate template based on unit type
        AssetSpecifications newUnit;

        switch (unitType?.Trim())
        {
            case "Motor":
                newUnit = AssetSpecifications.CreateMotor(newId);
                break;
            case "Heat Pump":
                newUnit = AssetSpecifications.CreateHeatPump(newId);
                break;
            case "Boiler":
            default: //Default to Boiler if type is null, empty, whitespace or unknown
                newUnit = AssetSpecifications.CreateBoiler(newId);
                break;
        }

        // Add to assets dictionary with integer key
        _assets[newId] = newUnit;
        return newUnit;
    }
}
