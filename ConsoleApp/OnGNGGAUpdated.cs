using System.Globalization;
using GPSReader;
using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static void OnGNGGAUpdated(GPSReaderService gpsReaderService, Window gnggaWindow)
    {
        gpsReaderService.OnGNGGAUpdated += (sender, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                gnggaWindow.RemoveAll();
                int y = 0;

                // Format UTC DateTime if available
                string utcDisplay = e.GNGGAData.UTC ?? "N/A";
                if (e.GNGGAData.UTCDateTime.HasValue)
                {
                    var utcTime = e.GNGGAData.UTCDateTime.Value;
                    utcDisplay = $"{e.GNGGAData.UTC} ({utcTime.ToString("HH:mm:ss.ff", new CultureInfo("en-US"))})";
                }

                // Format checksum validation status
                string checksumDisplay = e.GNGGAData.Checksum ?? "N/A";
                if (!string.IsNullOrEmpty(e.GNGGAData.Checksum))
                {
                    checksumDisplay = $"{e.GNGGAData.Checksum} ({(e.GNGGAData.ChecksumValid ? "Valid" : "Invalid")})";
                }

                gnggaWindow.Add(
                    new Label($"Raw Data: {e.RawData}") { Y = y++ },
                    new Label($"UTC: {utcDisplay}") { Y = y++ },
                    new Label($"Latitude: {e.GNGGAData.Latitude}") { Y = y++ },
                    new Label($"Longitude: {e.GNGGAData.Longitude}") { Y = y++ },
                    new Label($"Quality: {e.GNGGAData.Quality}") { Y = y++ },
                    new Label($"Satellites: {e.GNGGAData.Satellites}") { Y = y++ },
                    new Label($"HDOP: {e.GNGGAData.HDOP}") { Y = y++ },
                    new Label($"Altitude: {e.GNGGAData.Altitude}{e.GNGGAData.AltitudeUnits}") { Y = y++ },
                    new Label($"GeoidHeight: {e.GNGGAData.GeoidHeight}{e.GNGGAData.GeoidHeightUnits}") { Y = y++ },
                    new Label($"DGPSDataAge: {e.GNGGAData.DGPSDataAge}") { Y = y++ },
                    new Label($"DGPSStationID: {e.GNGGAData.DGPSStationID}") { Y = y++ },
                    new Label($"Checksum: {checksumDisplay}") { Y = y++ }
                );
            });
        };
    }
}
