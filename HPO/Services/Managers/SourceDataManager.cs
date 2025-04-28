using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HeatProductionOptimization.Services.Managers
{
    public class SourceDataManager
    {
        public List<HeatDemandRecord> WinterRecords { get; }
        public List<HeatDemandRecord> SummerRecords { get; }

        public SourceDataManager()
        {
                WinterRecords = new List<HeatDemandRecord>();
                SummerRecords = new List<HeatDemandRecord>();
        }

        public void ImportHeatDemandData(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("CSV file not found", filePath);

            WinterRecords.Clear();
            SummerRecords.Clear();

            try
            {
                var lines = File.ReadAllLines(filePath);
                
                // Skip header rows (first line)
                foreach (var line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = line.Split(',');
                    if (columns.Length >= 8)
                    {
                        // Winter data (columns 1-4)
                        if (TryParseRecord(columns, 0, out var winterRecord))
                            WinterRecords.Add(winterRecord);

                        // Summer data (columns 5-8)
                        if (TryParseRecord(columns, 4, out var summerRecord))
                            SummerRecords.Add(summerRecord);
                    }
                }

                if (WinterRecords.Count == 0 && SummerRecords.Count == 0)
                    throw new InvalidDataException("No valid data records found in CSV file");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading CSV data: {ex}");
                throw;
            }
        }

        private static bool TryParseRecord(string[] columns, int startIndex, out HeatDemandRecord record)
        {
            record = new HeatDemandRecord();
            
            try
            {
                if (columns.Length <= startIndex + 3)
                {
                    Console.WriteLine($"Not enough columns (need {startIndex + 4}, got {columns.Length})");
                    return false;
                }

                // Debug: Print the columns we're trying to parse
                Console.WriteLine($"Parsing: {string.Join("|", columns.Skip(startIndex).Take(4))}");

                record.TimeFrom = DateTime.ParseExact(columns[startIndex].Trim(), "M/d/yyyy H:mm", CultureInfo.InvariantCulture);
                record.TimeTo = DateTime.ParseExact(columns[startIndex + 1].Trim(), "M/d/yyyy H:mm", CultureInfo.InvariantCulture);
                record.HeatDemand = double.Parse(columns[startIndex + 2].Trim(), CultureInfo.InvariantCulture);
                record.ElectricityPrice = double.Parse(columns[startIndex + 3].Trim(), CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse record: {ex.Message}");
                Console.WriteLine($"Problematic columns: {string.Join("|", columns.Skip(startIndex).Take(4))}");
                return false;
            }
        }

        public (DateTime StartDate, DateTime EndDate) GetDataRange(bool winter)
        {
            var records = winter ? WinterRecords : SummerRecords;
            if (records.Count == 0)
                return (DateTime.MinValue, DateTime.MaxValue);

            return (records.Min(r => r.TimeFrom), records.Max(r => r.TimeTo));
        }
        
        //This method is not being used and may disappear depending on David's datarange selection
        //For David: Don't worry about this method
        public Dictionary<DateTime, double> CurrentElectricityPrice(string filePath, List<HeatDemandRecord>? WinterData, List<HeatDemandRecord>? SummerData)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException("File path is null or empty.");
            } 
            ImportHeatDemandData(filePath);

            Dictionary<DateTime, double> pricesList = new();
            if(WinterData != null && SummerData == null)
            {
                foreach(HeatDemandRecord data in WinterData)
                {
                    pricesList[data.TimeFrom] = data.ElectricityPrice;
                }
            }
            else if(WinterData == null && SummerData != null)
            {
                foreach(HeatDemandRecord data in SummerData)
                {
                    pricesList[data.TimeFrom] = data.ElectricityPrice;
                }
            }
            else if(WinterData == null && SummerData == null)
            {
                Console.WriteLine("No electricity prices found");
            }
            else if(WinterData != null && SummerData != null)
            {
                foreach(HeatDemandRecord data in WinterData)
                {
                    pricesList[data.TimeFrom] = data.ElectricityPrice;
                }
                foreach(HeatDemandRecord data in SummerData)
                {
                    pricesList[data.TimeFrom] = data.ElectricityPrice;
                }
            }
            else
            {
                Console.WriteLine("No electricity prices found");
            }
            
            foreach(KeyValuePair<DateTime,double> price in pricesList)
            {
                Console.WriteLine($"Day and time: {price.Key}, electricity price: {price.Value}");
            }
            return pricesList;
        }
    }

    public class HeatDemandRecord
    {
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public double HeatDemand { get; set; }
        public double ElectricityPrice { get; set; }
    }
}