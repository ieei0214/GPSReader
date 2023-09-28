namespace GPSReader.Models;

public class GPGGAData : NMEAData
{
    public string? UTC { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public int? Quality { get; set; }
    public int? Satellites { get; set; }
    public double? HDOP { get; set; }
    public double? Altitude { get; set; }
    public string? AltitudeUnits { get; set; }
    public double? GeoidHeight { get; set; }
    public string? GeoidHeightUnits { get; set; }
    public string? DGPSDataAge { get; set; }
    public string? Checksum { get; set; }

    public static GPGGAData CreateFromFields(string[] fields)
    {
        var data = new GPGGAData
        {
            UTC = IsFieldNotEmpty(fields[1]) ? fields[1] : null,
            Latitude = IsFieldNotEmpty(fields[2]) ? ConvertToDecimalDegrees(fields[2], fields[3]) : null,
            Longitude = IsFieldNotEmpty(fields[4]) ? ConvertToDecimalDegrees(fields[4], fields[5]) : null,
            Quality = IsFieldNotEmpty(fields[6]) ? int.Parse(fields[6]) : null,
            Satellites = IsFieldNotEmpty(fields[7]) ? int.Parse(fields[7]) : null,
            HDOP = IsFieldNotEmpty(fields[8]) ? double.Parse(fields[8]) : null,
            Altitude = IsFieldNotEmpty(fields[9]) ? double.Parse(fields[9]) : null,
            AltitudeUnits = IsFieldNotEmpty(fields[10]) ? fields[10] : null,
            GeoidHeight = IsFieldNotEmpty(fields[11]) ? double.Parse(fields[11]) : null,
            GeoidHeightUnits = IsFieldNotEmpty(fields[12]) ? fields[12] : null,
            DGPSDataAge = IsFieldNotEmpty(fields[13]) ? fields[13]  : null,
            Checksum = IsFieldNotEmpty(fields[14]) ? fields[14] : null
        };
        return data;
    }

    private static bool IsFieldNotEmpty(string field)
    {
        return !string.IsNullOrEmpty(field);
    }

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
}