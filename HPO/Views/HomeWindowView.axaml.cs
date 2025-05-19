using Avalonia.Controls;
using Avalonia.Interactivity;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.ViewModels;
using System;
using System.Linq;

namespace HeatProductionOptimization.Views;

public partial class HomeWindowView : UserControl
{
    private readonly AssetManager _assetManager;

    public HomeWindowView()
    {
        InitializeComponent();
        _assetManager = OptimizerViewModel.SharedAssetManager;
    }
    
    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is StackPanel stackPanel)
        {
            var textBlock = stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
            var buttonText = textBlock?.Text ?? "";

            if (buttonText.Contains("Scenario 1"))
            {
                ConfigureScenario1();
            }
            else if (buttonText.Contains("Scenario 2"))
            {
                ConfigureScenario2();
            }
        }

        SaveAssetChanges(); 
        UpdateMaxHeat();
        WindowManager.TriggerAssetManagerWindow();
    }

    private void ConfigureScenario1()
    {
            var assets = _assetManager.GetAllAssets();
            if (assets == null || assets.Count == 0)
                return;
                
            // First deactivate all assets
            foreach (var asset in assets.Values)
            {
                asset.IsActive = false;
            }
            
            // Find and activate 2 gas boilers
            int gasBoilersActivated = 0;
            foreach (var asset in assets.Values)
            {
                if (asset.UnitType == "Boiler" && asset.FuelType == "Gas" && gasBoilersActivated < 2)
                {
                    asset.IsActive = true;
                    gasBoilersActivated++;
                }
            }
            
            // Find and activate 1 oil boiler
            int oilBoilersActivated = 0;
            foreach (var asset in assets.Values)
            {
                if (asset.UnitType == "Boiler" && asset.FuelType == "Oil" && oilBoilersActivated < 1)
                {
                    asset.IsActive = true;
                    oilBoilersActivated++;
                }
            }
            
            Console.WriteLine($"Scenario 1 loaded: {gasBoilersActivated} gas boilers, {oilBoilersActivated} oil boiler");
    }

    private void ConfigureScenario2()
    {
            // Scenario 2: Single heating area, one gas boiler, one oil boiler, one gas motor, one heat pump
            var assets = _assetManager.GetAllAssets();
            if (assets == null || assets.Count == 0)
                return;
                
            // First deactivate all assets
            foreach (var asset in assets.Values)
            {
                asset.IsActive = false;
            }
            
            // Find and activate 1 of each required asset type
            int gasBoilersActivated = 0;
            int oilBoilersActivated = 0;
            int gasMotorsActivated = 0;
            int heatPumpsActivated = 0;
            
            foreach (var asset in assets.Values)
            {
                if (asset.UnitType == "Boiler" && asset.FuelType == "Gas" && gasBoilersActivated < 1)
                {
                    asset.IsActive = true;
                    gasBoilersActivated++;
                }
                else if (asset.UnitType == "Boiler" && asset.FuelType == "Oil" && oilBoilersActivated < 1)
                {
                    asset.IsActive = true;
                    oilBoilersActivated++;
                }
                else if (asset.UnitType == "Motor" && asset.FuelType == "Gas" && gasMotorsActivated < 1)
                {
                    asset.IsActive = true;
                    gasMotorsActivated++;
                }
                else if (asset.UnitType == "Heat Pump" && heatPumpsActivated < 1)
                {
                    asset.IsActive = true;
                    heatPumpsActivated++;
                }
            }
            
            Console.WriteLine($"Scenario 2 loaded: {gasBoilersActivated} gas boiler, {oilBoilersActivated} oil boiler, " +
                            $"{gasMotorsActivated} gas motor, {heatPumpsActivated} heat pump");
    }
    
    private void UpdateMaxHeat()
    {
        try
        {
            var assets = _assetManager.GetAllAssets();
            if (assets == null || assets.Count == 0)
                return;
                
            AssetManagerViewModel.MaxHeat = assets.Values
                .Where(a => a.IsActive)
                .Sum(a => a.MaxHeat);
                
            Console.WriteLine($"Updated MaxHeat: {AssetManagerViewModel.MaxHeat}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating MaxHeat: {ex.Message}");
        }
    }
    
    private void SaveAssetChanges()
    {
        try
        {
            var assets = _assetManager.GetAllAssets();
            if (assets != null && assets.Count > 0)
            {
                _assetManager.SaveAssets(assets.Values);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving asset changes: {ex.Message}");
        }
    }
}
