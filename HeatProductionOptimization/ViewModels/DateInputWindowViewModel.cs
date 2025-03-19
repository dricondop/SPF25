using System;
using ReactiveUI;
using Avalonia.Threading;

namespace HeatProductionOptimization.ViewModels;

public class DateInputWindowViewModel : ViewModelBase
{
    private DateTimeOffset? _selectedDate;
    private TimeSpan? _selectedTime;
    private string _result = string.Empty;

    public DateTimeOffset? SelectedDate
    {
        get => _selectedDate;
        set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
    }

    public TimeSpan? SelectedTime
    {
        get => _selectedTime;
        set => this.RaiseAndSetIfChanged(ref _selectedTime, value);
    }

    public string Result
    {
        get => _result;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    public void OnSubmitClick()
    {
        if (SelectedDate.HasValue && SelectedTime.HasValue)
        {
            var dateTime = SelectedDate.Value.Date + SelectedTime.Value;
            Result = $"Selected DateTime: {dateTime}";
            WindowManager.TriggerImportJsonWindow();
        }
        else
        {
            Result = "Please select both date and time";
        }
    }
}
