using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree; // Add this for GetVisualDescendants
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
            Dispatcher.UIThread.InvokeShutdown();
        }

        [Theory]
        [MemberData(nameof(GetNavigationTestData))]
        public void NavigationButton_ShouldNavigateToCorrectView(string buttonName, Type expectedViewModelType)
        {
            // Arrange
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            window.Show();

            var button = window.FindControl<Button>(buttonName);
            Assert.NotNull(button); // Verify button exists
            
            // Act
            button.Focus();
            window.KeyPress(Key.Enter, RawInputModifiers.None);

            // Assert
            var viewModel = (MainWindowViewModel)window.DataContext;
            Assert.NotNull(viewModel.CurrentPage);
            Assert.IsType(expectedViewModelType, viewModel.CurrentPage);
        }

        public static IEnumerable<object[]> GetNavigationTestData()
        {
            yield return new object[] { "HomeButton", typeof(HomeWindowViewModel) };
            yield return new object[] { "AssetManagerButton", typeof(AssetManagerViewModel) };
            yield return new object[] { "SourceDataManagerButton", typeof(SourceDataManagerViewModel) };
            yield return new object[] { "OptimizerButton", typeof(OptimizerViewModel) };
            yield return new object[] { "DataVisualizationButton", typeof(DataVisualizationViewModel) };
            yield return new object[] { "ResultDataManagerButton", typeof(ResultDataManagerViewModel) };
            yield return new object[] { "SettingsButton", typeof(SettingsViewModel) };
        }
    }
}