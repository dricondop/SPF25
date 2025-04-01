using System;

namespace HeatProductionOptimization;

public static class WindowManager
{
    public static event Action? HomeWindow;
    public static event Action? AssetManagerWindow;
    public static event Action? SourceDataManagerWindow;
    public static event Action? OptimizerWindow;
    public static event Action? DataVisualizationWindow;
    public static event Action? ResultDataManagerWindow;
    public static event Action? ImportJsonWindow;
    public static event Action? DateInputWindow;
    public static event Action? SettingsWindow;
    
    public static void TriggerImportJsonWindow()
    {
        ImportJsonWindow?.Invoke();
    }
    public static void TriggerDateInputWindow()
    {
        DateInputWindow?.Invoke();
    }
    public static void TriggerHomeWindow()
    {
        HomeWindow?.Invoke();
    }
    public static void TriggerAssetManagerWindow()
    {
        AssetManagerWindow?.Invoke();
    }
    public static void TriggerSourceDataManagerWindow()
    {
        SourceDataManagerWindow?.Invoke();
    }
    public static void TriggerOptimizerWindow()
    {
        OptimizerWindow?.Invoke();
    }
    public static void TriggerDataVisualizationWindow()
    {
        DataVisualizationWindow?.Invoke();
    }
    public static void TriggerResultDataManagerWindow()
    {
        ResultDataManagerWindow?.Invoke();
    }
    public static void TriggerSettingsWindow()
    {
        SettingsWindow?.Invoke();
    }
}