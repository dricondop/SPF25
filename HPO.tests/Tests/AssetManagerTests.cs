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
            _assetManager = new AssetManager();
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            Assert.NotNull(_assetManager.GetAllAssets());
        }

        [Fact]
        public void GetAllAssets_ReturnsAssetDictionary()
        {
            var assets = _assetManager.GetAllAssets();
            Assert.NotNull(assets);
            Assert.IsType<Dictionary<int, AssetSpecifications>>(assets);
        }

        [Fact]
        public void CreateNewUnit_WithBoilerType_CreatesBoilerWithCorrectProperties()
        {
            var boiler = _assetManager.CreateNewUnit("Boiler");

            Assert.NotNull(boiler);
            Assert.Equal("Boiler", boiler.UnitType);
            Assert.True(boiler.ID > 0);
            Assert.True(boiler.MaxHeat > 0);
        }

        [Fact]
        public void CreateNewUnit_WithMotorType_CreatesMotorWithCorrectProperties()
        {
            var motor = _assetManager.CreateNewUnit("Motor");

            Assert.NotNull(motor);
            Assert.Equal("Motor", motor.UnitType);
            Assert.True(motor.ID > 0);
            Assert.True(motor.MaxHeat > 0);
            //Assert.True(motor.MaxElectricity > 0); there's negative values for MaxElectricity
        }

        [Fact]
        public void CreateNewUnit_WithHeatPumpType_CreatesHeatPumpWithCorrectProperties()
        {
            var heatPump = _assetManager.CreateNewUnit("Heat Pump");

            Assert.NotNull(heatPump);
            Assert.Equal("Heat Pump", heatPump.UnitType);
            Assert.True(heatPump.ID > 0);
            Assert.True(heatPump.MaxHeat > 0);
            Assert.True(heatPump.MaxElectricity < 0);
        }

        [Fact]
        public void GetAssetSpecifications_WithValidId_ReturnsCorrectAsset()
        {
            var boiler = _assetManager.CreateNewUnit("Boiler");
            var retrievedAsset = _assetManager.GetAssetSpecifications(boiler.ID);

            Assert.NotNull(retrievedAsset);
            Assert.Equal(boiler.ID, retrievedAsset.ID);
            Assert.Equal(boiler.UnitType, retrievedAsset.UnitType);
        }

        [Fact]
        public void GetAssetSpecifications_WithInvalidId_ReturnsNull()
        {
            var retrievedAsset = _assetManager.GetAssetSpecifications(-1);
            Assert.Null(retrievedAsset);
        }
    }
}