using GPSReader.EventArgs;
using GPSReader.Interfaces;
using GPSReader.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GPSReader.Parsers;

public class GPGSVParser : BaseNMEAParser
{
    public override string SentenceId => "GPGSV";

    public override bool TryParse(string sentence, out NMEAEventArgs? eventArgs)
    {
        if (sentence.StartsWith($"${SentenceId}"))
        {
            var (fields, checkSum) = GetFieldAndChecksum(sentence);

            var data = GPGSVData.CreateFromFields(fields);
            data.Checksum = checkSum;
            eventArgs = new GPGSVEventArgs(sentence, data);
            return true;
        }

        eventArgs = null;
        Debug.WriteLine($"Failed to parse: {sentence}");
        return false;
    }

}