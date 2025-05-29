using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData.Kernel;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatProductionOptimization.Views;

public partial class HomeWindowView : UserControl
{
    private readonly AssetManager _assetManager;

    public HomeWindowView()
    {
        InitializeComponent();
        // Always use the shared instance for consistency across views
        _assetManager = OptimizerViewModel.SharedAssetManager;
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is StackPanel stackPanel)
        {
            var textBlock = stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBlock != null)
            {
                var buttonText = textBlock.Text;

                if (buttonText != null && buttonText.Contains("Scenario 1"))
                {
                    ConfigureScenario1();
                }
                else if (buttonText != null && buttonText.Contains("Scenario 2"))
                {
                    ConfigureScenario2();
                }
            }
        }

        SaveAssetChanges();
        UpdateMaxHeat();
        WindowManager.TriggerAssetManagerWindow();
    }

    private void ConfigureScenario1()
    {
        try
        {
            var assets = _assetManager.GetAllAssets();
            if (assets == null || assets.Count == 0)
                return;
            
            //if(assets.Values.Select(g => new{prodCost = g.ProductionCost, CO2 = g.CO2Emissions, fuelComp = g.FuelConsumption, fuelType = g.FuelType, maxHeat = g.MaxHeat, maxElec = g.MaxElectricity}))

            
            // Find and activate 2 gas boilers
            // Find and activate 1 oil boiler
            int gasBoiler1Activated = 0;
            int gasBoiler2Activated = 0;
            int oilBoilersActivated = 0;
            foreach (var asset in assets.Values)
            {
                // First deactivate all assets
                asset.IsActive = false;

                //Check if the default Danfoss units of Scenario 1 exist to activate them:
                if (asset.UnitType == "Boiler" && asset.FuelType == "Gas" && asset.ProductionCost == 520 && asset.MaxHeat == 4 && asset.CO2Emissions == 175 && asset.FuelConsumption == 0.9 && gasBoiler1Activated < 1)
                {
                    asset.IsActive = true;
                    gasBoiler1Activated++;
                }
                else if (asset.UnitType == "Boiler" && asset.FuelType == "Gas" && asset.ProductionCost == 560 && asset.MaxHeat == 3 && asset.CO2Emissions == 130 && asset.FuelConsumption == 0.7 && gasBoiler2Activated < 1)
                {
                    asset.IsActive = true;
                    gasBoiler2Activated++;  
                }
                else if (asset.UnitType == "Boiler" && asset.FuelType == "Oil" && asset.ProductionCost == 670 && asset.MaxHeat == 4 && asset.CO2Emissions == 330 && asset.FuelConsumption == 1.5 && oilBoilersActivated < 1)
                {
                    asset.IsActive = true;
                    oilBoilersActivated++;
                }
            }

            //If the default Danfoss units of Scenario 1 do not exist (aka have not been activated up there ↑), create them:
            if (gasBoiler1Activated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Gas Boiler 1", ID = id, IsActive = true, UnitType = "Boiler", MaxHeat = 4, ProductionCost = 520, CO2Emissions = 175, FuelType = "Gas", FuelConsumption = 0.9 });
            }
            else if (gasBoiler2Activated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Gas Boiler 2", ID = id, IsActive = true, UnitType = "Boiler", MaxHeat = 3, ProductionCost = 560, CO2Emissions = 130, FuelType = "Gas", FuelConsumption = 0.7 });
            }
            else if (oilBoilersActivated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Oil Boiler 1", ID = id, IsActive = true, UnitType = "Boiler", MaxHeat = 4, ProductionCost = 670, CO2Emissions = 330, FuelType = "Oil", FuelConsumption = 1.5});
            }

            Console.WriteLine($"Scenario 1 loaded: {gasBoiler1Activated + gasBoiler2Activated} gas boilers, {oilBoilersActivated} oil boiler");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring Scenario 1: {ex.Message}");
        }
    }

    private void ConfigureScenario2()
    {
        try
        {
            // Scenario 2: Single heating area, one gas boiler, one oil boiler, one gas motor, one heat pump
            var assets = _assetManager.GetAllAssets();
            if (assets == null || assets.Count == 0)
                return;

            // Find and activate 1 gas boiler
            // Find and activate 1 oil boiler
            // Find and activate 1 gas motor
            // Find and activate 1 heat pump
            int gasBoilerActivated = 0;
            int oilBoilersActivated = 0;
            int gasMotorActivated = 0;
            int heatPumpActivated = 0;
            foreach (var asset in assets.Values)
            {
                // First deactivate all assets
                asset.IsActive = false;

                //Check if the default Danfoss units of Scenario 2 exist to activate them:
                if (asset.UnitType == "Boiler" && asset.FuelType == "Gas" && asset.ProductionCost == 520 && asset.MaxHeat == 4 && asset.CO2Emissions == 175 && asset.FuelConsumption == 0.9 && gasBoilerActivated < 1)
                {
                    asset.IsActive = true;
                    gasBoilerActivated++;
                }
                else if (asset.UnitType == "Boiler" && asset.FuelType == "Oil" && asset.ProductionCost == 670 && asset.MaxHeat == 4 && asset.CO2Emissions == 330 && asset.FuelConsumption == 1.5 && oilBoilersActivated < 1)
                {
                    asset.IsActive = true;
                    oilBoilersActivated++;
                }
                else if (asset.UnitType == "Motor" && asset.FuelType == "Gas" && asset.ProductionCost == 990 && asset.MaxHeat == 3.5 && asset.CO2Emissions == 650 && asset.FuelConsumption == 1.8 && asset.MaxElectricity == 2.6 && gasMotorActivated < 1)
                {
                    asset.IsActive = true;
                    gasMotorActivated++;
                }
                else if (asset.UnitType == "Heat Pump" && asset.ProductionCost == 60 && asset.MaxHeat == 6 && asset.MaxElectricity == -6 && heatPumpActivated < 1)
                {
                    asset.IsActive = true;
                    heatPumpActivated++;
                }
            }

            //If the default Danfoss units of Scenario 2 do not exist (aka have not been activated up there ↑), create them:
            if (gasBoilerActivated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Gas Boiler 1", ID = id, IsActive = true, UnitType = "Boiler", MaxHeat = 4, ProductionCost = 520, CO2Emissions = 175, FuelType = "Gas", FuelConsumption = 0.9 });
            }
            else if (oilBoilersActivated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Oil Boiler 1", ID = id, IsActive = true, UnitType = "Boiler", MaxHeat = 4, ProductionCost = 670, CO2Emissions = 330, FuelType = "Oil", FuelConsumption = 1.5});
            }
            else if (gasMotorActivated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Gas Motor 1", ID = id, IsActive = true, UnitType = "Motor", MaxHeat = 3.5, MaxElectricity = 2.6, ProductionCost = 990, CO2Emissions = 650, FuelType = "Gas", FuelConsumption = 1.8});
            }
            else if (heatPumpActivated < 1)
            {
                int id = AssetManager._nextAvailableId++;
                assets.Add(id, new AssetSpecifications { Name = "Heat Pump 1", ID = id, IsActive = true, UnitType = "Heat Pump", MaxHeat = 6, ProductionCost = 60 , FuelType = "", MaxElectricity = -6});
            }

            Console.WriteLine($"Scenario 2 loaded: {gasBoilerActivated} gas boiler, {oilBoilersActivated} oil boiler, " +$"{gasMotorActivated} gas motor, {heatPumpActivated} heat pump");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring Scenario 2: {ex.Message}");
        }
    }

    private void UpdateMaxHeat()
    {
        try
        {
            var assets = _assetManager.GetAllAssets();
            if (assets == null || assets.Count == 0)
                return;

            // Update the static MaxHeat property used by the optimizer
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
                // This will update the Production_Units.json file with the new configuration
                _assetManager.SaveAssets(assets.Values);
                Console.WriteLine("Asset changes saved to file");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving asset changes: {ex.Message}");
        }
    }
}