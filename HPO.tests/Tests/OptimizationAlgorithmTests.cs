using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using HeatProductionOptimization.Models;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Tests
{
    public class OptimizationAlgorithmTests
    {
        private readonly OptAlgorithm _algorithm;
        private readonly AssetSpecifications _testBoiler;
        private readonly AssetSpecifications _testMotor;
        private readonly AssetSpecifications _testHeatPump;

        public OptimizationAlgorithmTests()
        {
            _algorithm = new OptAlgorithm();

            // Create one of each type for simpler testing
            _testBoiler = new AssetSpecifications
            {
                Name = "TestBoiler",
                UnitType = "Boiler",
                IsActive = true,
                MaxHeat = 100,
                ProductionCost = 50,
                CO2Emissions = 80,
                FuelConsumption = 90
            };

            _testMotor = new AssetSpecifications
            {
                Name = "TestMotor",
                UnitType = "Motor",
                IsActive = true,
                MaxHeat = 150,
                MaxElectricity = 50,
                ProductionCost = 40,
                CO2Emissions = 60,
                FuelConsumption = 70
            };

            _testHeatPump = new AssetSpecifications
            {
                Name = "TestHeatPump",
                UnitType = "Heat Pump",
                IsActive = true,
                MaxHeat = 200,
                MaxElectricity = -75,
                ProductionCost = 30,
                CO2Emissions = 20,
                FuelConsumption = 0
            };
        }

        [Fact]
        public void GetObjective_ShouldCalculateForSingleBoiler()
        {
            var units = new List<AssetSpecifications> { _testBoiler };
            var parameters = new int[] { 1, 0, 0 }; // Only cost parameter active

            var result = _algorithm.GetObjective(units, parameters, 0, new List<AssetSpecifications>(), new List<AssetSpecifications>());

            Assert.Single(result);
            Assert.Equal(_testBoiler, result.Keys.First());
        }

        [Fact]
        public void CalculateUnits_ShouldDistributeHeatToSingleUnit()
        {
            var units = new List<AssetSpecifications> { _testBoiler };
            var objectives = new Dictionary<AssetSpecifications, double> { { _testBoiler, 1.0 } };
            var heatDemand = 50.0;
            var dateTime = DateTime.Now;

            _algorithm.CalculateUnits(units, objectives, heatDemand, dateTime);

            Assert.Equal(heatDemand, _testBoiler.ProducedHeat[dateTime]);
        }

        [Fact]
        public void CalculateElectricity_ShouldCalculateForSingleMotor()
        {
            var units = new List<AssetSpecifications> { _testMotor };
            var dateTime = DateTime.Now;
            var electricityPrices = new Dictionary<DateTime, double?> { { dateTime, 45.0 } };
            _testMotor.ProducedHeat[dateTime] = 50;

            var result = _algorithm.CalculateElectricity(units, electricityPrices);

            Assert.NotNull(result);
        }

        [Fact]
        public void CalculateUnits_WithExcessiveHeatDemand_RespectsMaxCapacity()
        {
            var units = new List<AssetSpecifications> { _testBoiler };
            var objectives = new Dictionary<AssetSpecifications, double> { { _testBoiler, 1.0 } };
            _testBoiler.ProducedHeat = new Dictionary<DateTime, double?>();
            var dateTime = DateTime.Now;
            var excessiveHeatDemand = _testBoiler.MaxHeat + 50.0;

            _algorithm.CalculateUnits(units, objectives, excessiveHeatDemand, dateTime);

            Assert.Equal(_testBoiler.MaxHeat, _testBoiler.ProducedHeat[dateTime]);
        }

        [Fact]
        public void CalculateElectricity_ForHeatPump_CalculatesConsumptionCost()
        {
            var units = new List<AssetSpecifications> { _testHeatPump };
            var dateTime = DateTime.Now;
            _testHeatPump.ProducedHeat = new Dictionary<DateTime, double?> 
            { 
                { dateTime, 100.0 } 
            };
            var electricityPrices = new Dictionary<DateTime, double?> 
            { 
                { dateTime, 45.0 } 
            };

            var result = _algorithm.CalculateElectricity(units, electricityPrices);

            Assert.True(result > 0, "Heat pump should have positive electricity cost");
        }

        [Fact]
        public void OptimizationAlgorithm_WithMultipleUnits_OptimizesCorrectly()
        {
            var units = new List<AssetSpecifications> 
            { 
                _testBoiler, _testMotor, _testHeatPump 
            };
            var parameters = new int[] { 1, 1, 1 };
            var dateTime = DateTime.Now;
            var heatDemand = 300.0;

            // Initialize ProducedHeat dictionaries
            _testBoiler.ProducedHeat = new Dictionary<DateTime, double?>();
            _testMotor.ProducedHeat = new Dictionary<DateTime, double?>();
            _testHeatPump.ProducedHeat = new Dictionary<DateTime, double?>();

            _algorithm.OptimizationAlgorithm(units, parameters, 45.0, heatDemand, dateTime);

            var totalHeatProduced = units.Sum(u => u.ProducedHeat[dateTime] ?? 0);
            Assert.Equal(heatDemand, totalHeatProduced);
        }
    }
}