using GPSReader;
using GPSReader.Interfaces;
using Terminal.Gui;

namespace ConsoleApp
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            // show prompt to select input type
            // 1. Serial Port
            // 2. File
            Console.WriteLine("Select input type: \n1. Serial Port \n2. File(example.txt for simulation)");
            var inputType = Console.ReadLine();
            
            Application.Init();
            var (inputWindow, gpggaWindow, gpgsaWindow, gpgllWindow, gpgsvWindow) = CreateWindows();

            var logger = CreateLogger();

            INMEAInput? input;

            if (inputType == "1")
            {
                input = new SerialInput("COM7", 115200);
            }
            else
            {
                input = new FileInput(@"example.txt");
            }
            
            var gpsReaderService = new GPSReaderService(logger, input);
            
            OnDataReceived(input, inputWindow);
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