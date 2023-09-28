using GPSReader.EventArgs;
using GPSReader.Interfaces;

namespace GPSReader.Tests;

public class MockInputSource : INMEAInput
{
    public event EventHandler<InputReceivedEventArgs>? DataReceived;

    public void Open()
    {
    }

    public void Close()
    {
    }

    public bool IsOpen { get; private set; } // Add this line

    public void SimulateDataReceived(string data)
    {
        DataReceived?.Invoke(this, new InputReceivedEventArgs(data));
    }
}