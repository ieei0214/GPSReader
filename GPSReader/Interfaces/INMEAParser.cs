using GPSReader.EventArgs;
using GPSReader.Models;

namespace GPSReader.Interfaces;

public interface INMEAParser
{
    bool TryParse(string sentence, out NMEAEventArgs eventArgs);
    string SentenceId { get; }  
}