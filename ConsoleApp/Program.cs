using System.Globalization;
using Microsoft.Extensions.Logging;
using GPSReader;
using GPSReader.Interfaces;
using GPSReader.Parsers;
using Serilog;
using Serilog.Core;
using Terminal.Gui;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            var (gpggaWindow, gpgsaWindow, gpgllWindow) = CreateWindows();

            var logger = CreateLogger();

            var gpsReaderService = new GPSReaderService(logger, new SerialInput());

            gpsReaderService.OnGPGGAUpdated += (sender, e) =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    gpggaWindow.RemoveAll();
                    int y = 0;
                    var utcTime = DateTime.ParseExact(e.GPGGAData.UTC, "HHmmss.ff", CultureInfo.InvariantCulture);
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

            gpsReaderService.OnGPGLLUpdated += (sender, e) =>
            {
                Application.MainLoop.Invoke(() =>
                {
                    gpgllWindow.RemoveAll();
                    int y = 0;
                    var utcTime = DateTime.ParseExact(e.GPGLLData.UTC, "HHmmss.ff", CultureInfo.InvariantCulture);

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

            gpsReaderService.StartReading();

            Application.Run();
            Application.Shutdown();

            gpsReaderService.StopReading();
        }

        private static ILogger<GPSReaderService> CreateLogger()
        {
            // Dont show the header like "2024-03-18 13:59:51.378 -07:00 [DBG]" in front of the log message
            // try to show only the log message and config the Log
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Message:lj}{NewLine}")
                .CreateLogger();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
                //builder.AddConfiguration(configuration.GetSection("Logging")).AddDebug().AddConsole();
            });
            ILogger<GPSReaderService> logger = loggerFactory.CreateLogger<GPSReaderService>();
            return logger;
        }

        private static (Window gpggaWindow, Window gpgsaWindow, Window gpgllWindow) CreateWindows()
        {
            var top = Application.Top;

            var gpggaWindow = new Window("GPGGA Data")
                { X = 0, Y = 1, Width = Dim.Fill(), Height = Dim.Percent(33) };
            var gpgsaWindow = new Window("GPGSA Data")
                { X = 0, Y = Pos.Bottom(gpggaWindow), Width = Dim.Fill(), Height = Dim.Percent(33) };
            var gpgllWindow = new Window("GPGLL Data")
                { X = 0, Y = Pos.Bottom(gpgsaWindow), Width = Dim.Fill(), Height = Dim.Percent(33) };

            var menu = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("_File", new MenuItem[]
                {
                    new MenuItem("_Quit", "", () => { Application.RequestStop(); })
                }),
            });

            top.Add(menu, gpggaWindow, gpgsaWindow, gpgllWindow);
            return (gpggaWindow, gpgsaWindow, gpgllWindow);
        }
    }
}