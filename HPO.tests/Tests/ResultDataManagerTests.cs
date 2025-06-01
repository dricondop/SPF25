using Xunit;
using HeatProductionOptimization.ViewModels;
using System;
using System.Threading.Tasks;
using System.IO;

namespace HeatProductionOptimization.Tests
{
    public class ResultDataManagerTests
    {
        private readonly ResultDataManagerViewModel _viewModel;

        public ResultDataManagerTests()
        {
            //SUT
            _viewModel = new ResultDataManagerViewModel();
        }

        [Fact]
        public void Constructor_InitializesCorrectly() // Tests basic initialization of ViewModel
        {
            
            // Act & Assert
            Assert.NotNull(_viewModel);
            Assert.NotNull(_viewModel.ResultsData);
        }

        [Fact]
        public async Task ExportDataToCsv_WithNoActiveAssets_ReturnsEarly() // Tests graceful handling of empty export
        {
            // Act
            await _viewModel.ExportDataToCsv();
            
            // Assert - passes if no exception thrown
        }

        [Fact]
        public async Task GenerateAndSavePdfReport_WithNoData_ReturnsEarly() // Tests graceful handling of empty PDF generation
        {
            // Act
            await _viewModel.GenerateAndSavePdfReport();
            
            // Assert - passes if no exception thrown
        }
    }
}