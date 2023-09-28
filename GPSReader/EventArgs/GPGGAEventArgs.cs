using GPSReader.Models;

namespace GPSReader.EventArgs;

public class GPGGAEventArgs : NMEAEventArgs
{
    public GPGGAData GPGGAData { get; }
    public GPGGAEventArgs(string rawData, GPGGAData gpggaData) : base(rawData) => GPGGAData = gpggaData;
}