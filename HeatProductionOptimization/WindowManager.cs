using System;

namespace HeatProductionOptimization;

public static class WindowManager
{
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

}