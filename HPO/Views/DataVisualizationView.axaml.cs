using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HeatProductionOptimization.Services.Managers;

namespace HeatProductionOptimization.Views;

public partial class DataVisualizationView : UserControl
{
    public DataVisualizationView()
    {
        InitializeComponent();
    }
    
    private void Export_Click(object sender, RoutedEventArgs e)
    {
        WindowManager.TriggerResultDataManagerWindow();
    }
}