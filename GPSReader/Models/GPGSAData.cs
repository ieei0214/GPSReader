namespace GPSReader.Models;


public class GPGSAData : NMEAData
{
    public string? Mode { get; }
    public string? FixStatus { get; }
    public List<string>? Satellites { get; }
    public string? PDOP { get; }
    public string? HDOP { get; }
    public string VDOP { get; }

    public GPGSAData(string mode, string fixStatus, List<string> satellites, string pdop, string hdop,
        string vdop)
    {
        Mode = mode;
        FixStatus = fixStatus;
        Satellites = satellites;
        PDOP = pdop;
        HDOP = hdop;
        VDOP = vdop;
    }

    public static GPGSAData CreateFromFields(string[] fields)
    {
        var mode = fields[1];
        var fixStatus = fields[2];
        var satellites = new List<string>();
        for (int i = 0; i < 12; i++)
        {
            if (fields[i + 3] != "")                        {
                satellites.Add(fields[i + 3]);
            }
        }
        var pdop = fields[15];
        var hdop = fields[16];
        var vdop = fields[17];
        return new GPGSAData(mode, fixStatus, satellites, pdop, hdop, vdop);
    }
}