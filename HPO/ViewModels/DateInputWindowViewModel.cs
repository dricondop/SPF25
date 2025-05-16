using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using HeatProductionOptimization.Services.DataProviders;

namespace HeatProductionOptimization.ViewModels
{
    public class DateInputWindowViewModel : ViewModelBase
    {
        private DateTimeOffset? _startDate;
        private DateTimeOffset? _endDate;
        private int _startHour;
        private int _endHour;
        private string _statusMessage = string.Empty;
        private bool _canProceed;
        private bool _useWinterData = true;
        private bool _useSummerData = true;
        private readonly IDataRangeProvider _dataRangeProvider;

        public static SelectedDateRange SelectedDateRange { get; private set; } = new SelectedDateRange();

        public DateInputWindowViewModel(IDataRangeProvider dataRangeProvider)
        {
            _dataRangeProvider = dataRangeProvider;
            UpdateDefaultDates();
            StartHour = 0;
            EndHour = 0;

            this.WhenAnyValue(
                x => x.UseWinterData,
                x => x.UseSummerData)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ShowDateSelection));
                    UpdateDefaultDates();
                });
        }

        public List<int> HourOptions => Enumerable.Range(0, 24).ToList();

        public bool UseWinterData
        {
            get => _useWinterData;
            set => this.RaiseAndSetIfChanged(ref _useWinterData, value);
        }

        public bool UseSummerData
        {
            get => _useSummerData;
            set => this.RaiseAndSetIfChanged(ref _useSummerData, value);
        }

        public DateTimeOffset? StartDate
        {
            get => _startDate;
            set
            {
                this.RaiseAndSetIfChanged(ref _startDate, value);
                ValidateDates();
            }
        }

        public DateTimeOffset? EndDate
        {
            get => _endDate;
            set
            {
                this.RaiseAndSetIfChanged(ref _endDate, value);
                ValidateDates();
            }
        }

        public int StartHour
        {
            get => _startHour;
            set
            {
                this.RaiseAndSetIfChanged(ref _startHour, value);
                ValidateDates();
            }
        }

        public int EndHour
        {
            get => _endHour;
            set
            {
                this.RaiseAndSetIfChanged(ref _endHour, value);
                ValidateDates();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool CanProceed
        {
            get => _canProceed;
            private set => this.RaiseAndSetIfChanged(ref _canProceed, value);
        }

        public bool ShowDateSelection => UseWinterData ^ UseSummerData;

        private void UpdateDefaultDates()
        {
            var (startDate, endDate) = GetAvailableDataRange();
            
            // Check if dates are valid before assigning
            if (startDate == DateTime.MinValue || startDate == DateTime.MaxValue)
                startDate = DateTime.Today;
            
            if (endDate == DateTime.MinValue || endDate == DateTime.MaxValue)
                endDate = DateTime.Today.AddDays(1);
            
            // Use constructor with explicit date
            StartDate = new DateTimeOffset(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, TimeSpan.Zero);
            EndDate = new DateTimeOffset(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0, TimeSpan.Zero);
            
            ValidateDates();
        }

        private (DateTime start, DateTime end) GetAvailableDataRange()
        {
            DateTime minDate = DateTime.MaxValue;
            DateTime maxDate = DateTime.MinValue;
            bool hasData = false;

            if (UseWinterData)
            {
                var winterRange = _dataRangeProvider.GetWinterDataRange();
                if (winterRange.start < minDate) minDate = winterRange.start;
                if (winterRange.end > maxDate) maxDate = winterRange.end;
                hasData = true;
            }

            if (UseSummerData)
            {
                var summerRange = _dataRangeProvider.GetSummerDataRange();
                if (summerRange.start < minDate) minDate = summerRange.start;
                if (summerRange.end > maxDate) maxDate = summerRange.end;
                hasData = true;
            }

            if (!hasData)
            {
                minDate = DateTime.Today;
                maxDate = DateTime.Today.AddDays(1);
            }
            
            if (minDate == DateTime.MinValue || minDate == DateTime.MaxValue)
                minDate = DateTime.Today;
            
            if (maxDate == DateTime.MinValue || maxDate == DateTime.MaxValue)
                maxDate = DateTime.Today.AddDays(1);
            
            return (minDate, maxDate);
            
        }

        private void ValidateDates()
        {
            if (!UseWinterData && !UseSummerData)
            {
                StatusMessage = "Please select at least one data group";
                CanProceed = false;
                return;
            }

            if (ShowDateSelection)
            {
                if (!StartDate.HasValue || !EndDate.HasValue)
                {
                    StatusMessage = "Please select both dates";
                    CanProceed = false;
                    return;
                }

                var start = StartDate.Value.DateTime.AddHours(StartHour);
                var end = EndDate.Value.DateTime.AddHours(EndHour);

                if (start >= end)
                {
                    StatusMessage = "Error: End date must be after start date";
                    CanProceed = false;
                    return;
                }

                bool isValid = true;
                string outOfRangeMessage = string.Empty;

                if (UseWinterData)
                {
                    var winterRange = _dataRangeProvider.GetWinterDataRange();
                    if (start < winterRange.start || end > winterRange.end)
                    {
                        outOfRangeMessage += $"Winter data: {winterRange.start:yyyy-MM-dd} to {winterRange.end:yyyy-MM-dd}\n";
                        isValid = false;
                    }
                }

                if (UseSummerData)
                {
                    var summerRange = _dataRangeProvider.GetSummerDataRange();
                    if (start < summerRange.start || end > summerRange.end)
                    {
                        outOfRangeMessage += $"Summer data: {summerRange.start:yyyy-MM-dd} to {summerRange.end:yyyy-MM-dd}\n";
                        isValid = false;
                    }
                }

                if (!isValid)
                {
                    StatusMessage = $"Selected range is outside available data:\n{outOfRangeMessage}";
                    CanProceed = false;
                    return;
                }
            }

            var winterRangeDisplay = _dataRangeProvider.GetWinterDataRange();
            var summerRangeDisplay = _dataRangeProvider.GetSummerDataRange();

            if (UseWinterData && !UseSummerData)
            {
                StatusMessage = $"Winter data range:\n{winterRangeDisplay.start:yyyy-MM-dd} to {winterRangeDisplay.end:yyyy-MM-dd}";
            }
            else if (!UseWinterData && UseSummerData)
            {
                StatusMessage = $"Summer data range:\n{summerRangeDisplay.start:yyyy-MM-dd} to {summerRangeDisplay.end:yyyy-MM-dd}";
            }
            else
            {
                StatusMessage = $"Available data ranges:\n" +
                              $"Winter: {winterRangeDisplay.start:yyyy-MM-dd} to {winterRangeDisplay.end:yyyy-MM-dd}\n" +
                              $"Summer: {summerRangeDisplay.start:yyyy-MM-dd} to {summerRangeDisplay.end:yyyy-MM-dd}";
            }

            CanProceed = true;
        }

        public void SubmitDates()
        {
            Console.WriteLine("SubmitDates called");
            ValidateDates();
            if (!CanProceed) 
            {
                Console.WriteLine("Cannot proceed - dates invalid");
                return;
            }

            SaveSelectedDateRange();
            Console.WriteLine("Date range saved, proceeding to optimizer");
            
            WindowManager.TriggerOptimizerWindow();
        }

        private void SaveSelectedDateRange()
        {
            if (UseWinterData && UseSummerData)
            {
                var (startDate, endDate) = GetAvailableDataRange();
                SelectedDateRange.StartDate = startDate;
                SelectedDateRange.EndDate = endDate;
                SelectedDateRange.UseWinterData = true;
                SelectedDateRange.UseSummerData = true;
                
                // Add this line to update IDataRangeProvider
                _dataRangeProvider.SetSelectedDateRange(startDate, endDate);
            }
            else if (ShowDateSelection)
            {
                if (!StartDate.HasValue || !EndDate.HasValue)
                {
                    // Handle the error or return early
                    Console.WriteLine("StartDate or EndDate is null. Cannot save selected date range.");
                    return;
                }

                DateTime start = StartDate.Value.DateTime.AddHours(StartHour);
                DateTime end = EndDate.Value.DateTime.AddHours(EndHour);
                
                SelectedDateRange.StartDate = start; 
                SelectedDateRange.EndDate = end;
                SelectedDateRange.UseWinterData = UseWinterData;
                SelectedDateRange.UseSummerData = UseSummerData;
                
                // Add this line to update IDataRangeProvider
                _dataRangeProvider.SetSelectedDateRange(start, end);
            }

            SelectedDateRange.StartHour = StartHour;
            SelectedDateRange.EndHour = EndHour;
            
            // Add debug message
            Console.WriteLine($"Saved date range: {SelectedDateRange.StartDate} to {SelectedDateRange.EndDate}");
            Console.WriteLine($"Using Winter: {SelectedDateRange.UseWinterData}, Summer: {SelectedDateRange.UseSummerData}");
        }
    }

    public class SelectedDateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public bool UseWinterData { get; set; }
        public bool UseSummerData { get; set; }
    }
}