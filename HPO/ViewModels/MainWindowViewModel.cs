using Avalonia.Controls;
using ReactiveUI;

namespace HeatProductionOptimization.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage = null!;
    
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }
    
    private readonly ViewModelBase[] Windows =
    {
        new HomeWindowViewModel(),
        new AssetManagerViewModel(),
        new SourceDataManagerViewModel(),
        new OptimizerViewModel(),
        new DataVisualizationViewModel(),
        new ResultDataManagerViewModel(),
        new SettingsViewModel(),
        new ImportJsonWindowViewModel(),
        new DateInputWindowViewModel(),
    };

    public MainWindowViewModel()
    {
        CurrentPage = Windows[0];
        WindowManager.HomeWindow += () => HomeWindow();
        WindowManager.AssetManagerWindow += () => AssetManagerWindow();
        WindowManager.SourceDataManagerWindow += () => SourceDataManagerWindow();
        WindowManager.OptimizerWindow += () => OptimizerWindow();
        WindowManager.DataVisualizationWindow += () => DataVisualizationWindow();
        WindowManager.ResultDataManagerWindow += () => ResultDataManagerWindow();
        WindowManager.SettingsWindow += () => SettingsWindow();
        WindowManager.ImportJsonWindow += () => ImportJsonWindow();
        WindowManager.DateInputWindow += () => DateInputWindow();
        
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