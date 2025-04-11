using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HeatProductionOptimization.ViewModels;
using HeatProductionOptimization.Services.Managers;

namespace HeatProductionOptimization.Views;

public partial class SourceDataManagerView : UserControl
{
    public SourceDataManagerView()
    {
        InitializeComponent();
    }

    private async void LoadFile_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SourceDataManagerViewModel viewModel)
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
                Title = "Select CSV File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new("CSV Files") { Patterns = new[] { "*.csv" } },
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
        if (DataContext is SourceDataManagerViewModel viewModel && viewModel.HasValidData)
        {
            WindowManager.TriggerDateInputWindow();
        }
        else
        {
            Console.WriteLine("No valid data loaded - cannot proceed");
        }
    }
}