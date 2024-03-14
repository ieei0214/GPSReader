using GPSReader.Models;

namespace GPSReader.EventArgs;

public class GPGSAEventArgs : NMEAEventArgs
{
    public GPGSAData GPGSAData { get; }
    public GPGSAEventArgs(string rawData, GPGSAData gpgsaData) : base(rawData) => GPGSAData = gpgsaData;
}