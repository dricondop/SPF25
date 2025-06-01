using System.Collections.Generic;
using System.Linq;
using Xunit;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Tests
{
    public class AssetManagerTests
    {
        private readonly AssetManager _assetManager;

        public AssetManagerTests()
        {
            //SUT (System Under Test), this means _assetManager is now global for every test in this file
            _assetManager = new AssetManager();
        }

        [Fact]
        public void Constructor_InitializesCorrectly()  // Tests that asset manager initializes with expected default state
        {
            // No Arrange needed - constructor does setup (SUT)

            // Act
            var assets = _assetManager.GetAllAssets();
            
            // Assert
            Assert.NotNull(assets);  // Dictionary is created
            Assert.IsType<Dictionary<int, AssetSpecifications>>(assets);  // Correct type
            Assert.Empty(assets);  // Should be empty when file not found
            Assert.Empty(_assetManager.GetFilePath());  // No valid file path in test environment
        }

        [Fact]
        public void GetAllAssets_ReturnsAssetDictionary()  // Tests asset dictionary retrieval
        {
            // Arrange & Act
            var assets = _assetManager.GetAllAssets();
            
            // Assert
            Assert.NotNull(assets);
            Assert.IsType<Dictionary<int, AssetSpecifications>>(assets);
        }

        [Fact] 
        public void CreateNewUnit_WithBoilerType_CreatesBoilerWithCorrectProperties()  // Tests boiler unit creation
        {
            // Act
            var boiler = _assetManager.CreateNewUnit("Boiler");

            // Assert
            Assert.NotNull(boiler);
            Assert.Equal("Boiler", boiler.UnitType);
            Assert.True(boiler.ID > 0);
            Assert.True(boiler.MaxHeat > 0);
        }

        [Fact]
        public void CreateNewUnit_WithMotorType_CreatesMotorWithCorrectProperties()  // Tests motor unit creation
        {
            // Act
            var motor = _assetManager.CreateNewUnit("Motor");

            // Assert
            Assert.NotNull(motor);
            Assert.Equal("Motor", motor.UnitType);
            Assert.True(motor.ID > 0);
            Assert.True(motor.MaxHeat > 0);
        }

        [Fact]
        public void CreateNewUnit_WithHeatPumpType_CreatesHeatPumpWithCorrectProperties()  // Tests heat pump unit creation
        {
            // Act
            var heatPump = _assetManager.CreateNewUnit("Heat Pump");

            // Assert
            Assert.NotNull(heatPump);
            Assert.Equal("Heat Pump", heatPump.UnitType);
            Assert.True(heatPump.ID > 0);
            Assert.True(heatPump.MaxHeat > 0);
            Assert.True(heatPump.MaxElectricity < 0);
        }

        [Fact]
        public void GetAssetSpecifications_WithValidId_ReturnsCorrectAsset()  // Tests asset retrieval with valid ID
        {
            // Arrange
            var boiler = _assetManager.CreateNewUnit("Boiler");

            // Act
            var retrievedAsset = _assetManager.GetAssetSpecifications(boiler.ID);

            // Assert
            Assert.NotNull(retrievedAsset);
            Assert.Equal(boiler.ID, retrievedAsset.ID);
            Assert.Equal(boiler.UnitType, retrievedAsset.UnitType);
        }

        [Fact]
        public void GetAssetSpecifications_WithInvalidId_ReturnsNull()  // Tests asset retrieval with invalid ID
        {
            // Act
            var retrievedAsset = _assetManager.GetAssetSpecifications(-1);

            // Assert
            Assert.Null(retrievedAsset);
        }

        [Fact]
        public void GetFilePath_WhenFileNotFound_ReturnsEmptyString() // Test if file path error handling work
        {
            // No Arrange needed - constructor does setup (SUT)

            // Act
            var path = _assetManager.GetFilePath();
            
            // Assert
            Assert.Empty(path);
        }

        [Fact]
        public void CreateNewUnit_WithInvalidType_CreatesDefaultBoiler()  // Tests invalid type defaults to boiler
        {
            // No Arrange needed - constructor does setup (SUT)

            // Act
            var unit = _assetManager.CreateNewUnit("InvalidType");

            // Assert
            Assert.NotNull(unit);
            Assert.Equal("Boiler", unit.UnitType); //Default to Boiler if type is null, empty, whitespace or unknown (see AssetManager class method CreateNewUnit())
        }

        [Fact]
        public void CreateMultipleUnits_AssignsUniqueIds()  // Tests unique ID assignment so they don't repeat
        {
            // Arrange
            var units = new List<AssetSpecifications>();

            // Act
            units.Add(_assetManager.CreateNewUnit("Boiler"));
            units.Add(_assetManager.CreateNewUnit("Motor"));
            units.Add(_assetManager.CreateNewUnit("Heat Pump"));

            // Assert
            var uniqueIds = units.Select(u => u.ID).Distinct(); // Select all distinct ID's from the created units
            Assert.Equal(units.Count, uniqueIds.Count()); // Check if the number of units created is equal to the number of distinct ID's
        }
    }
}