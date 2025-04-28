using System;

namespace HeatProductionOptimization.Services.DataProviders;

public interface IDataRangeProvider
{
    (DateTime start, DateTime end) GetWinterDataRange();
    (DateTime start, DateTime end) GetSummerDataRange();
    (DateTime start, DateTime end) GetSelectedDateRange();
    void SetSelectedDateRange(DateTime start, DateTime end); // Rango seleccionado
}