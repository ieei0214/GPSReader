namespace GPSReader.Models;

public class GPGLLData : NMEAData
{
    public string? Latitude { get; set; }
    public string? LatitudeHemisphere { get; set; }
    public string? Longitude { get; set; }
    public string? LongitudeHemisphere { get; set; }
    public string? UTC { get; set; }
    public string? PositionStatus { get; set; }
    public string? ModeIndicator { get; set; }
    
    public static GPGLLData CreateFromFields(string[] fields)
    {
        var data = new GPGLLData
        {
            Latitude = IsFieldNotEmpty(fields[1]) ? fields[1] : null,
            LatitudeHemisphere = IsFieldNotEmpty(fields[2]) ? fields[2] : null,
            Longitude = IsFieldNotEmpty(fields[3]) ? fields[3] : null,
            LongitudeHemisphere = IsFieldNotEmpty(fields[4]) ? fields[4] : null,
            UTC = IsFieldNotEmpty(fields[5]) ? fields[5] : null,
            PositionStatus = IsFieldNotEmpty(fields[6]) ? fields[6] : null,
            ModeIndicator = IsFieldNotEmpty(fields[7]) ? fields[7] : null
        };

        return data;
    }
    
}