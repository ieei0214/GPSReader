namespace GPSReader.EventArgs;

public class InputReceivedEventArgs : System.EventArgs
{
    public string? Data { get; }

    public InputReceivedEventArgs(string? data)
    {
        Data = data;
    }
}