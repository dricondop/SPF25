using System;

namespace HeatProductionOptimization;

public static class WindowManager
{
    public static event Action? HomeWindow;
    public static event Action? AssetManagerWindow;
    public static event Action? ImportJsonWindow;
    public static event Action? DateInputWindow;
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
}