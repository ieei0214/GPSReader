using GPSReader.Models;
using GPSReader.Parsers;
using Microsoft.Extensions.Logging;

namespace GPSReader;

public class GPSReaderService
{
    private INMEAInput _input;
    private List<BaseNMEAParser> _parsers;
    private readonly ILogger<GPSReaderService> _logger;

    private List<GPGSVData> _listGPGSVDatas = new List<GPGSVData>();

    public event EventHandler<GPGGAEventArgs>? OnGPGGAUpdated;
    public event EventHandler<GPGSAEventArgs>? OnGPGSAUpdated;
    public event EventHandler<GPGLLEventArgs>? OnGPGLLUpdated;
    public event EventHandler<GPGSVEventArgs>? OnGPGSVUpdated;
    public event EventHandler<GPGSVListEventArgs>? OnGPGSVListUpdated;

    public GPSReaderService(ILogger<GPSReaderService> logger, INMEAInput input, List<BaseNMEAParser> parsers)
    {
        _logger = logger;
        _input = input;
        _parsers = parsers;
    }

    public GPSReaderService(ILogger<GPSReaderService> logger, INMEAInput input)
        : this(logger, input, new List<BaseNMEAParser>
        {
            new GPGGAParser(),
            new GPGSAParser(),
            new GPGLLParser(),
            new GPGSVParser()
        })
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

    private void DataReceived(object? sender, InputReceivedEventArgs e)
    {
        string? data = e.Data;
        _logger.LogDebug(data);
        string[] sentences = data!.Split('\n');

        foreach (string sentence in sentences)
        {
            foreach (var parser in _parsers)
            {
                if (parser.TryParse(sentence, out NMEAEventArgs? eventArgs))
                {
                    if (eventArgs == null)
                        continue;
                    switch (eventArgs)
                    {
                        case GPGGAEventArgs gpggaEventArgs:
                            OnGPGGAUpdated?.Invoke(this, gpggaEventArgs);
                            break;
                        case GPGSAEventArgs gpgsaEventArgs:
                            OnGPGSAUpdated?.Invoke(this, gpgsaEventArgs);
                            break;
                        case GPGLLEventArgs gpgllEventArgs:
                            OnGPGLLUpdated?.Invoke(this, gpgllEventArgs);
                            break;
                        case GPGSVEventArgs gpgsvEventArgs:
                            _listGPGSVDatas.Add(gpgsvEventArgs.GPGSVData);
                            OnGPGSVUpdated?.Invoke(this, gpgsvEventArgs);
                            if (gpgsvEventArgs.GPGSVData.DataCount != null &&
                                int.TryParse(gpgsvEventArgs.GPGSVData.DataCount, out int dataCount) &&
                                _listGPGSVDatas.Count == dataCount)
                            {
                                OnGPGSVListUpdated?.Invoke(this,
                                    new GPGSVListEventArgs(new List<GPGSVData>(_listGPGSVDatas)));
                                _listGPGSVDatas.Clear();
                            }
                            break;
                    }
                }
            }
        }
    }
}