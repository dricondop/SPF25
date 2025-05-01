using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Views;

public partial class OptimizerView : UserControl
{
    public OptimizerView()
    {
        InitializeComponent();
    }

    /*private void RunOptimization_Click(object sender, RoutedEventArgs e)
    {
        // In a real implementation, this would trigger the optimization algorithm
        if (DataContext is OptimizerViewModel viewModel)
        {
            viewModel.RunOptimization();
        }
    }*/

    private void ViewResults_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to the Results Data Manager View
        WindowManager.TriggerResultDataManagerWindow();
    }

    private void VisualizeData_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to the Data Visualization View
        WindowManager.TriggerDataVisualizationWindow();
    }
}