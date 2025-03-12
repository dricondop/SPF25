using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

    class Program
    {
        // Static dictionary to store all boiler specifications
        private static readonly Dictionary<string, BoilerSpecification> _boilers;

        static Program()
        {
            _boilers = LoadBoilerSpecifications(); // Load boiler specifications once when the program starts
        }
        
        // Accessing specific boilers by their names
        var GB1 = GetBoilerSpecification("GB1");
        var GB2 = GetBoilerSpecification("GB2");
        var OB1 = GetBoilerSpecification("OB1");
        var GM1 = GetBoilerSpecification("GM1");
        var HP1 = GetBoilerSpecification("HP1");
    }
    public class BoilerSpecification 
    {
        [Name("Name")] // Column name in the CSV file
        public string Name { get; set; } // Property name in the class

        [Name("Type")]
        public string Type { get; set; }

        [Name("Max Heat (MW)")]
        public double MaxHeat { get; set; }

        [Name("Max Electricity (MW)")]
        public double? MaxElectricity { get; set; }

        [Name("Production Cost (DKK/MWh)")]
        public double ProductionCost { get; set; }

        [Name("CO2 Emissions (kg/MWh)")]
        public double? CO2Emissions { get; set; }

        [Name("Fuel Type")]
        public string FuelType { get; set; }

        [Name("Fuel Consumption (MWh fuel/MWh heat)")]
        public double? FuelConsumption { get; set; }
    }

    // Static method to load boiler specifications
    private static Dictionary<string, BoilerSpecification> LoadBoilerSpecifications()
    {
        var boilers = new Dictionary<string, BoilerSpecification>(); 

        if (!File.Exists("Product_units.csv")) // Check if the file exists
        {
            Console.WriteLine("Error: file not found.");
            return boilers;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) // CSV configuration
        {
            HasHeaderRecord = true, // File has a header record
            Delimiter = ",", // Delimiter is a comma
            MissingFieldFound = null, // Ignore missing fields
            IgnoreBlankLines = true    // Ignore blank lines
        };

        try
        {
            using (var reader = new StreamReader("Product_units.csv")) // Read the CSV file
            using (var csv = new CsvReader(reader, config)) // Create a CSV reader
            {
                foreach (var record in csv.GetRecords<BoilerSpecification>()) // Read records from the CSV
                {
                    boilers[record.Name] = record; // Add the boiler specification to the dictionary
                }
            }
        }
        catch (Exception ex)  // Handle exceptions
        {
            Console.WriteLine($"Error reading CSV: {ex.Message}");
        }

        return boilers; 
    }

    // Static method to retrieve a boiler specification by name
    private static BoilerSpecification GetBoilerSpecification(string name)
    {
        return _boilers.TryGetValue(name, out var spec) ? spec : null;
    }

