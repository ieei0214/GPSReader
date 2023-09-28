namespace GPSReader.EventArgs;

public class NMEAEventArgs : System.EventArgs
{
    public string RawData { get; }
    public NMEAEventArgs(string rawData) => RawData = rawData;
}