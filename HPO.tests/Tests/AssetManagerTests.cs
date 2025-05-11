using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using HeatProductionOptimization.Services.Managers;
using HeatProductionOptimization.Models.DataModels;

namespace HeatProductionOptimization.Tests
{
    public class AssetManagerTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly AssetManager _assetManager;

        public AssetManagerTests()
        {
            // Setup test file path
            _testFilePath = Path.Combine(Path.GetTempPath(), "test_units.json");
            _assetManager = new AssetManager(_testFilePath);
        }

        [Fact]
        public void Constructor_WithValidPath_InitializesCorrectly()
        {
            Assert.NotNull(_assetManager.GetAllAssets());
            Assert.Equal(_testFilePath, _assetManager.GetFilePath());
        }

        [Fact]
        public void LoadAssetsSpecifications_WithNonExistentFile_ReturnsEmptyDictionary()
        {
            var assets = _assetManager.LoadAssetsSpecifications();
            Assert.Empty(assets);
        }

        [Fact]
        public void CreateNewUnit_WithBoilerType_CreatesBoilerWithCorrectProperties()
        {
            var boiler = _assetManager.CreateNewUnit("Boiler");
            
            Assert.NotNull(boiler);
            Assert.Equal("Boiler", boiler.UnitType);
            Assert.True(boiler.ID > 0);
        }

        [Fact]
        public void CreateNewUnit_WithMotorType_CreatesMotorWithCorrectProperties()
        {
            var motor = _assetManager.CreateNewUnit("Motor");
            
            Assert.NotNull(motor);
            Assert.Equal("Motor", motor.UnitType);
            Assert.True(motor.ID > 0);
        }

        [Fact]
        public void CreateNewUnit_WithHeatPumpType_CreatesHeatPumpWithCorrectProperties()
        {
            var heatPump = _assetManager.CreateNewUnit("Heat Pump");
            
            Assert.NotNull(heatPump);
            Assert.Equal("Heat Pump", heatPump.UnitType);
            Assert.True(heatPump.ID > 0);
        }

        [Fact]
        public void SaveAssets_WithValidAssets_SavesSuccessfully()
        {
            var assets = new List<AssetSpecifications>
            {
                _assetManager.CreateNewUnit("Boiler"),
                _assetManager.CreateNewUnit("Motor")
            };

            bool result = _assetManager.SaveAssets(assets);
            
            Assert.True(result);
            Assert.True(File.Exists(_testFilePath));
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

        [Fact]
        public void UpdateAssetDictionary_WithDuplicateIds_SkipsDuplicates()
        {
            var assets = new List<AssetSpecifications>
            {
                new AssetSpecifications { ID = 1, Name = "Unit 1" },
                new AssetSpecifications { ID = 1, Name = "Unit 2" }
            };

            _assetManager.SaveAssets(assets);
            var savedAssets = _assetManager.GetAllAssets();
            
            Assert.Single(savedAssets);
            Assert.Equal("Unit 1", savedAssets[1].Name);
        }

        public void Dispose()
        {
            // Cleanup test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}