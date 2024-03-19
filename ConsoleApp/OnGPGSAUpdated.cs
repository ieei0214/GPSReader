using GPSReader;
using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static void OnGPGSAUpdated(GPSReaderService gpsReaderService, Window gpgsaWindow)
    {
        gpsReaderService.OnGPGSAUpdated += (sender, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                gpgsaWindow.RemoveAll();
                int y = 0;
                // convert the list of satellites to a string
                var satellites = string.Join(", ", e.GPGSAData.Satellites);
                gpgsaWindow.Add(
                    new Label($"Raw Data: {e.RawData}") { Y = y++ },
                    new Label($"Mode: {e.GPGSAData.Mode}") { Y = y++ },
                    new Label($"FixType: {e.GPGSAData.FixStatus}") { Y = y++ },
                    new Label($"Satellites: {satellites}") { Y = y++ },
                    new Label($"PDOP: {e.GPGSAData.PDOP}") { Y = y++ },
                    new Label($"HDOP: {e.GPGSAData.HDOP}") { Y = y++ },
                    new Label($"VDOP: {e.GPGSAData.VDOP}") { Y = y++ },
                    new Label($"Checksum: {e.GPGSAData.Checksum}") { Y = y++ }
                );
            });
        };
    }
}