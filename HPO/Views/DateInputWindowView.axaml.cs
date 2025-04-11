using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HeatProductionOptimization.ViewModels;
using HeatProductionOptimization.Services.Managers;

namespace HeatProductionOptimization.Views;

public partial class DateInputWindowView : UserControl
{
    public DateInputWindowView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}