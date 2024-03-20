using GPSReader;
using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static void OnGPGSVListUpdated(GPSReaderService gpsReaderService, Window gpgsvWindow)
    {
        gpsReaderService.OnGPGSVListUpdated += (sender, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                gpgsvWindow.RemoveAll();
                int y = 0;

                foreach (var d in e.GPGSVListData)
                {
                    gpgsvWindow.Add(
                        new Label($"ID: {d.MessageNumber}/{e.GPGSVListData.Count} Number: {d.SatellitesInView} Checksum: {d.Checksum}") { Y = y++ }
                    );

                    foreach (var s in d.Satellites!)
                    {
                        gpgsvWindow.Add(
                            new Label($"   {s.SatelliteNumber} : Elevation: {s.Elevation} Azimuth: {s.Azimuth} SNR: {s.SNR}") { Y = y++ }
                        );
                        
                    }
                }
            });
        };
    }
}