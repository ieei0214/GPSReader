using System.Globalization;
using Microsoft.Extensions.Logging;
using GPSReader;
using GPSReader.Interfaces;
using GPSReader.Parsers;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                //builder.AddConfiguration(configuration.GetSection("Logging")).AddDebug().AddConsole();
            });
            ILogger<GPSReaderService> logger = loggerFactory.CreateLogger<GPSReaderService>();
            var gpsReaderService = new GPSReaderService(logger, new SerialInput());

            gpsReaderService.OnGPGGAUpdated += (sender, e) =>
            {
                Console.Clear();
                Console.WriteLine(e.RawData);
                var utcTime = DateTime.ParseExact(e.GPGGAData.UTC, "HHmmss.ff", CultureInfo.InvariantCulture);
                Console.WriteLine($"UTC: {e.GPGGAData.UTC} ({utcTime.ToString()})");
                Console.WriteLine($"Latitude: {e.GPGGAData.Latitude}");
                Console.WriteLine($"Longitude: {e.GPGGAData.Longitude}");
                Console.WriteLine($"Quality: {e.GPGGAData.Quality}");
                Console.WriteLine($"Satellites: {e.GPGGAData.Satellites}");
                Console.WriteLine($"HDOP: {e.GPGGAData.HDOP}");
                Console.WriteLine($"Altitude: {e.GPGGAData.Altitude}{e.GPGGAData.AltitudeUnits}");
                Console.WriteLine($"GeoidHeight: {e.GPGGAData.GeoidHeight}{e.GPGGAData.GeoidHeightUnits}");
                Console.WriteLine($"DGPSDataAge: {e.GPGGAData.DGPSDataAge}");
                Console.WriteLine($"Checksum: {e.GPGGAData.Checksum}");
            };

            gpsReaderService.StartReading();
            Console.ReadKey();

            gpsReaderService.StopReading();

        }
    }
}
