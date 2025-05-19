using Xunit;
using HeatProductionOptimization.ViewModels;
using HeatProductionOptimization.Services.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatProductionOptimization.Tests
{
    public class ResultDataManagerTests
    {
        private readonly ResultDataManagerViewModel _viewModel;

        public ResultDataManagerTests()
        {
            _viewModel = new ResultDataManagerViewModel();
        }

        [Fact]
        public void Constructor_InitializesResultsData()
        {
            Assert.NotNull(_viewModel.ResultsData);
            Assert.NotNull(_viewModel.ResultsData.TimeStamps);
            Assert.NotNull(_viewModel.ResultsData.HeatDemand);
            Assert.NotNull(_viewModel.ResultsData.ElectricityPrice);
            Assert.NotNull(_viewModel.ResultsData.ProductionData);
            Assert.NotNull(_viewModel.ResultsData.TotalCosts);
            Assert.NotNull(_viewModel.ResultsData.TotalEmissions);
        }

        [Fact]
        public void LoadSampleData_PopulatesDataCorrectly()
        {
            // Sample data is loaded in constructor
            Assert.Equal(24, _viewModel.ResultsData.TimeStamps.Count);
            Assert.Equal(24, _viewModel.ResultsData.HeatDemand.Count);
            Assert.Equal(24, _viewModel.ResultsData.ElectricityPrice.Count);
            Assert.Equal(24, _viewModel.ResultsData.ProductionData.Count);
            Assert.Equal(24, _viewModel.ResultsData.TotalCosts.Count);
            Assert.Equal(24, _viewModel.ResultsData.TotalEmissions.Count);
        }

        [Fact]
        public void AddDataPoint_AddsCorrectData()
        {
            var resultsData = new ResultsData();
            var timestamp = DateTime.Now;
            var heatDemand = 100.0;
            var electricityPrice = 50.0;
            var production = new Dictionary<string, double>
            {
                { "GB", 25.0 },
                { "OB", 25.0 },
                { "GM", 25.0 },
                { "EK", 25.0 },
                { "HK", 25.0 }
            };
            var totalCost = 1000.0;
            var totalEmission = 500.0;

            resultsData.AddDataPoint(timestamp, heatDemand, electricityPrice, 
                                   production, totalCost, totalEmission);

            Assert.Single(resultsData.TimeStamps);
            Assert.Equal(timestamp, resultsData.TimeStamps[0]);
            Assert.Equal(heatDemand, resultsData.HeatDemand[0]);
            Assert.Equal(electricityPrice, resultsData.ElectricityPrice[0]);
            Assert.Equal(totalCost, resultsData.TotalCosts[0]);
            Assert.Equal(totalEmission, resultsData.TotalEmissions[0]);
            Assert.Equal(production, resultsData.ProductionData[0]);
        }

        [Fact]
        public void ClearData_RemovesAllData()
        {
            var resultsData = new ResultsData();
            resultsData.AddDataPoint(DateTime.Now, 100, 50, 
                new Dictionary<string, double>(), 1000, 500);

            resultsData.Clear();

            Assert.Empty(resultsData.TimeStamps);
            Assert.Empty(resultsData.HeatDemand);
            Assert.Empty(resultsData.ElectricityPrice);
            Assert.Empty(resultsData.ProductionData);
            Assert.Empty(resultsData.TotalCosts);
            Assert.Empty(resultsData.TotalEmissions);
        }

        [Fact]
        public void SampleData_HasValidRanges()
        {
            foreach (var heatDemand in _viewModel.ResultsData.HeatDemand)
            {
                Assert.InRange(heatDemand, 100, 300);
            }

            foreach (var price in _viewModel.ResultsData.ElectricityPrice)
            {
                Assert.InRange(price, 30, 80);
            }

            foreach (var cost in _viewModel.ResultsData.TotalCosts)
            {
                Assert.InRange(cost, 5000, 15000);
            }

            foreach (var emission in _viewModel.ResultsData.TotalEmissions)
            {
                Assert.InRange(emission, 100, 300);
            }
        }

        [Fact]
        public void ProductionData_ContainsAllUnits()
        {
            var expectedUnits = new[] { "GB", "OB", "GM", "EK", "HK" };
            
            foreach (var production in _viewModel.ResultsData.ProductionData)
            {
                foreach (var unit in expectedUnits)
                {
                    Assert.Contains(unit, production.Keys);
                    Assert.InRange(production[unit], 0, 100);
                }
            }
        }
    }
}