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

            var logger = CreateLogger();

            INMEAInput? input;

            if (inputType == "1")
            {
                // Get all available serial ports and let the user select one
                var ports = System.IO.Ports.SerialPort.GetPortNames();
                Console.WriteLine("Select a serial port:");
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {ports[i]}");
                }

                var p = Console.ReadLine();
                input = new SerialInput(ports[int.Parse(p) - 1], 115200);
            }
            else
            {
                input = new FileInput(@"example.txt");
            }
            var gpsReaderService = new GPSReaderService(logger, input);
            
            Application.Init();
            var (inputWindow, gpggaWindow, gnggaWindow, gpgsaWindow, gpgllWindow, gpgsvWindow) = CreateWindows();

            OnDataReceived(input, inputWindow);
            OnGPGGAUpdated(gpsReaderService, gpggaWindow);
            OnGNGGAUpdated(gpsReaderService, gnggaWindow);
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