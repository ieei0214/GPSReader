using Microsoft.Extensions.Logging;
using GPSReader;
using Serilog;
using Terminal.Gui;

namespace ConsoleApp
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            var (inputWindow, gpggaWindow, gpgsaWindow, gpgllWindow, gpgsvWindow) = CreateWindows();

            var logger = CreateLogger();

            var serialInput = new SerialInput();
            var gpsReaderService = new GPSReaderService(logger, serialInput);

            OnSerialDataReceived(serialInput, inputWindow);
            OnGPGGAUpdated(gpsReaderService, gpggaWindow);
            OnGPGSAUpdated(gpsReaderService, gpgsaWindow);
            OnGPGLLUpdated(gpsReaderService, gpgllWindow);
            OnGPGSVListUpdated(gpsReaderService, gpgsvWindow);

            gpsReaderService.StartReading();

            Application.Run();
            Application.Shutdown();

            gpsReaderService.StopReading();
        }
    }
}