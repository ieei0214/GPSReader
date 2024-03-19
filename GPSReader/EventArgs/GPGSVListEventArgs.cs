using GPSReader.Models;

namespace GPSReader;

public class GPGSVListEventArgs
{
    public List<GPGSVData> GPGSVListData { get; }
    public GPGSVListEventArgs(List<GPGSVData> data) => GPGSVListData = data;
}