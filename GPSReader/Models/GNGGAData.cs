namespace GPSReader.Models;

/// <summary>
/// Represents parsed GNGGA (Global Navigation Satellite System GGA) sentence data.
/// GNGGA provides combined positioning data from multiple satellite constellations (GPS + GLONASS, GPS + Galileo, etc.).
/// </summary>
public class GNGGAData : NMEAData
{
    /// <summary>
    /// Original UTC time string from NMEA sentence (hhmmss or hhmmss.ss format).
    /// </summary>
    public string? UTC { get; set; }

    /// <summary>
    /// Parsed UTC time as DateTime object (date portion is arbitrary, only time is meaningful).
    /// </summary>
    public DateTime? UTCDateTime { get; set; }

    /// <summary>
    /// Latitude in decimal degrees (converted from NMEA degrees-minutes format).
    /// </summary>
    public string? Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees (converted from NMEA degrees-minutes format).
    /// </summary>
    public string? Longitude { get; set; }

    /// <summary>
    /// GPS fix quality indicator (0=Invalid, 1=GPS, 2=DGPS, 3=PPS, 4-9=Other).
    /// </summary>
    public int? Quality { get; set; }

    /// <summary>
    /// Number of satellites in use.
    /// </summary>
    public int? Satellites { get; set; }

    /// <summary>
    /// Horizontal Dilution of Precision.
    /// </summary>
    public double? HDOP { get; set; }

    /// <summary>
    /// Altitude above mean sea level.
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// Unit of altitude measurement (typically "M" for meters).
    /// </summary>
    public string? AltitudeUnits { get; set; }

    /// <summary>
    /// Geoid height (separation between geoid and WGS84 ellipsoid).
    /// </summary>
    public double? GeoidHeight { get; set; }

    /// <summary>
    /// Unit of geoid height measurement (typically "M" for meters).
    /// </summary>
    public string? GeoidHeightUnits { get; set; }

    /// <summary>
    /// Age of DGPS data in seconds (empty if not using DGPS).
    /// </summary>
    public string? DGPSDataAge { get; set; }

    /// <summary>
    /// DGPS station ID (empty or 4-digit code if using DGPS).
    /// </summary>
    public string? DGPSStationID { get; set; }

    /// <summary>
    /// Indicates whether the received checksum matches the calculated checksum.
    /// </summary>
    public bool ChecksumValid { get; set; }

    /// <summary>
    /// Creates a GNGGAData instance from parsed NMEA sentence fields.
    /// </summary>
    /// <param name="fields">Array of NMEA sentence fields (including sentence ID).</param>
    /// <returns>A new GNGGAData instance with parsed field values.</returns>
    public static GNGGAData CreateFromFields(string[] fields)
    {
        var data = new GNGGAData
        {
            UTC = IsFieldNotEmpty(fields[1]) ? fields[1] : null,
            UTCDateTime = IsFieldNotEmpty(fields[1]) ? ParseUTCDateTime(fields[1]) : null,
            Latitude = IsFieldNotEmpty(fields[2]) ? ConvertToDecimalDegrees(fields[2], fields[3]) : null,
            Longitude = IsFieldNotEmpty(fields[4]) ? ConvertToDecimalDegrees(fields[4], fields[5]) : null,
            Quality = IsFieldNotEmpty(fields[6]) ? int.Parse(fields[6]) : null,
            Satellites = IsFieldNotEmpty(fields[7]) ? int.Parse(fields[7]) : null,
            HDOP = IsFieldNotEmpty(fields[8]) ? double.Parse(fields[8]) : null,
            Altitude = IsFieldNotEmpty(fields[9]) ? double.Parse(fields[9]) : null,
            AltitudeUnits = IsFieldNotEmpty(fields[10]) ? fields[10] : null,
            GeoidHeight = IsFieldNotEmpty(fields[11]) ? double.Parse(fields[11]) : null,
            GeoidHeightUnits = IsFieldNotEmpty(fields[12]) ? fields[12] : null,
            DGPSDataAge = IsFieldNotEmpty(fields[13]) ? fields[13] : null,
            DGPSStationID = fields.Length > 14 && IsFieldNotEmpty(fields[14]) ? fields[14] : null
        };
        return data;
    }

    /// <summary>
    /// Converts NMEA coordinate format (ddmm.mmmmm) to decimal degrees.
    /// </summary>
    /// <param name="degreesMinutes">Coordinate in NMEA format (e.g., "3400.97900" or "11739.17621").</param>
    /// <param name="direction">Hemisphere direction ("N", "S", "E", or "W").</param>
    /// <returns>Coordinate in decimal degrees format as string.</returns>
    private static string ConvertToDecimalDegrees(string degreesMinutes, string direction)
    {
        var parts = degreesMinutes.Split('.');
        var degrees = int.Parse(parts[0].Substring(0, parts[0].Length - 2));
        var minutes = double.Parse(parts[0].Substring(parts[0].Length - 2) + "." + parts[1]);
        var decimalDegrees = degrees + minutes / 60;
        if (direction == "S" || direction == "W")
            decimalDegrees = -decimalDegrees;
        return decimalDegrees.ToString();
    }

    /// <summary>
    /// Parses UTC time string from NMEA sentence to DateTime object.
    /// </summary>
    /// <param name="utcString">UTC time string in NMEA format (e.g., "211921.00" or "123519").</param>
    /// <returns>DateTime object with parsed time (date portion is arbitrary, only time is meaningful).</returns>
    private static DateTime? ParseUTCDateTime(string utcString)
    {
        try
        {
            // Try parsing 8-digit format with fractional seconds (hhmmss.ss)
            if (utcString.Contains('.'))
            {
                return DateTime.ParseExact(utcString, "HHmmss.ff", System.Globalization.CultureInfo.InvariantCulture);
            }
            // Try parsing 6-digit format without fractional seconds (hhmmss)
            else
            {
                return DateTime.ParseExact(utcString, "HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        catch
        {
            // Return null if parsing fails
            return null;
        }
    }
}
