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
    private Dictionary<int, AssetSpecifications> _assets; // Use int keys
    private int _nextAvailableId = 1;

    public AssetManager(string assetsFilePath = "Resources/Data/Production_Units.json")
    {
        _assetsFilePath = Path.GetFullPath(assetsFilePath);
        _assets = LoadAssetsSpecifications();
    }

    public Dictionary<int, AssetSpecifications> LoadAssetsSpecifications()
    {
        var assets = new Dictionary<int, AssetSpecifications>();
        _nextAvailableId = 1; // Initialize counter

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
                        if (!int.TryParse(kvp.Value.ID, out assetId))
                        {
                            assetId = _nextAvailableId++;
                        }
                        else
                        {
                            // Update next ID to be greater than any existing
                            _nextAvailableId = Math.Max(_nextAvailableId, assetId + 1);
                        }

                        // Set ID and Name properties
                        kvp.Value.ID = assetId.ToString();
                        
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

    public AssetSpecifications GetAssetSpecifications(int id)
    {
        return _assets.TryGetValue(id, out var spec) ? spec : null;
    }
    
    public string GetFilePath()
    {
        return _assetsFilePath;
    }

    public bool SaveAssets(IEnumerable<AssetSpecifications> assets)
    {
        try
        {
            var jsonArray = new List<Dictionary<string, AssetSpecifications>>();
            
            foreach (var asset in assets)
            {
                if (string.IsNullOrEmpty(asset.ID))
                {
                    Console.WriteLine("Warning: Skipping asset with null or empty ID");
                    continue;
                }
                
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

    private void UpdateAssetDictionary(IEnumerable<AssetSpecifications> assets)
    {
        var newAssets = new Dictionary<int, AssetSpecifications>();
        
        foreach (var asset in assets)
        {
            if (!string.IsNullOrEmpty(asset.ID))
            {
                // Try to parse ID as integer
                if (int.TryParse(asset.ID, out int id))
                {
                    newAssets[id] = asset;
                    // Update next available ID
                    _nextAvailableId = Math.Max(_nextAvailableId, id + 1);
                }
                else
                {
                    // If parsing fails, assign a new ID
                    int newId = _nextAvailableId++;
                    asset.ID = newId.ToString();
                    newAssets[newId] = asset;
                }
            }
            else
            {
                // If ID is null/empty, assign a new ID
                int newId = _nextAvailableId++;
                asset.ID = newId.ToString();
                newAssets[newId] = asset;
            }
        }
        
        _assets = newAssets;
    }

    public AssetSpecifications CreateNewUnit(string unitType = "Boiler")
    {
        int newId = _nextAvailableId++;
        string idString = newId.ToString();
        
        // Create the appropriate template based on unit type
        AssetSpecifications newUnit;
        
        switch (unitType?.Trim())
        {
            case "Motor":
                newUnit = AssetSpecifications.CreateMotor(idString);
                break;
            case "Heat Pump":
                newUnit = AssetSpecifications.CreateHeatPump(idString);
                break;
            case "Boiler":
            default:
                newUnit = AssetSpecifications.CreateBoiler(idString);
                break;
        }
        
        // Add to assets dictionary with integer key
        _assets[newId] = newUnit;
        return newUnit;
    }
}
