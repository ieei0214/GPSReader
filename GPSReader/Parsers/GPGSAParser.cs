using GPSReader.EventArgs;
using GPSReader.Interfaces;
using GPSReader.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GPSReader.Parsers;

public class GPGSAParser : INMEAParser
{
    public string SentenceId => "GPGSA";

    public bool TryParse(string sentence, out NMEAEventArgs eventArgs)
    {
        if (sentence.StartsWith($"${SentenceId}"))
        {
            var value = sentence.Split('*');
            string[] fields = value[0].Split(',');
            string checkSum = value[1];

            if (fields.Length >= 15)
            {
                var data = GPGSAData.CreateFromFields(fields);
                data.Checksum = checkSum;
                eventArgs = new GPGSAEventArgs(sentence, data);
                return true;
            }
            else
            {
                Debug.WriteLine($"Insufficient fields in {SentenceId} sentence");
            }
        }

        eventArgs = null;
        Debug.WriteLine($"Failed to parse: {sentence}");
        return false;
    }
}