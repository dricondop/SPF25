using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Views;

public partial class AssetManagerView : UserControl
{
    public AssetManagerView()
    {
        InitializeComponent();
        this.Loaded += AssetManagerView_Loaded;
    }

    private void AssetManagerView_Loaded(object? sender, RoutedEventArgs e)
    {
        // When the view is loaded, refresh the assets from the JSON file
        if (DataContext is AssetManagerViewModel viewModel)
        {
            viewModel.ReloadAssets();
        }
    }

    private new void DoubleTapped(object sender, RoutedEventArgs e)
    {
        if (sender is Border border)
        {
            var panel = border.Child as Panel;
            if (panel == null)
                return;

            var textBlock = panel.FindDescendantOfType<TextBlock>();
            var textBox = panel.FindDescendantOfType<TextBox>();

            if (textBlock != null && textBox != null)
            {
                textBlock.IsVisible = false;
                textBox.IsVisible = true;

                textBox.Focus();
                textBox.SelectAll();
            }
        }
    }

    private new void LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var panel = textBox.Parent as Panel;
            if (panel == null)
                return;

            var textBlock = panel.Children[0] as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = textBox.Text;
                textBox.IsVisible = false;
                textBlock.IsVisible = true;
            }
        }
    }

    private new void KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (sender is TextBox textBox)
            {
                var panel = textBox.Parent as Panel;
                if (panel == null) 
                    return;

                var textBlock = panel.Children[0] as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = textBox.Text;
                    textBox.IsVisible = false;
                    textBlock.IsVisible = true;

                    e.Handled = true;
                }
            }
        }
    }

    private void AddUnit_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AssetManagerViewModel viewModel)
        {
            var comboBox = this.FindControl<ComboBox>("UnitTypeComboBox");
            
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string unitType = selectedItem.Content?.ToString() ?? "Boiler";
                
                viewModel.SelectedUnitType = unitType;
            }
            
            // Add the unit
            viewModel.AddNewUnit();
        }
    }
    
    private void SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AssetManagerViewModel viewModel)
        {
            viewModel.SaveChanges();
        }
    }

    private async void LoadFile_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not AssetManagerViewModel viewModel)
        {
            Console.WriteLine("DataContext is not set correctly");
            return;
        }

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
            {
                Console.WriteLine("Could not get TopLevel");
                return;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select JSON File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new("JSON Files") { Patterns = new[] { "*.json" } },
                    new("All Files") { Patterns = new[] { "*" } }
                },
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
            });

            if (files.Count > 0 && files[0].TryGetLocalPath() is { } filePath)
            {
                viewModel.LoadFromFile(filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File dialog error: {ex}");
        }
    }
    
    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AssetManagerViewModel viewModel && viewModel.Assets?.Count > 0)
        {
            WindowManager.TriggerSourceDataManagerWindow();
        }
        else
        {
            Console.WriteLine("No assets loaded - cannot proceed");
        }
    }
}