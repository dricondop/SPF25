using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

    public class BoilerSpecification 
    {
        //This Dictionary has to be in other class, 
        // but it is going to be here for now
        public Dictionary<string, BoilerSpecification>? _boilers;
 
        [JsonPropertyName("Type")]
        public string? Type { get; set; }

        [JsonPropertyName("Max Heat (MW)")]
        public double MaxHeat { get; set; }

        [JsonPropertyName("Max Electricity (MW)")]
        public double? MaxElectricity { get; set; }

        [JsonPropertyName("Production Cost (DKK/MWh)")]
        public double ProductionCost { get; set; }

        [JsonPropertyName("CO2 Emissions (kg/MWh)")]
        public double CO2Emissions { get; set; }

        [JsonPropertyName("Fuel Type")]
        public string? FuelType { get; set; }

        [JsonPropertyName("Fuel Consumption (MWh fuel/MWh heat)")]
        public double FuelConsumption { get; set; }
        
        public double ProducedHeat { get; set; } = 0.0;

    // Method to load boiler specifications from a JSON file
    public Dictionary<string, BoilerSpecification> LoadBoilerSpecifications()
    {
        var boilers = new Dictionary<string, BoilerSpecification>();
        
        // Check if the file exists
        if (!File.Exists("Product_unit.json")) 
        {
            Console.WriteLine("Error: file not found.");
            return boilers;
        }

        try
        {
            // Read the JSON file
            var json = File.ReadAllText("Product_unit.json");
            
            // Deserialize the JSON content to a dictionary
            var boiler_dictionary = JsonSerializer.Deserialize<Dictionary<string, BoilerSpecification>>(json);
            return boiler_dictionary ?? new Dictionary<string, BoilerSpecification>();
          
        }
        catch (Exception ex)  // Handle exceptions
        {
            Console.WriteLine($"Error reading Json: {ex.Message}");
        }

        return boilers; 
    }

    //We would have to set the value in other place by creatin an instance of this class, 
    // then use the LoadBoilerSpecifications() to set the _boilers and then use the GetBoilerSpecification() method

    // Method to get a specific boiler specification by name
    public BoilerSpecification GetBoilerSpecification(string name, Dictionary<string, BoilerSpecification> boiler)
    {
        return boiler.TryGetValue(name, out var spec) ? spec : throw new KeyNotFoundException($"Boiler specification '{name}' not found.");
    }

}