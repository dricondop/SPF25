using System;
using System.IO;
using Xunit;
using HeatProductionOptimization.Services.Managers;

namespace HeatProductionOptimization.Tests
{
    public class SourceDataManagerTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly SourceDataManager _manager;

        public SourceDataManagerTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), "test_heat_demand.csv");
            _manager = SourceDataManager.sourceDataManagerInstance;
        }

        [Fact]
        public void Singleton_ReturnsSameInstance()
        {
            var instance1 = SourceDataManager.sourceDataManagerInstance;
            var instance2 = SourceDataManager.sourceDataManagerInstance;
            
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void ImportHeatDemandData_WithValidFile_LoadsDataCorrectly()
        {
            // Arrange
            CreateValidTestFile();

            // Act
            _manager.ImportHeatDemandData(_testFilePath);

            // Assert
            Assert.Single(_manager.WinterRecords);
            Assert.Single(_manager.SummerRecords);
            
            var winterRecord = _manager.WinterRecords[0];
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0), winterRecord.TimeFrom);
            Assert.Equal(50.5, winterRecord.HeatDemand);
            Assert.Equal(100.0, winterRecord.ElectricityPrice);
        }

        [Fact]
        public void ImportHeatDemandData_WithInvalidFilePath_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => 
                _manager.ImportHeatDemandData("nonexistent.csv"));
        }

        [Fact]
        public void ImportHeatDemandData_WithEmptyFile_ThrowsInvalidDataException()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "Header1,Header2,Header3,Header4,Header5,Header6,Header7,Header8\n");

            // Act & Assert
            Assert.Throws<InvalidDataException>(() => 
                _manager.ImportHeatDemandData(_testFilePath));
        }

        [Fact]
        public void ImportHeatDemandData_WithInvalidDateFormat_SkipsInvalidRecord()
        {
            // Arrange
            var content = @"Time From,Time To,Heat Demand,Price,Time From,Time To,Heat Demand,Price
invalid,1/1/2024 1:00,50.5,100.0,1/1/2024 0:00,1/1/2024 1:00,45.5,95.0";
            File.WriteAllText(_testFilePath, content);

            // Act
            _manager.ImportHeatDemandData(_testFilePath);

            // Assert
            Assert.Empty(_manager.WinterRecords);
            Assert.Single(_manager.SummerRecords);
        }

        [Fact]
        public void GetDataRange_WithValidData_ReturnsCorrectRange()
        {
            // Arrange
            CreateValidTestFile();
            _manager.ImportHeatDemandData(_testFilePath);

            // Act
            var (startDate, endDate) = _manager.GetDataRange(winter: true);

            // Assert
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0), startDate);
            Assert.Equal(new DateTime(2024, 1, 1, 1, 0, 0), endDate);
        }

        [Fact]
        public void GetDataRange_WithEmptyRecords_ReturnsMinMaxValue()
        {
            // Act
            var (startDate, endDate) = _manager.GetDataRange(winter: true);

            // Assert
            Assert.Equal(DateTime.MinValue, startDate);
            Assert.Equal(DateTime.MaxValue, endDate);
        }

        private void CreateValidTestFile()
        {
            var content = @"Time From,Time To,Heat Demand,Price,Time From,Time To,Heat Demand,Price
1/1/2024 0:00,1/1/2024 1:00,50.5,100.0,1/1/2024 0:00,1/1/2024 1:00,45.5,95.0";
            File.WriteAllText(_testFilePath, content);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}