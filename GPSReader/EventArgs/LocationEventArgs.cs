namespace GPSReader.EventArgs;

public class LocationEventArgs : EventArgs
{
    public string Latitude { get; }
    public string Longitude { get; }

    public LocationEventArgs(string latitude, string longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}