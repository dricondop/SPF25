using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.ViewModels;
using HeatProductionOptimization.Views;
using Xunit;

namespace HeatProductionOptimization.test;

public class AssetManagerTests
{   
    [AvaloniaFact]
    public async Task CreateNewAsset_ShouldSaveToJsonFile()
    {
        // Arrange
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_assets_{Guid.NewGuid()}.json");
        try
        {
            // Create empty JSON file with empty array
            File.WriteAllText(tempFilePath, "[]");
            
            // Initialize the AssetManager with our test file
            var assetManager = new AssetManager();
            assetManager.JsonFilePath = tempFilePath;
            
            // Create and initialize the view/viewmodel
            var viewModel = new AssetManagerViewModel(assetManager);
            var view = new AssetManagerView
            {
                DataContext = viewModel
            };
            
            // Show the view
            var window = new Window { Content = view };
            window.Show();
            await Task.Delay(100); // Give UI time to initialize
            
            // Act
            // Find and click "Add Asset" button
            var addButton = view.FindControl<Button>("AddAssetButton");
            Assert.NotNull(addButton);
            
            await Dispatcher.UIThread.InvokeAsync(() => {
                addButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
            await Task.Delay(100); // Wait for dialog to open
            
            // Find the dialog window that opened
            var dialog = window.GetVisualDescendants().OfType<Window>().LastOrDefault();
            Assert.NotNull(dialog);
            
            // Fill the form fields
            var nameTextBox = dialog.FindControl<TextBox>("NameTextBox");
            var typeComboBox = dialog.FindControl<ComboBox>("TypeComboBox");
            var valueTextBox = dialog.FindControl<TextBox>("ValueTextBox");
            var descriptionTextBox = dialog.FindControl<TextBox>("DescriptionTextBox");
            
            await Dispatcher.UIThread.InvokeAsync(() => {
                nameTextBox.Text = "Test Asset";
                typeComboBox.SelectedIndex = 0; // Select first asset type
                valueTextBox.Text = "1000";
                descriptionTextBox.Text = "Test asset description";
            });
            
            // Click save button
            var saveButton = dialog.FindControl<Button>("SaveButton");
            Assert.NotNull(saveButton);
            
            await Dispatcher.UIThread.InvokeAsync(() => {
                saveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            });
            
            await Task.Delay(100); // Wait for saving to complete
            
            // Assert
            // Verify asset was saved to the JSON file
            string jsonContent = File.ReadAllText(tempFilePath);
            var assets = JsonSerializer.Deserialize<List<Asset>>(jsonContent);
            
            Assert.NotNull(assets);
            Assert.Single(assets);
            
            var savedAsset = assets.First();
            Assert.Equal("Test Asset", savedAsset.Name);
            Assert.Equal(1000, savedAsset.Value);
            Assert.Equal("Test asset description", savedAsset.Description);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
