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
        new ImportJsonWindowViewModel(),
        new DateInputWindowViewModel()
    };

    public MainWindowViewModel()
    {
        CurrentPage = Windows[0]; // Set HomeWindow as initial view
        WindowManager.HomeWindow += () => HomeWindow();
        WindowManager.AssetManagerWindow += () => AssetManagerWindow();
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
    public void ImportJsonWindow()
    {
        CurrentPage = Windows[2];
    }

    public void DateInputWindow()
    {
        CurrentPage = Windows[3];
    }

}