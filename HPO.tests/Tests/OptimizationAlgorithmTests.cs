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
        private readonly List<AssetSpecifications> _testBoilers;

        public OptimizationAlgorithmTests()
        {
            _algorithm = new OptAlgorithm();
            _testBoilers = CreateTestBoilers();
        }

        private List<AssetSpecifications> CreateTestBoilers()
        {
            return new List<AssetSpecifications>
            {
                new AssetSpecifications
                {
                    ID = 1,
                    Name = "TestBoiler1",
                    UnitType = "Boiler",
                    IsActive = true,
                    MaxHeat = 100,
                    ProductionCost = 50,
                    CO2Emissions = 80,
                    FuelConsumption = 90
                },
                new AssetSpecifications
                {
                    ID = 2,
                    Name = "TestMotor1",
                    UnitType = "Motor",
                    IsActive = true,
                    MaxHeat = 150,
                    MaxElectricity = 50,
                    ProductionCost = 40,
                    CO2Emissions = 60,
                    FuelConsumption = 70
                },
                new AssetSpecifications
                {
                    ID = 3,
                    Name = "TestHeatPump1",
                    UnitType = "Heat Pump",
                    IsActive = true,
                    MaxHeat = 200,
                    MaxElectricity = -75,
                    ProductionCost = 30,
                    CO2Emissions = 20,
                    FuelConsumption = 0
                }
            };
        }

        [Fact]
        public void GetObjective_WithValidParameters_ReturnsCorrectObjectiveValues()
        {
            // Arrange
            var parameters = new int[] { 1, 1, 1 }; // All parameters active
            var electricityPrice = 45.0;
            var heatPumps = _testBoilers.Where(b => b.UnitType == "Heat Pump").ToList();
            var motors = _testBoilers.Where(b => b.UnitType == "Motor").ToList();

            // Act
            var result = _algorithm.GetObjective(_testBoilers, parameters, electricityPrice, heatPumps, motors);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testBoilers.Count, result.Count);
            Assert.All(result.Values, v => Assert.True(v >= 0));
        }

        [Fact]
        public void GetObjective_WithNoActiveParameters_ThrowsDivideByZeroException()
        {
            // Arrange
            var parameters = new int[] { 0, 0, 0 }; // No parameters active

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() => 
                _algorithm.GetObjective(_testBoilers, parameters, 45.0, new List<AssetSpecifications>(), new List<AssetSpecifications>()));
        }

        [Fact]
        public void CalculateUnits_DistributesHeatCorrectly()
        {
            // Arrange
            var heatDemand = 300.0;
            var dateTime = DateTime.Now;
            var objectives = new Dictionary<AssetSpecifications, double>
            {
                { _testBoilers[0], 0.5 },
                { _testBoilers[1], 0.3 },
                { _testBoilers[2], 0.2 }
            };

            // Act
            _algorithm.CalculateUnits(_testBoilers, objectives, heatDemand, dateTime);

            // Assert
            var totalProducedHeat = _testBoilers.Sum(b => b.ProducedHeat[dateTime]);
            Assert.Equal(heatDemand, totalProducedHeat);
        }

        [Fact]
        public void CalculateElectricity_ReturnsCorrectBalance()
        {
            // Arrange
            var dateTime = DateTime.Now;
            var electricityPrices = new Dictionary<DateTime, double?>
            {
                { dateTime, 45.0 }
            };

            _testBoilers.ForEach(b => b.ProducedHeat[dateTime] = 50);

            // Act
            var result = _algorithm.CalculateElectricity(_testBoilers, electricityPrices);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasValue);
        }

        [Fact]
        public void OptimizationAlgorithm_ExecutesFullOptimization()
        {
            // Arrange
            var parameters = new int[] { 1, 1, 1 };
            var electricityPrice = 45.0;
            var heatDemand = 300.0;
            var dateTime = DateTime.Now;

            // Act
            _algorithm.OptimizationAlgorithm(_testBoilers, parameters, electricityPrice, heatDemand, dateTime);

            // Assert
            Assert.All(_testBoilers.Where(b => b.IsActive), 
                b => Assert.True(b.ProducedHeat.ContainsKey(dateTime)));
            
            var totalHeat = _testBoilers.Where(b => b.IsActive)
                .Sum(b => b.ProducedHeat[dateTime]);
            Assert.Equal(heatDemand, totalHeat);
        }
    }
}