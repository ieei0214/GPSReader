using System.Globalization;
using GPSReader;
using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static void OnGPGLLUpdated(GPSReaderService gpsReaderService, Window gpgllWindow)
    {
        gpsReaderService.OnGPGLLUpdated += (sender, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                gpgllWindow.RemoveAll();
                int y = 0;
                var utcTime = DateTime.ParseExact(e.GPGLLData.UTC!, "HHmmss.ff", CultureInfo.InvariantCulture);

                gpgllWindow.Add(
                    new Label($"Raw Data: {e.RawData}") { Y = y++ },
                    new Label($"Latitude: {e.GPGLLData.Latitude}") { Y = y++ },
                    new Label($"Longitude: {e.GPGLLData.Longitude}") { Y = y++ },
                    new Label($"UTC: {e.GPGLLData.UTC} {utcTime.ToString()}") { Y = y++ },
                    new Label($"PositionStatus: {e.GPGLLData.PositionStatus}") { Y = y++ },
                    new Label($"ModeIndicator: {e.GPGLLData.ModeIndicator}") { Y = y++ },
                    new Label($"Checksum: {e.GPGLLData.Checksum}") { Y = y++ }
                );
            });
        };
    }
}