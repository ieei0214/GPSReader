namespace GPSReader.Models;

public class NMEAData
{
    public string Checksum { get; set; }

    protected static bool IsFieldNotEmpty(string field)
    {
        return !string.IsNullOrEmpty(field);
    }
}