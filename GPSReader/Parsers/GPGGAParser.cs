using GPSReader.EventArgs;
using GPSReader.Interfaces;
using GPSReader.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GPSReader.Parsers;

public class GPGGAParser : INMEAParser
{
    public string SentenceId => "GPGGA";

    public bool TryParse(string sentence, out NMEAEventArgs eventArgs)
    {
        if (sentence.StartsWith($"${SentenceId}"))
        {
            string[] fields = sentence.Split(',');

            if (fields.Length >= 15)
            {
                eventArgs = new GPGGAEventArgs(sentence, GPGGAData.CreateFromFields(fields));
                return true;
            }
            else
            {
                Debug.WriteLine("Insufficient fields in GPGGA sentence");
            }
        }

        eventArgs = null;
        Debug.WriteLine($"Failed to parse: {sentence}");
        return false;
    }
}