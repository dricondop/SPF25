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
        public ImportButton(Window window)
        {
            StorageProvider = window.StorageProvider;
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
                }
            }
            else
            {
                Console.WriteLine("Only .csv files supported");
            }
        }
}