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
using CommunityToolkit.Mvvm.Input;

namespace HeatProductionOptimization.ViewModels;

public partial class AssetManagerViewModel : ViewModelBase
{
    private AssetManager _assetManager;
    private ObservableCollection<AssetSpecifications>? _assets;
    private string _statusMessage = "Do not forget to save any changes :)";
    private string _currentFilePath;
    private string _selectedUnitType = "Boiler";
    private ComboBoxItem? _selectedUnitTypeItem;
    public static double? MaxHeat = 0;
    public bool HasAssets => Assets?.Count > 0;

    public AssetManagerViewModel()
    {
        _assetManager = OptimizerViewModel.SharedAssetManager;
        _currentFilePath = _assetManager.GetFilePath();
        LoadAssets();
        UpdateMaxHeat();
    }

    public void ReloadAssets()
    {
        try
        {
            var assetDict = _assetManager.LoadAssetsSpecifications();

            Assets = new ObservableCollection<AssetSpecifications>(assetDict.Values);

            UpdateMaxHeat();

            StatusMessage = $"Assets refreshed from file. {Assets.Count(a => a.IsActive)} active units.";

            Console.WriteLine($"Assets reloaded from file. {Assets.Count(a => a.IsActive)} active units out of {Assets.Count} total.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing assets: {ex.Message}";
            Console.WriteLine($"Error refreshing assets: {ex}");
        }
    }

    public ObservableCollection<AssetSpecifications>? Assets
    {
        get => _assets;
        set
        {
            this.RaiseAndSetIfChanged(ref _assets, value);
            this.RaisePropertyChanged(nameof(HasAssets)); 
        }
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

    public ComboBoxItem? SelectedUnitTypeItem
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
            string unitType = _selectedUnitType;

            var newUnit = _assetManager.CreateNewUnit(unitType);

            Assets?.Add(newUnit);

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

    [RelayCommand]
    public void RemoveAsset(AssetSpecifications asset)
    {
        if (_assets != null)
        {
            _assets.Remove(asset);
        }
        else
        {
            Console.WriteLine("No asset to remove!");
        }
    }
    
    public bool TryParseNumericField(string fieldName, string value, out object? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            StatusMessage = $"{fieldName} cannot be empty.";
            return false;
        }

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
                    result = doubleResult;
                    return true;
                }
                StatusMessage = $"Invalid number format for {fieldName}.";
                return false;

            default:
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

            if (!ValidateAssets())
            {
                return;
            }

            bool result = _assetManager.SaveAssets(Assets);
            UpdateMaxHeat();
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

    public void UpdateMaxHeat()
    {
        MaxHeat = Assets?.Where(m => m.IsActive).Select(m => m.MaxHeat).Sum();
        Console.WriteLine($"Max Heat: {MaxHeat}");
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

            _assetManager = new AssetManager();

            CurrentFilePath = _assetManager._assetsFilePath;

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