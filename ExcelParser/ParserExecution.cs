using System;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(">>> Program started");

        try
        {
            var parser = new ExcelHeatDataParser();
            var path = @"C:\Users\gerga\Desktop\Parser\Parser\data.xlsx";

            int winter, summer;
            var data = parser.LoadExcelData(path, out winter, out summer);

            Console.WriteLine($">>> Parsed rows: Winter={winter}, Summer={summer}");
            Console.WriteLine($">>> Loaded days: {data.Count}");

            // -----------------------------
            // USER INPUT SECTION
            // -----------------------------
            Console.WriteLine();
            Console.WriteLine("Enter date (format: yyyy-MM-dd): ");
            string dateInput = Console.ReadLine();

            Console.WriteLine("Enter hour (0-23): ");
            string hourInput = Console.ReadLine();

            if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime day))
            {
                Console.WriteLine("Invalid date format!");
                return;
            }

            if (!int.TryParse(hourInput, out int hour) || hour < 0 || hour > 23)
            {
                Console.WriteLine("Invalid hour! Must be between 0 and 23.");
                return;
            }

            // -----------------------------
            // LOOKUP LOGIC
            // -----------------------------
            if (!data.TryGetValue(day, out var dayData))
            {
                Console.WriteLine($"No data found for day {day:yyyy-MM-dd}");
            }
            else if (!dayData.Hours.TryGetValue(hour, out var hourData))
            {
                Console.WriteLine($"No data found for hour {hour}:00");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(">>> RESULT:");
                Console.WriteLine($"Timestamp: {hourData.Timestamp}");
                Console.WriteLine($"Heat Demand (MWh): {hourData.HeatDemand:F3}");
                Console.WriteLine($"Electricity Price (DKK/MWh): {hourData.ElectricityPrice:F3}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(">>> ERROR OCCURRED:");
            Console.WriteLine(ex);
        }

        Console.WriteLine("\n>>> Finished. Press a key...");
        Console.ReadKey();
    }
}