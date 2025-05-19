using Xunit;
using HeatProductionOptimization.ViewModels;
using HeatProductionOptimization.Models.DataModels;
using HeatProductionOptimization.Services.Managers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HeatProductionOptimization.Tests
{
    public class DataVisualizationTests
    {
        private readonly DataVisualizationViewModel _viewModel;
        private readonly SourceDataManager _sourceDataManager;

        public DataVisualizationTests()
        {
            _viewModel = new DataVisualizationViewModel();
            _sourceDataManager = SourceDataManager.sourceDataManagerInstance;
        }

        [Fact]
        public void Constructor_InitializesDefaultValues()
        {
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
        public void ChartWidth_ReturnsMinimumWidth_WhenNoData()
        {
            Assert.Equal(900, _viewModel.ChartWidth);
        }

        [Fact]
        public void UpdateChart_WithHeatDemandData_CreatesCorrectAxes()
        {
            _viewModel.SelectedDataSource = "Heat Demand Data";
            _viewModel.SelectedChartType = "Line Chart";

            var testRecord = new HeatDemandRecord
            {
                TimeFrom = DateTime.Now,
                HeatDemand = 100
            };
            _sourceDataManager.WinterRecords.Add(testRecord);

            _viewModel.UpdateChartCommand.Execute(null);

            Assert.Single(_viewModel.XAxes);
            Assert.Single(_viewModel.YAxes);
            Assert.Contains("Heat Demand", _viewModel.YAxes[0].Name);
        }

        [Fact]
        public void AutoScale_UpdatesYAxisLimits()
        {
            _viewModel.AutoScale = true;

            _viewModel.AutoScale = false;

            Assert.Equal(0, _viewModel.YAxes[0].MinLimit);
        }

        [Fact]
        public void ShowGridLines_UpdatesAxisSeparators()
        {
            _viewModel.ShowGridLines = true;

            _viewModel.ShowGridLines = false;

            Assert.False(_viewModel.XAxes[0].ShowSeparatorLines);
            Assert.False(_viewModel.YAxes[0].ShowSeparatorLines);
        }

        [Fact]
        public void ShowDataLabels_UpdatesAxisTitles()
        {
            string expectedTitle = "Test Title";
            _viewModel.XAxes[0].Name = expectedTitle;

            _viewModel.ShowDataLabels = false;

            Assert.Equal("", _viewModel.XAxes[0].Name);
        }
    }
}