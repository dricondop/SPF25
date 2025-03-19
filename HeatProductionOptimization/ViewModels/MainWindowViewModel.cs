using System.ComponentModel;

namespace HeatProductionOptimization.ViewModels;

public partial class MainWindowViewModel : ViewModelBase , INotifyPropertyChanged
{
    
    private ViewModelBase _currentPage;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    
    public MainWindowViewModel()
    {
        _currentPage = Windows[0];
        WindowManager.ImportJsonWindow += () => ImportJsonWindow();
        WindowManager.DateInputWindow += () => DateInputWindow();
    }
    
    
    public ViewModelBase CurrentPage
    {
        get {return _currentPage; }
    }

    private ViewModelBase[] Windows =
    {
        new ImportJsonWindowViewModel(),
        new DateInputWindowViewModel()
    };

    public System.Action ImportJsonWindow()
    {
        _currentPage = Windows[1];
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
        return null;
    }

    public System.Action DateInputWindow()
    {
        _currentPage = Windows[0];
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
        return null;
    }

}
