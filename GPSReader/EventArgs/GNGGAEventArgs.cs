using GPSReader.Models;

namespace GPSReader.EventArgs;

/// <summary>
/// Event arguments raised when a GNGGA sentence is successfully parsed.
/// </summary>
public class GNGGAEventArgs : NMEAEventArgs
{
    /// <summary>
    /// Gets the parsed GNGGA sentence data.
    /// </summary>
    public GNGGAData GNGGAData { get; }

    /// <summary>
    /// Initializes a new instance of the GNGGAEventArgs class.
    /// </summary>
    /// <param name="rawData">The original unparsed NMEA sentence.</param>
    /// <param name="data">The parsed GNGGA sentence data.</param>
    public GNGGAEventArgs(string rawData, GNGGAData data) : base(rawData)
    {
        GNGGAData = data;
    }
}
