using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Globalization;
using HeatProductionOptimization.Models;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using Avalonia.Controls;
using ReactiveUI;
using System.Diagnostics;

namespace HeatProductionOptimization.ViewModels;

public class AssetManagerViewModel : ViewModelBase
{
    private AssetManager _assetManager;
    private ObservableCollection<AssetSpecifications> _assets;
    private string _statusMessage = "Do not forget to save any changes :)";
    private string _currentFilePath;
    private string _selectedUnitType = "Boiler"; // Default unit type
    private ComboBoxItem _selectedUnitTypeItem;

    public AssetManagerViewModel()
    {
        // Initialize AssetManager with null to use the default path
        _assetManager = new AssetManager();
        _currentFilePath = _assetManager.GetFilePath();
        LoadAssets();
    }

    public ObservableCollection<AssetSpecifications> Assets
    {
        get => _assets;
        set => this.RaiseAndSetIfChanged(ref _assets, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    public string SelectedUnitType
    {
        get => _selectedUnitType;
        set => this.RaiseAndSetIfChanged(ref _selectedUnitType, value);
    }
    
    public ComboBoxItem SelectedUnitTypeItem
    {
        get => _selectedUnitTypeItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUnitTypeItem, value);
            if (value != null && value.Content is string content)
            {
                SelectedUnitType = content;
            }
        }
    }

    private void LoadAssets()
    {
        var assetDict = _assetManager.GetAllAssets();
        Assets = new ObservableCollection<AssetSpecifications>(assetDict.Values);
    }
    
    public void AddNewUnit()
    {
        try
        {
            // Extract the actual unit type text from the ComboBoxItem if needed
            string unitType = _selectedUnitType;
            
            // Create new unit with the selected type
            var newUnit = _assetManager.CreateNewUnit(unitType);
            
            // Add to observable collection
            Assets.Add(newUnit);
            
            StatusMessage = $"New {unitType} unit added.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding new unit: {ex.Message}";
            Console.WriteLine($"Error adding new unit: {ex}");
        }
    }
    
    public bool ValidateAssets()
    {
        if (Assets == null || Assets.Count == 0)
        {
            StatusMessage = "No assets to validate.";
            return false;
        }

        foreach (var asset in Assets)
        {
            // Validate required string fields
            if (string.IsNullOrWhiteSpace(asset.Name))
            {
                StatusMessage = $"Asset ID {asset.ID}: Name cannot be empty.";
                return false;
            }

            // Unit Type validation
            if (string.IsNullOrWhiteSpace(asset.UnitType))
            {
                StatusMessage = $"Asset {asset.Name}: Unit Type cannot be empty.";
                return false;
            }

            // Fuel Type validation for Boiler and Motor
            if ((asset.UnitType == "Boiler" || asset.UnitType == "Motor") && 
                string.IsNullOrWhiteSpace(asset.FuelType))
            {
                StatusMessage = $"Asset {asset.Name}: Fuel Type is required for {asset.UnitType}.";
                return false;
            }

            // Validate Fuel Type for Boilers specifically
            if (asset.UnitType == "Boiler" && 
                (string.IsNullOrWhiteSpace(asset.FuelType) || 
                (asset.FuelType != "Gas" && asset.FuelType != "Oil")))
            {
                StatusMessage = $"Asset {asset.Name}: Fuel Type for Boiler must be either 'Gas' or 'Oil'.";
                return false;
            }

            // Numeric field validations
            if (asset.MaxHeat.HasValue && asset.MaxHeat <= 0)
            {
                StatusMessage = $"Asset {asset.Name}: Max Heat must be positive.";
                return false;
            }

            if (asset.ProductionCost.HasValue && asset.ProductionCost < 0)
            {
                StatusMessage = $"Asset {asset.Name}: Production Cost cannot be negative.";
                return false;
            }

            if (asset.FuelConsumption.HasValue && asset.FuelConsumption <= 0)
            {
                StatusMessage = $"Asset {asset.Name}: Fuel Consumption must be positive.";
                return false;
            }
        }

        return true;
    }

    public bool TryParseNumericField(string fieldName, string value, out object? result)
    {
        result = null;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            StatusMessage = $"{fieldName} cannot be empty.";
            return false;
        }

        // Different parsing logic based on field type
        switch (fieldName)
        {
            case "MaxHeat":
            case "MaxElectricity":
            case "ProductionCost":
            case "CO2Emissions":
            case "FuelConsumption":
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleResult))
                {
                    result = doubleResult;
                    return true;
                }
                else if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out doubleResult))
                {
                    // Also try with current culture (which might use comma as decimal separator)
                    result = doubleResult;
                    return true;
                }
                StatusMessage = $"Invalid number format for {fieldName}.";
                return false;
                
            default:
                // For string fields
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result = value;
                    return true;
                }
                StatusMessage = $"{fieldName} cannot be empty.";
                return false;
        }
    }
    
    public void SaveChanges()
    {
        try
        {
            if (Assets == null || Assets.Count == 0)
            {
                StatusMessage = "No assets to save.";
                return;
            }
            
            // Validate assets before saving
            if (!ValidateAssets())
            {
                return;
            }
            
            bool result = _assetManager.SaveAssets(Assets);
            
            if (result)
            {
                StatusMessage = "Changes saved successfully!";
            }
            else
            {
                StatusMessage = "Failed to save changes. Check the console for details.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Console.WriteLine($"Save error: {ex}");
        }
    }

    public string CurrentFilePath 
    { 
        get => _currentFilePath; 
        private set => this.RaiseAndSetIfChanged(ref _currentFilePath, value); 
    }
    
    public void LoadFromFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                StatusMessage = "Invalid file path specified.";
                return;
            }
            
            _assetManager = new AssetManager(filePath);
            
            CurrentFilePath = filePath;
            
            LoadAssets();
            
            StatusMessage = $"Successfully loaded assets from: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
            Console.WriteLine($"Error loading file: {ex}");
        }
    }
}
