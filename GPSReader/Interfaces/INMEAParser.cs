namespace GPSReader.Interfaces;

public interface INMEAParser
{
    bool TryParse(string sentence, out (string Latitude, string Longitude) location);
}