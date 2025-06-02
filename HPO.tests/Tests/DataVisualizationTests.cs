using Xunit;
using HeatProductionOptimization.ViewModels;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace HeatProductionOptimization.Tests
{
    public class DataVisualizationTests
    {
        private readonly DataVisualizationViewModel _viewModel;
        private readonly SourceDataManager _sourceDataManager;

        public DataVisualizationTests()
        {
            //SUT
            _viewModel = new DataVisualizationViewModel();
            _sourceDataManager = SourceDataManager.sourceDataManagerInstance;
        }

        [Fact]
        public void Constructor_InitializesDefaultValues()  // Tests default initialization of chart properties
        {
            // No Arrange needed - constructor does setup in test class
            
            // Act & Assert combined since we're just verifying initialization
            Assert.NotNull(_viewModel.CartesianSeries);
            Assert.NotNull(_viewModel.XAxes);
            Assert.NotNull(_viewModel.YAxes);
            Assert.Equal("Optimization Results", _viewModel.SelectedDataSource);
            Assert.Equal("Line Chart", _viewModel.SelectedChartType);
            Assert.True(_viewModel.ShowLegend);
            Assert.True(_viewModel.ShowDataLabels);
            Assert.True(_viewModel.ShowGridLines);
            Assert.True(_viewModel.AutoScale);
        }

        [Fact]
        public void ChartWidth_ReturnsMinimumWidth_WhenNoData()  // Tests minimum chart width with no data
        {
            // No Arrange needed - using default state
            
            // Act & Assert combined
            Assert.Equal(900, _viewModel.ChartWidth);
        }

        [Fact]
        public void UpdateChart_WithHeatDemandData_CreatesCorrectAxes()  // Tests chart axes setup for heat demand data
        {
            // Arrange
            _viewModel.SelectedDataSource = "Heat Demand Data";
            _viewModel.SelectedChartType = "Line Chart";
            var testRecord = new HeatDemandRecord
            {
                TimeFrom = DateTime.Now,
                HeatDemand = 100
            };
            _sourceDataManager.WinterRecords.Add(testRecord);

            // Act 
            _viewModel.UpdateChartCommand.Execute(null);

            // Assert
            Assert.Single(_viewModel.XAxes);
            Assert.Single(_viewModel.YAxes);
            Assert.Contains("Heat Demand", _viewModel.YAxes[0].Name);
        }

        [Fact]
        public void AutoScale_UpdatesYAxisLimits()  // Tests Y-axis limits update when auto-scaling
        {
            // Arrange
            _viewModel.AutoScale = true;

            // Act
            _viewModel.AutoScale = false;

            // Assert
            Assert.Equal(0, _viewModel.YAxes[0].MinLimit);
        }

        [Fact]
        public void ShowGridLines_UpdatesAxisSeparators()  // Tests grid lines visibility updates
        {
            // Arrange
            _viewModel.ShowGridLines = true;

            // Act
            _viewModel.ShowGridLines = false;

            // Assert
            Assert.False(_viewModel.XAxes[0].ShowSeparatorLines);
            Assert.False(_viewModel.YAxes[0].ShowSeparatorLines);
        }

        [Fact]
        public void ShowDataLabels_UpdatesAxisTitles()  // Tests data labels visibility updates
        {
            // Arrange
            string expectedTitle = "Test Title";
            _viewModel.XAxes[0].Name = expectedTitle;

            // Act
            _viewModel.ShowDataLabels = false;

            // Assert
            Assert.Equal("", _viewModel.XAxes[0].Name);
        }

        [Fact]
        public void GenerateAllCharts_SavesChartFilesToTempDirectory()
        {
            var viewModel = new DataVisualizationViewModel();
            
            var chartImages = viewModel.GenerateAllCharts();

            Assert.NotNull(chartImages);
            Assert.Equal(4, chartImages.Count);
            
            Assert.Contains("HeatDemand", chartImages.Keys);
            Assert.Contains("ElectricityPrice", chartImages.Keys);
            Assert.Contains("Optimization", chartImages.Keys);
            Assert.Contains("Production", chartImages.Keys);
            
            var tempPath = Path.GetTempPath();
            foreach (var path in chartImages.Values)
            {
                Assert.StartsWith(tempPath, path, StringComparison.OrdinalIgnoreCase);
                Assert.True(File.Exists(path), $"File {path} does not exist");
            }
            
            foreach (var path in chartImages.Values)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}