using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        _currentFilePath = Path.GetFullPath("Resources/Data/Production_Units.json");
        _assetManager = new AssetManager(_currentFilePath);
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
    
    public void SaveChanges()
    {
        try
        {
            if (Assets == null || Assets.Count == 0)
            {
                StatusMessage = "No assets to save.";
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
