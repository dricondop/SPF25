using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class SourceDataManager
{
    public List<HeatDemandRecord> WinterRecords { get; private set; }
    public List<HeatDemandRecord> SummerRecords { get; private set; }

    public SourceDataManager()
    {
        WinterRecords = new List<HeatDemandRecord>();
        SummerRecords = new List<HeatDemandRecord>();
    }

    public void ImportHeatDemandData(string filePath, bool displayData = false, int displayLimit = 6)
    {
        WinterRecords.Clear();
        SummerRecords.Clear();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var columns = line.Split(',');

                    if (columns.Length >= 10)
                    {
                        WinterRecords.Add(ParseHeatDemandRecord(columns, 1));
                        SummerRecords.Add(ParseHeatDemandRecord(columns, 6));
                    }
                }
            }

            if (displayData)
            {
                Console.WriteLine("First 6 Winter Periods:");
                DisplayLimitedRecords(WinterRecords, displayLimit);

                Console.WriteLine("\nFirst 6 Summer Periods:");
                DisplayLimitedRecords(SummerRecords, displayLimit);
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.Message}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Data format error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing heat demand data: {ex.Message}");
        }
    }

    private HeatDemandRecord ParseHeatDemandRecord(string[] columns, int startIndex)
    {
        string dateFormat = "M/d/yyyy H:mm";

        return new HeatDemandRecord
        {
            TimeFrom = DateTime.ParseExact(columns[startIndex].Trim(), dateFormat, CultureInfo.InvariantCulture),
            TimeTo = DateTime.ParseExact(columns[startIndex + 1].Trim(), dateFormat, CultureInfo.InvariantCulture),
            HeatDemand = double.Parse(columns[startIndex + 2].Trim(), CultureInfo.InvariantCulture),
            ElectricityPrice = double.Parse(columns[startIndex + 3].Trim(), CultureInfo.InvariantCulture)
        };
    }

    private void DisplayLimitedRecords(List<HeatDemandRecord> records, int limit)
    {
        foreach (var record in records.Take(limit))
        {
            Console.WriteLine($"Time From: {record.TimeFrom}, Time To: {record.TimeTo}, Heat Demand: {record.HeatDemand}, Electricity Price: {record.ElectricityPrice}");
        }
    }
}

public class HeatDemandRecord
{
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public double HeatDemand { get; set; }
    public double ElectricityPrice { get; set; }
}