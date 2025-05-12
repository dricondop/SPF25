using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.Input;
using System.Linq;
using HeatProductionOptimization;

namespace HeatProductionOptimization.Tests
{
    public static class AvaloniaApp
    {
        private static bool _isRegistered;

        public static void RegisterPlatform()
        {
            if (_isRegistered)
                return;

            var builder = AppBuilder.Configure<App>();
            
            builder
                .UseHeadless(new AvaloniaHeadlessPlatformOptions
                {
                    UseHeadlessDrawing = true
                })
                .LogToTrace();

            builder.SetupWithoutStarting();
            _isRegistered = true;
        }

        public static void Stop()
        {
            Dispatcher.UIThread.InvokeShutdown();
            _isRegistered = false;
        }
    }
}