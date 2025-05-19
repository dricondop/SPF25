using System;
using Xunit;
using HeatProductionOptimization.Services.Managers;

namespace HeatProductionOptimization.Tests
{
    public class SourceDataManagerTests
    {
        private readonly SourceDataManager _manager;

        public SourceDataManagerTests()
        {
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
        public void Constructor_InitializesEmptyCollections()
        {
            Assert.Empty(_manager.WinterRecords);
            Assert.Empty(_manager.SummerRecords);
        }

        [Fact]
        public void GetDataRange_WithNoRecords_ReturnsMinMaxValues()
        {
            var (start, end) = _manager.GetDataRange(winter: true);
            
            Assert.Equal(DateTime.MinValue, start);
            Assert.Equal(DateTime.MaxValue, end);
        }
    }
}