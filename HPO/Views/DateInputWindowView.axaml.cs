using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Views;

public partial class DateInputWindowView : UserControl
{
    public DateInputWindowView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnSubmitClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DateInputWindowViewModel vm)
        {
            vm.OnSubmitClick();
        }
    }
}