using System.Globalization;
using GPSReader;
using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static void OnGPGGAUpdated(GPSReaderService gpsReaderService, Window gpggaWindow)
    {
        gpsReaderService.OnGPGGAUpdated += (sender, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                gpggaWindow.RemoveAll();
                int y = 0;
                var utcTime = DateTime.ParseExact(e.GPGGAData.UTC!, "HHmmss.ff", CultureInfo.InvariantCulture);
                gpggaWindow.Add(
                    new Label($"Raw Data: {e.RawData}") { Y = y++ },
                    new Label($"UTC: {e.GPGGAData.UTC} {utcTime.ToString()}") { Y = y++ },
                    new Label($"Latitude: {e.GPGGAData.Latitude}") { Y = y++ },
                    new Label($"Longitude: {e.GPGGAData.Longitude}") { Y = y++ },
                    new Label($"Quality: {e.GPGGAData.Quality}") { Y = y++ },
                    new Label($"Satellites: {e.GPGGAData.Satellites}") { Y = y++ },
                    new Label($"HDOP: {e.GPGGAData.HDOP}") { Y = y++ },
                    new Label($"Altitude: {e.GPGGAData.Altitude}{e.GPGGAData.AltitudeUnits}") { Y = y++ },
                    new Label($"GeoidHeight: {e.GPGGAData.GeoidHeight}{e.GPGGAData.GeoidHeightUnits}") { Y = y++ },
                    new Label($"DGPSDataAge: {e.GPGGAData.DGPSDataAge}") { Y = y++ },
                    new Label($"Checksum: {e.GPGGAData.Checksum}") { Y = y++ }
                );
            });
        };
    }
}