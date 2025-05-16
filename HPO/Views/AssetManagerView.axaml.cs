using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Views;

public partial class AssetManagerView : UserControl
{
    public AssetManagerView()
    {
        InitializeComponent();
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
            // If the ComboBox selection isn't set in the ViewModel, try to get it directly
            var comboBox = this.FindControl<ComboBox>("UnitTypeComboBox");
            
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Extract text content from the ComboBoxItem
                string unitType = selectedItem.Content?.ToString() ?? "Boiler";
                
                // Set it in the ViewModel
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
        if (DataContext is AssetManagerViewModel viewModel)
        {
            try
            {
                // Create file picker
                var dialog = new OpenFileDialog
                {
                    Title = "Select JSON Asset File",
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "JSON Files", Extensions = new List<string> { "json" } },
                        new FileDialogFilter { Name = "All Files", Extensions = new List<string> { "*" } }
                    }
                };
                
                // Try to set initial directory to the same directory as the current file
                string currentFilePath = viewModel.CurrentFilePath;
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    string? directory = Path.GetDirectoryName(currentFilePath);
                    if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    {
                        dialog.Directory = $"Couldn't set the initial directory in AssetManager: {directory}";
                    }
                }
                
                // Show dialog and get result
                var window = this.VisualRoot as Window;
                if (window == null)
                {
                    viewModel.StatusMessage = "Error: Unable to get the parent window for file dialog.";
                    return;
                }
                var result = await dialog.ShowAsync(window);
                
                if (result != null && result.Length > 0)
                {
                    viewModel.LoadFromFile(result[0]);
                }
            }
            catch (Exception ex)
            {
                viewModel.StatusMessage = $"Error loading file: {ex.Message}";
            }
        }
    }
}