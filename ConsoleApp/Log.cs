using GPSReader;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ConsoleApp;

internal partial class Program
{
    private static ILogger<GPSReaderService> CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day,
                outputTemplate: "{Message:lj}{NewLine}")
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddSerilog(); });
        ILogger<GPSReaderService> logger = loggerFactory.CreateLogger<GPSReaderService>();
        return logger;
    }
}