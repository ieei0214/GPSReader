using GPSReader.Interfaces;

namespace GPSReader.Parsers;

public class GPGGAParser : INMEAParser
{
    public bool TryParse(string sentence, out (string Latitude, string Longitude) location)
    {
        if (sentence.StartsWith("$GPGGA"))
        {
            string[] fields = sentence.Split(',');

            if (fields.Length > 6 && !string.IsNullOrEmpty(fields[3]) && !string.IsNullOrEmpty(fields[5]))
            {
                location = (fields[3] + " " + fields[4], fields[5] + " " + fields[6]);
                return true;
            }
        }

        location = (null, null);
        return false;
    }
}