using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace HeatProductionOptimization.Views;

public partial class ImportJsonWindowView : UserControl
{
    private ImportButton _importButton;
    public ImportJsonWindowView()
    {
        AvaloniaXamlLoader.Load(this);
        _importButton = new ImportButton(this);
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        _importButton.ImportClicked(sender, e);
    }
}