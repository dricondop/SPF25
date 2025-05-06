﻿using System;
using System.IO;
using System.Linq;
using HeatProductionOptimization.Services.Managers;
using ReactiveUI;
using HeatProductionOptimization.Services.DataProviders;

namespace HeatProductionOptimization.ViewModels;

public class SourceDataManagerViewModel : ViewModelBase, IDataRangeProvider
{
    private SourceDataManager _sourceDataManager = SourceDataManager.sourceDataManagerInstance;
    private string _statusMessage = "Select a CSV file to begin";
    private string? _currentFilePath;
    private string _dateRange = "Date range: Not available";
    
    private DateTime _selectedStartDate = DateTime.MinValue;
    private DateTime _selectedEndDate = DateTime.MinValue;

    public SourceDataManagerViewModel()
    {
        Initialize();
    }

    public (DateTime winterStart, DateTime winterEnd, DateTime summerStart, DateTime summerEnd) GetSelectedDateRange()
    {

            (DateTime winterStart, DateTime winterEnd)  = GetWinterDataRange();
            (DateTime summerStart, DateTime summerEnd)  = GetSummerDataRange();
            
        return (winterStart, winterEnd, summerStart, summerEnd);
    }

    public void SetSelectedDateRange(DateTime start, DateTime end)
    {
        _selectedStartDate = start;
        _selectedEndDate = end;
    }

    private void Initialize()
    {
        try
        {
            var locations = new[]
            {
                Path.GetFullPath("../../../Resources/Data/ElectricityData.csv")
            };

            _currentFilePath = locations.FirstOrDefault(File.Exists);
            
            if (_currentFilePath != null)
            {
                LoadData(_currentFilePath);
            }
            else
            {
                StatusMessage = "Default data file not found in any location!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public string? CurrentFilePath 
    { 
        get => _currentFilePath; 
        private set => this.RaiseAndSetIfChanged(ref _currentFilePath, value); 
    }

    public string DateRange
    {
        get => _dateRange;
        private set => this.RaiseAndSetIfChanged(ref _dateRange, value);
    }

    public bool HasValidData => _sourceDataManager.WinterRecords.Count > 0 || 
                               _sourceDataManager.SummerRecords.Count > 0;

    public (DateTime start, DateTime end) GetWinterDataRange()
    {
        if (_sourceDataManager.WinterRecords.Count == 0)
            return (DateTime.MinValue, DateTime.MaxValue);
            
        return (_sourceDataManager.WinterRecords.Min(r => r.TimeFrom),
                _sourceDataManager.WinterRecords.Max(r => r.TimeTo));
    }

    public (DateTime start, DateTime end) GetSummerDataRange()
    {
        if (_sourceDataManager.SummerRecords.Count == 0)
            return (DateTime.MinValue, DateTime.MaxValue);
            
        return (_sourceDataManager.SummerRecords.Min(r => r.TimeFrom),
                _sourceDataManager.SummerRecords.Max(r => r.TimeTo));
    }

    public void LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "Invalid file path provided";
            return;
        }

        LoadData(filePath);
    }

    private void LoadData(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                StatusMessage = $"File not found: {filePath}";
                DateRange = "Date range: File not found";
                return;
            }

            _sourceDataManager.ImportHeatDemandData(filePath);
            CurrentFilePath = filePath;
            StatusMessage = $"Successfully loaded: {Path.GetFileName(filePath)}";
            UpdateDateRange();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
            DateRange = "Date range: Error";
        }
    }

    private void UpdateDateRange()
    {
        try
        {
            string winterRange = "No winter data";
            string summerRange = "No summer data";

            if (_sourceDataManager.WinterRecords.Count > 0)
            {
                var winterStart = _sourceDataManager.WinterRecords.Min(r => r.TimeFrom);
                var winterEnd = _sourceDataManager.WinterRecords.Max(r => r.TimeTo);
                winterRange = $"Winter: {winterStart:yyyy-MM-dd} to {winterEnd:yyyy-MM-dd}";
            }

            if (_sourceDataManager.SummerRecords.Count > 0)
            {
                var summerStart = _sourceDataManager.SummerRecords.Min(r => r.TimeFrom);
                var summerEnd = _sourceDataManager.SummerRecords.Max(r => r.TimeTo);
                summerRange = $"Summer: {summerStart:yyyy-MM-dd} to {summerEnd:yyyy-MM-dd}";
            }

            DateRange = $"{winterRange} | {summerRange}";
        }
        catch (Exception)
        {
            DateRange = "Date range: Calculation error";
        }
    }
}