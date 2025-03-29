using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using HeatProductionOptimization.Models;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using Avalonia.Controls;

namespace HeatProductionOptimization.ViewModels;

public class AssetManagerViewModel : ViewModelBase
{
    private readonly AssetManager _assetManager;
    private ObservableCollection<AssetSpecification> _assets;
    private string _statusMessage;

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
    
    //public ICommand SaveChangesCommand { get; }
    
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
    
    /*private void SaveChanges()
    {
        try
        {
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
        }
    }*/
}
