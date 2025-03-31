using System;
using System.Collections.ObjectModel;
using System.Linq;
using HeatProductionOptimization.Models;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using Avalonia.Controls;
using ReactiveUI;

namespace HeatProductionOptimization.ViewModels;

public class AssetManagerViewModel : ViewModelBase
{
    private readonly AssetManager _assetManager;
    private ObservableCollection<AssetSpecification> _assets;
    private string _statusMessage = "Do not forget to save any changes :)";

    public AssetManagerViewModel()
    {
        _assetManager = new AssetManager();
        LoadAssets();
    }

    public ObservableCollection<AssetSpecification> Assets
    {
        get => _assets;
        set => this.RaiseAndSetIfChanged(ref _assets, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    private void LoadAssets()
    {
        var assetDict = _assetManager.GetAllAssets();
        Assets = new ObservableCollection<AssetSpecification>(assetDict.Values);
    }
    
    public void AddNewUnit()
    {
        try
        {
            var newUnit = _assetManager.CreateNewUnit();
            
            Assets.Add(newUnit);
            
            StatusMessage = "New unit added.";
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
}
