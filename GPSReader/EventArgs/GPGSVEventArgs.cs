using GPSReader.Models;

namespace GPSReader.Parsers;

public class GPGSVEventArgs : NMEAEventArgs
{
    public GPGSVData GPGSVData { get; }
    public GPGSVEventArgs(string rawData, GPGSVData gpgsvData) : base(rawData) => GPGSVData = gpgsvData;
}