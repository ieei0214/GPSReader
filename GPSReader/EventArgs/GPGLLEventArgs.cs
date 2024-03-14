using GPSReader.Models;

namespace GPSReader.Parsers;

public class GPGLLEventArgs : NMEAEventArgs
{
    public GPGLLData GPGLLData { get; }
    public GPGLLEventArgs(string rawData, GPGLLData gpggaData) : base(rawData) => GPGLLData = gpggaData;
}