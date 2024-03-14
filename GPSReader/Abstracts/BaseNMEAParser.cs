using GPSReader.EventArgs;
using GPSReader.Models;

namespace GPSReader.Interfaces;

public abstract class BaseNMEAParser
{
    public abstract string SentenceId { get; }
    public abstract bool TryParse(string sentence, out NMEAEventArgs eventArgs);

    protected (string[] fields, string checkSum) GetFieldAndChecksum(string sentence)
    {
        var value = sentence.Split('*');
        string[] fields = value[0].Split(',');
        string checkSum = value[1];
        return (fields, checkSum);
    }
}