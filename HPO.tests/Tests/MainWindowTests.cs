using Avalonia.Controls;
using Avalonia.Threading;
using Xunit;
using HeatProductionOptimization.Views;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Tests
{
    public class MainWindowTests : IDisposable
    {
        private readonly MainWindow _window;
        private readonly MainWindowViewModel _viewModel;

        public MainWindowTests()
        {
            AvaloniaApp.RegisterPlatform();
            _window = new MainWindow();
            _viewModel = new MainWindowViewModel();
            _window.DataContext = _viewModel;
        }

        public void Dispose()
        {
            try 
            {
                Dispatcher.UIThread.InvokeShutdown();
            }
            catch { /* Ignore shutdown errors */ }
        }

        [Theory]
        [InlineData("HomeButton", typeof(HomeWindowViewModel))]
        [InlineData("AssetManagerButton", typeof(AssetManagerViewModel))]
        [InlineData("OptimizerButton", typeof(OptimizerViewModel))]
        public void NavigationButton_ShouldNavigateToCorrectView(string buttonName, System.Type expectedType)
        {
            // Find button by nname
            var button = _window.FindControl<Button>(buttonName);
            Assert.NotNull(button);

            // This simulates clicking a button
            button.Command?.Execute(null);

            Assert.NotNull(_viewModel.CurrentPage);
            Assert.IsType(expectedType, _viewModel.CurrentPage);
        }
    }
}