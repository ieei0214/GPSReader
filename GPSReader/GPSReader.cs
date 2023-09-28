using GPSReader.Parsers;
using Microsoft.Extensions.Logging;

namespace GPSReader;

public class GPSReaderService
{
    private INMEAInput _input;
    private List<INMEAParser> _parsers;
    private readonly ILogger<GPSReaderService> _logger;

    public event EventHandler<GPGGAEventArgs> OnGPGGAUpdated;

    public GPSReaderService(ILogger<GPSReaderService> logger, INMEAInput input, IEnumerable<INMEAParser> parsers)
    {
        _logger = logger;
        _input = input;
        _parsers = new List<INMEAParser>(parsers);
    }

    public GPSReaderService(ILogger<GPSReaderService> logger, INMEAInput input) : this(logger, input,
        new List<INMEAParser> { new GPGGAParser() })
    {
    }

    public void StartReading()
    {
        try
        {
            _input.Open();
            _input.DataReceived += DataReceived;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start reading");
            throw new GPSException("Failed to start reading: " + ex.Message);
        }
    }

    public void StopReading()
    {
        if (_input.IsOpen)
        {
            _input.Close();
        }
    }

    private void DataReceived(object sender, InputReceivedEventArgs e)
    {
        string data = e.Data;
        string[] sentences = data.Split('\n');

        foreach (string sentence in sentences)
        {
            foreach (var parser in _parsers)
            {
                if (parser.TryParse(sentence, out NMEAEventArgs eventArgs))
                {
                    switch (eventArgs)
                    {
                        case GPGGAEventArgs gpggaEventArgs:
                            OnGPGGAUpdated?.Invoke(this, gpggaEventArgs);
                            break;
                    }
                }
            }
        }
    }
}