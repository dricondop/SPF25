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
        new ImportJsonWindowViewModel(),
        new DateInputWindowViewModel()
    };

    public MainWindowViewModel()
    {
        CurrentPage = Windows[1];
        WindowManager.ImportJsonWindow += () => ImportJsonWindow();
        WindowManager.DateInputWindow += () => DateInputWindow();
    }

    public void ImportJsonWindow()
    {
        CurrentPage = Windows[0];
    }

    public void DateInputWindow()
    {
        CurrentPage = Windows[1];
    }
}
