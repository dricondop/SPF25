using System;
using System.IO;
using Xunit;
using HeatProductionOptimization.Services.Managers;

namespace HeatProductionOptimization.Tests
{
    public class SourceDataManagerTests
    {
        private readonly SourceDataManager _manager;

        public SourceDataManagerTests()
        {
            //SUT
            _manager = SourceDataManager.sourceDataManagerInstance;
        }

        [Fact]
        public void Singleton_ReturnsSameInstance()  // Tests singleton pattern implementation
        {
            // Arrange
            var instance1 = SourceDataManager.sourceDataManagerInstance;
            var instance2 = SourceDataManager.sourceDataManagerInstance;
            
            // Act & Assert combined
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void Constructor_InitializesEmptyCollections()  // Tests initial empty data collections
        {
            // No Arrange needed - constructor does setup (SUT)
            
            // Act & Assert combined
            Assert.Empty(_manager.WinterRecords);
            Assert.Empty(_manager.SummerRecords);
        }

        [Fact]
        public void GetDataRange_WithNoRecords_ReturnsMinMaxValues()  // Tests default date range with no data
        {
            // No Arrange needed - using empty collections
            
            // Act
            var (start, end) = _manager.GetDataRange(winter: true);
            
            // Assert
            Assert.Equal(DateTime.MinValue, start);
            Assert.Equal(DateTime.MaxValue, end);
        }
    }
}