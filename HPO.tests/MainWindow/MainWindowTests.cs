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
using Xunit;
using HeatProductionOptimization.Views;
using HeatProductionOptimization.ViewModels;

namespace HeatProductionOptimization.Tests
{
    public class MainWindowTests : IDisposable
    {
        private MainWindow _window;
        private MainWindowViewModel _viewModel;
        
        public MainWindowTests()
        {
            // Initialize Avalonia Headless platform with app settings
            AvaloniaApp.RegisterPlatform();
            
            _window = new MainWindow();
            _viewModel = new MainWindowViewModel();
            _window.DataContext = _viewModel;
        }

        public void Dispose()
        {
            Dispatcher.UIThread.InvokeShutdown();
        }

        [AvaloniaFact]
        public void ClickHomeButton_ShouldNavigateToHomeView()
        {
            // Arrange
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            window.Show();

            var homeButton = window.FindControl<Button>("HomeButton");
            
            // Act
            homeButton?.Focus();
            window.KeyPress(Key.Enter, RawInputModifiers.None);

            // Assert
            var viewModel = (MainWindowViewModel)window.DataContext;
            Assert.NotNull(viewModel.CurrentPage);
            Assert.IsType<HomeWindowViewModel>(viewModel.CurrentPage);
        }

        [AvaloniaFact]
        public void ClickSettingsButton_ShouldNavigateToSettingsView()
        {
            // Arrange
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            window.Show();

            var settingsButton = window.FindControl<Button>("SettingsButton");
            
            // Act
            settingsButton?.Focus();
            window.KeyPress(Key.Enter, RawInputModifiers.None);

            // Assert
            var viewModel = (MainWindowViewModel)window.DataContext;
            Assert.NotNull(viewModel.CurrentPage);
            Assert.IsType<SettingsViewModel>(viewModel.CurrentPage);
        }
    }
}