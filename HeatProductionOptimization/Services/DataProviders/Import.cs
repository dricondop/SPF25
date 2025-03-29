using Avalonia.Interactivity;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using System;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace HeatProductionOptimization;

internal class ImportButton
{
    private IStorageProvider? StorageProvider;
    // Import button logic when clicked
    public ImportButton(UserControl control)
    {
        var topLevel = TopLevel.GetTopLevel(control);
        StorageProvider = topLevel?.StorageProvider;
    }
    internal async void ImportClicked(object? sender, RoutedEventArgs e)
    {
        if (StorageProvider is not null)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
                }
            });

            var file = files.FirstOrDefault();
            if (file != null)
            {
                Console.WriteLine($"File loaded: {file}");
                WindowManager.TriggerDateInputWindow();
            }
        }
        else
        {
            Console.WriteLine("Only .csv files supported");
        }
    }
}