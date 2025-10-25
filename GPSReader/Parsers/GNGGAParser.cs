using GPSReader.EventArgs;
using GPSReader.Interfaces;
using GPSReader.Models;
using System.Diagnostics;
using System.Globalization;

namespace GPSReader.Parsers;

/// <summary>
/// Parser for GNGGA (Global Navigation Satellite System GGA) NMEA sentences.
/// GNGGA sentences provide combined positioning data from multiple satellite constellations.
/// </summary>
public class GNGGAParser : BaseNMEAParser
{
    /// <summary>
    /// Gets the NMEA sentence identifier for GNGGA sentences.
    /// </summary>
    public override string SentenceId => "GNGGA";

    /// <summary>
    /// Attempts to parse a GNGGA NMEA sentence.
    /// </summary>
    /// <param name="sentence">The NMEA sentence to parse.</param>
    /// <param name="eventArgs">The parsed event arguments if successful, null otherwise.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public override bool TryParse(string sentence, out NMEAEventArgs? eventArgs)
    {
        if (sentence.StartsWith($"${SentenceId}"))
        {
            // Check if sentence has checksum delimiter
            if (!sentence.Contains('*'))
            {
                Debug.WriteLine($"Missing checksum delimiter in {SentenceId} sentence");
                eventArgs = null;
                return false;
            }

            var (fields, checkSum) = GetFieldAndChecksum(sentence);

            if (fields.Length >= 15)
            {
                try
                {
                    var data = GNGGAData.CreateFromFields(fields);
                    data.Checksum = checkSum;

                    // Checksum validation
                    var calculatedChecksum = CalculateChecksum(sentence);
                    data.ChecksumValid = calculatedChecksum.Equals(checkSum, StringComparison.OrdinalIgnoreCase);

                    if (!data.ChecksumValid)
                    {
                        Debug.WriteLine($"Checksum mismatch for {SentenceId} sentence. Expected: {calculatedChecksum}, Received: {checkSum}");
                    }

                    // Semantic validation warnings
                    if (data.Latitude != null && double.TryParse(data.Latitude, CultureInfo.InvariantCulture, out double lat))
                    {
                        if (lat < -90 || lat > 90)
                        {
                            Debug.WriteLine($"Latitude out of valid range [-90, 90]: {lat}");
                        }
                    }

                    if (data.Longitude != null && double.TryParse(data.Longitude, CultureInfo.InvariantCulture, out double lon))
                    {
                        if (lon < -180 || lon > 180)
                        {
                            Debug.WriteLine($"Longitude out of valid range [-180, 180]: {lon}");
                        }
                    }

                    if (data.Quality.HasValue && data.Quality.Value > 9)
                    {
                        Debug.WriteLine($"Quality out of valid range [0, 9]: {data.Quality}");
                    }

                    if (data.Satellites.HasValue && data.Satellites.Value > 30)
                    {
                        Debug.WriteLine($"Satellites count unusually high (>30): {data.Satellites}");
                    }

                    if (data.HDOP.HasValue && data.HDOP.Value > 100)
                    {
                        Debug.WriteLine($"HDOP unusually high (>100): {data.HDOP}");
                    }

                    eventArgs = new GNGGAEventArgs(sentence, data);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing {SentenceId} sentence: {ex.Message}");
                    eventArgs = null;
                    return false;
                }
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

    /// <summary>
    /// Calculates the NMEA checksum for a sentence.
    /// The checksum is an XOR of all characters between '$' and '*'.
    /// </summary>
    /// <param name="sentence">The NMEA sentence.</param>
    /// <returns>The calculated checksum as a hexadecimal string.</returns>
    private string CalculateChecksum(string sentence)
    {
        int startIndex = sentence.IndexOf('$');
        int endIndex = sentence.IndexOf('*');

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
        {
            return string.Empty;
        }

        byte checksum = 0;
        for (int i = startIndex + 1; i < endIndex; i++)
        {
            checksum ^= (byte)sentence[i];
        }

        return checksum.ToString("X2");
    }
}
