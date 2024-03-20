using GPSReader.EventArgs;
using GPSReader.Interfaces;
using GPSReader.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GPSReader.Parsers;

public class GPGLLParser : BaseNMEAParser
{
    public override string SentenceId => "GPGLL";

    public override bool TryParse(string sentence, out NMEAEventArgs? eventArgs)
    {
        if (sentence.StartsWith($"${SentenceId}"))
        {
            var (fields, checkSum) = GetFieldAndChecksum(sentence);

            // if (fields.Length >= 15)
            {
                var data = GPGLLData.CreateFromFields(fields);
                data.Checksum = checkSum;
                eventArgs = new GPGLLEventArgs(sentence, data);
                return true;
            }
        }

        eventArgs = null;
        Debug.WriteLine($"Failed to parse: {sentence}");
        return false;
    }
}