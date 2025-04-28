using Avalonia.Controls;
using ReactiveUI;
using HeatProductionOptimization.Services.DataProviders;

namespace HeatProductionOptimization.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    private readonly IDataRangeProvider _dataRangeProvider;
    private readonly ViewModelBase[] Windows;

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public MainWindowViewModel()
    {
        _dataRangeProvider = new SourceDataManagerViewModel();
        Windows = new ViewModelBase[]
        {
            new HomeWindowViewModel(),
            new AssetManagerViewModel(),
            new SourceDataManagerViewModel(),
            new OptimizerViewModel(_dataRangeProvider), 
            new DataVisualizationViewModel(),
            new ResultDataManagerViewModel(),
            new SettingsViewModel(),
            new ImportJsonWindowViewModel(),
            new DateInputWindowViewModel(_dataRangeProvider)
        };
        CurrentPage = Windows[0];
        _currentPage = CurrentPage;
        
        WindowManager.HomeWindow += () => CurrentPage = Windows[0];
        WindowManager.AssetManagerWindow += () => CurrentPage = Windows[1];
        WindowManager.SourceDataManagerWindow += () => CurrentPage = Windows[2];
        WindowManager.OptimizerWindow += () => CurrentPage = Windows[3];
        WindowManager.DataVisualizationWindow += () => CurrentPage = Windows[4];
        WindowManager.ResultDataManagerWindow += () => CurrentPage = Windows[5];
        WindowManager.SettingsWindow += () => CurrentPage = Windows[6];
        WindowManager.ImportJsonWindow += () => CurrentPage = Windows[7];
        WindowManager.DateInputWindow += () => CurrentPage = Windows[8];  
    }

    public void HomeWindow()
    {
        CurrentPage = Windows[0];
    }
    public void AssetManagerWindow()
    {
        CurrentPage = Windows[1];
    }
    public void SourceDataManagerWindow()
    {
        CurrentPage = Windows[2];
    }
    public void OptimizerWindow()
    {
        CurrentPage = Windows[3];
    }
    public void DataVisualizationWindow()
    {
        CurrentPage = Windows[4];
    }
    public void ResultDataManagerWindow()
    {
        CurrentPage = Windows[5];
    }
    public void SettingsWindow()
    {
        CurrentPage = Windows[6];
    }
    public void ImportJsonWindow()
    {
        CurrentPage = Windows[7];
    }

    public void DateInputWindow()
    {
        CurrentPage = Windows[8];
    }

}