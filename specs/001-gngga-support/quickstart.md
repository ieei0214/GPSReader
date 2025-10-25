# Quickstart: GNGGA NMEA Sentence Support

**Feature**: 001-gngga-support | **Date**: 2025-10-25
**Purpose**: Quick reference guide for developers implementing and using GNGGA parsing

## For Implementers

### Implementation Checklist

- [ ] **Step 1**: Create `GNGGAData.cs` in `GPSReader/Models/`
  - Inherit from `NMEAData`
  - Add all properties (UTC, UTCDateTime, Latitude, Longitude, Quality, Satellites, HDOP, Altitude, etc.)
  - Implement `CreateFromFields(string[] fields)` static factory method
  - Copy `ConvertToDecimalDegrees()` from GPGGAData.cs

- [ ] **Step 2**: Create `GNGGAEventArgs.cs` in `GPSReader/EventArgs/`
  - Inherit from `NMEAEventArgs`
  - Add `GNGGAData` property
  - Constructor: `public GNGGAEventArgs(string rawData, GNGGAData data) : base(rawData)`

- [ ] **Step 3**: Create `GNGGAParser.cs` in `GPSReader/Parsers/`
  - Inherit from `BaseNMEAParser`
  - Override `SentenceId` → return `"GNGGA"`
  - Override `TryParse(string sentence, out NMEAEventArgs? eventArgs)`
  - Validate structure, parse fields, compute checksum, create GNGGAEventArgs

- [ ] **Step 4**: Register parser in `GPSReaderService`
  - Add `public event EventHandler<GNGGAEventArgs>? OnGNGGAUpdated;` to GPSReader.cs
  - Instantiate `GNGGAParser` in constructor
  - Wire up parser to raise `OnGNGGAUpdated` event when TryParse succeeds

- [ ] **Step 5**: Add unit tests in `GPSReader.Tests/Tests.cs`
  - `Test_OnGNGGAUpdated` - event triggering
  - `Test_GNGGAEventArgs` - full field validation with two test sentences
  - `Test_GNGGAInvalidChecksum` - checksum validation behavior
  - `Test_GNGGAOutOfRangeValues` - semantic validation warnings

- [ ] **Step 6**: Create `OnGNGGAUpdated.cs` in `ConsoleApp/`
  - Implement event handler method
  - Create Terminal.Gui window with labels for all fields
  - Use `Application.MainLoop.Invoke()` for thread-safe UI updates
  - Format UTC DateTime with CultureInfo("en-US")

- [ ] **Step 7**: Update `ConsoleApp/Program.cs`
  - Create GNGGA window instance
  - Subscribe to `gpsReaderService.OnGNGGAUpdated` event
  - Call `OnGNGGAUpdated(gpsReaderService, gnggaWindow)` helper

- [ ] **Step 8**: Validate implementation
  - Run all unit tests → should pass
  - Run ConsoleApp with test file containing GNGGA sentences → window should display
  - Verify coordinate conversion accuracy (within 0.0001 degrees)
  - Verify checksum validation warnings logged but parsing continues

---

## For Library Users

### Basic Usage

```csharp
using GPSReader;
using GPSReader.Input;
using Microsoft.Extensions.Logging;

// 1. Setup logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<GPSReaderService>();

// 2. Create input source (serial port or file)
var input = new SerialInput("COM3", 9600, logger);  // or FileInput("gps_data.txt", logger)

// 3. Create GPS reader service
var gpsReader = new GPSReaderService(logger, input);

// 4. Subscribe to GNGGA events
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    var data = e.GNGGAData;

    Console.WriteLine($"GNGGA Sentence Received:");
    Console.WriteLine($"  Raw: {e.RawData}");
    Console.WriteLine($"  UTC: {data.UTC} ({data.UTCDateTime:HH:mm:ss.ff})");
    Console.WriteLine($"  Position: {data.Latitude}, {data.Longitude}");
    Console.WriteLine($"  Quality: {data.Quality} ({data.Satellites} satellites)");
    Console.WriteLine($"  Altitude: {data.Altitude}{data.AltitudeUnits}");
    Console.WriteLine($"  HDOP: {data.HDOP}");
    Console.WriteLine($"  Geoid Height: {data.GeoidHeight}{data.GeoidHeightUnits}");

    if (!data.ChecksumValid)
        Console.WriteLine($"  WARNING: Checksum invalid!");
};

// 5. Start reading
gpsReader.StartReading();

// Keep application running to receive events
Console.WriteLine("Listening for GNGGA sentences... Press Ctrl+C to exit");
Console.ReadLine();

// 6. Cleanup
gpsReader.StopReading();
```

### Handling Both GPGGA and GNGGA

```csharp
// Subscribe to both GPS-only (GPGGA) and multi-GNSS (GNGGA) events
gpsReader.OnGPGGAUpdated += (sender, e) =>
{
    Console.WriteLine($"[GPS-only] Position: {e.GPGGAData.Latitude}, {e.GPGGAData.Longitude}");
};

gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    Console.WriteLine($"[Multi-GNSS] Position: {e.GNGGAData.Latitude}, {e.GNGGAData.Longitude}");
};

// Modern GPS receivers may send both sentence types simultaneously
// This allows you to compare GPS-only vs multi-constellation positioning
```

### Filtering Valid Data

```csharp
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    var data = e.GNGGAData;

    // Only process data with valid checksums and good fix quality
    if (data.ChecksumValid && data.Quality >= 1)
    {
        // Safe to use for navigation
        UpdateMap(data.Latitude, data.Longitude);
    }
    else
    {
        // Checksum invalid or no fix - log for diagnostics but don't use
        Logger.LogWarning($"Skipping GNGGA data: Checksum={data.ChecksumValid}, Quality={data.Quality}");
    }
};
```

### Working with UTC Time

```csharp
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    var data = e.GNGGAData;

    // Access original NMEA string (for logging/debugging)
    string originalUtc = data.UTC;  // "211921.00"

    // Access parsed DateTime (for application logic)
    if (data.UTCDateTime.HasValue)
    {
        var timeOfDay = data.UTCDateTime.Value.TimeOfDay;  // 21:19:21.00

        // Combine with current date for full timestamp
        var timestamp = DateTime.UtcNow.Date + timeOfDay;

        // Store to database, compare times, etc.
        LogPosition(timestamp, data.Latitude, data.Longitude);
    }
};
```

### Complete Example with Error Handling

```csharp
using GPSReader;
using GPSReader.Input;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<GPSReaderService>();

        try
        {
            // Create input source with error handling
            var input = new SerialInput("COM3", 9600, logger);
            var gpsReader = new GPSReaderService(logger, input);

            // Subscribe to GNGGA events
            gpsReader.OnGNGGAUpdated += HandleGNGGASentence;

            // Start reading
            gpsReader.StartReading();
            logger.LogInformation("GPS reader started. Listening for GNGGA sentences...");

            // Wait for Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                gpsReader.StopReading();
                logger.LogInformation("GPS reader stopped");
            };

            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in GPS reader application");
        }
    }

    static void HandleGNGGASentence(object? sender, GNGGAEventArgs e)
    {
        var data = e.GNGGAData;

        // Validate data quality
        if (!data.ChecksumValid)
        {
            Console.WriteLine($"[WARNING] Checksum invalid: {e.RawData}");
            return;
        }

        if (data.Quality == 0)
        {
            Console.WriteLine($"[INFO] No GPS fix available");
            return;
        }

        // Process valid position data
        Console.WriteLine($"Position: {data.Latitude}, {data.Longitude}");
        Console.WriteLine($"Altitude: {data.Altitude}m (Geoid: {data.GeoidHeight}m)");
        Console.WriteLine($"Quality: {GetQualityDescription(data.Quality)} " +
                          $"({data.Satellites} satellites, HDOP {data.HDOP})");

        if (!string.IsNullOrEmpty(data.DGPSStationID))
        {
            Console.WriteLine($"DGPS Station: {data.DGPSStationID}");
        }
    }

    static string GetQualityDescription(int? quality) => quality switch
    {
        0 => "Invalid",
        1 => "GPS fix",
        2 => "DGPS fix",
        3 => "PPS fix",
        4 => "RTK",
        5 => "Float RTK",
        6 => "Estimated",
        _ => $"Unknown ({quality})"
    };
}
```

---

## Testing

### Running Unit Tests

```bash
# From repository root
dotnet test GPSReader.Tests/GPSReader.Tests.csproj

# Run specific GNGGA tests
dotnet test --filter "FullyQualifiedName~GNGGA"

# View detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Manual Testing with Test File

Create `test_gngga.txt` with sample NMEA sentences:
```
$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47
$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71
$GPGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*42
```

Run ConsoleApp:
```bash
dotnet run --project ConsoleApp/ConsoleApp.csproj
# Select FileInput mode
# Browse to test_gngga.txt
# Verify both GPGGA and GNGGA windows display data
```

### Expected Test Results

**Test Sentence 1** (simple format):
```
Input: $GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47

Expected Output:
  UTC: 123519 (12:35:19)
  Latitude: 48.1173 ±0.0001
  Longitude: 11.5221 ±0.0001
  Quality: 1
  Satellites: 8
  HDOP: 0.9
  Altitude: 545.4M
  GeoidHeight: 46.9M
  DGPSDataAge: (null)
  DGPSStationID: (null)
  ChecksumValid: true
```

**Test Sentence 2** (high-precision format):
```
Input: $GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71

Expected Output:
  UTC: 211921.00 (21:19:21.00)
  Latitude: 34.0163166 ±0.0001
  Longitude: -117.6529368 ±0.0001
  Quality: 2
  Satellites: 12
  HDOP: 0.86
  Altitude: 247.6M
  GeoidHeight: -32.2M
  DGPSDataAge: (null)
  DGPSStationID: 0000
  ChecksumValid: true
```

---

## Common Pitfalls

### ❌ Don't: Assume GNGGA and GPGGA are interchangeable

```csharp
// WRONG: Assuming GNGGA data is the same as GPGGA
gpsReader.OnGPGGAUpdated += (sender, e) =>
{
    // This won't receive GNGGA sentences!
    ProcessPosition(e.GPGGAData);
};
```

```csharp
// CORRECT: Subscribe to both events if you need all position data
gpsReader.OnGPGGAUpdated += (sender, e) => ProcessPosition(e.GPGGAData.Latitude, e.GPGGAData.Longitude);
gpsReader.OnGNGGAUpdated += (sender, e) => ProcessPosition(e.GNGGAData.Latitude, e.GNGGAData.Longitude);
```

### ❌ Don't: Ignore checksum validation status

```csharp
// WRONG: Using data without checking validity
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    SaveToDatabase(e.GNGGAData.Latitude, e.GNGGAData.Longitude);  // Might be corrupt!
};
```

```csharp
// CORRECT: Validate before using
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    if (e.GNGGAData.ChecksumValid && e.GNGGAData.Quality > 0)
    {
        SaveToDatabase(e.GNGGAData.Latitude, e.GNGGAData.Longitude);
    }
    else
    {
        Logger.LogWarning("Skipping invalid GNGGA data");
    }
};
```

### ❌ Don't: Block the event handler thread

```csharp
// WRONG: Long-running operation blocks parser
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    HttpClient.Post("https://api.example.com/gps", e.GNGGAData);  // Blocks!
};
```

```csharp
// CORRECT: Offload to background task
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    var data = e.GNGGAData;
    _ = Task.Run(() => HttpClient.Post("https://api.example.com/gps", data));
};
```

### ❌ Don't: Parse UTC string manually

```csharp
// WRONG: Manual parsing duplicates logic
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    var utcString = e.GNGGAData.UTC;  // "211921.00"
    var hours = int.Parse(utcString.Substring(0, 2));  // Reinventing the wheel
    var minutes = int.Parse(utcString.Substring(2, 2));
    var seconds = int.Parse(utcString.Substring(4, 2));
};
```

```csharp
// CORRECT: Use pre-parsed DateTime
gpsReader.OnGNGGAUpdated += (sender, e) =>
{
    var utcTime = e.GNGGAData.UTCDateTime;  // Already parsed!
    if (utcTime.HasValue)
    {
        var hours = utcTime.Value.Hour;
        var minutes = utcTime.Value.Minute;
        var seconds = utcTime.Value.Second;
    }
};
```

---

## Performance Tips

- **Event handlers should be fast**: Offload heavy processing (network calls, database writes, complex calculations) to background tasks
- **Reuse instances**: Create one `GPSReaderService` and keep it running; don't create/dispose for each sentence
- **Avoid boxing**: GNGGAData properties are structs where possible (int, double, DateTime) to minimize heap allocations
- **Batch logging**: If logging many positions, consider buffering and writing in batches to reduce I/O overhead

---

## Troubleshooting

| Problem | Likely Cause | Solution |
|---------|-------------|----------|
| OnGNGGAUpdated never fires | GPS receiver not sending GNGGA sentences | Check GPS device configuration; it may only send GPGGA. Try test file with GNGGA sentences |
| Checksum warnings logged | Signal interference or cable issues | Inspect `ChecksumValid` flag; reject data if critical, or log for diagnostics |
| Coordinates way off | Wrong hemisphere or coordinate format | Verify test sentence format matches NMEA spec; check for typos in latitude/longitude fields |
| UTC DateTime is null | UTC field empty in sentence | Check `data.UTC` string; if empty, GPS may not have time sync yet |
| ConsoleApp window not updating | Event handler on wrong thread | Ensure `Application.MainLoop.Invoke()` used in OnGNGGAUpdated.cs |
| Unit tests failing on coordinate precision | Floating-point rounding differences | Use `BeApproximately(expected, 0.0001)` instead of exact equality |

---

## Next Steps

- **After implementation**: Run `/speckit.tasks` to generate detailed task breakdown
- **Review**: Read `research.md` for architectural decisions and rationale
- **Data model**: Reference `data-model.md` for complete field documentation
- **Constitution**: Review `.specify/memory/constitution.md` for quality standards

---

## Quick Reference: NMEA GNGGA Sentence Format

```
$GNGGA,hhmmss.ss,llll.ll,a,yyyyy.yy,a,x,xx,x.x,x.x,M,x.x,M,x.x,xxxx*hh
│      │         │       │ │        │ │ │  │   │   │ │   │ │   │    └─ Checksum
│      │         │       │ │        │ │ │  │   │   │ │   │ │   └────── DGPS station ID
│      │         │       │ │        │ │ │  │   │   │ │   │ └────────── DGPS data age (seconds)
│      │         │       │ │        │ │ │  │   │   │ │   └──────────── Geoid height units (M)
│      │         │       │ │        │ │ │  │   │   │ └──────────────── Geoid height (meters)
│      │         │       │ │        │ │ │  │   │   └──────────────────Altitude units (M)
│      │         │       │ │        │ │ │  │   └──────────────────────Altitude (meters above sea level)
│      │         │       │ │        │ │ │  └──────────────────────────HDOP (horizontal dilution of precision)
│      │         │       │ │        │ │ └─────────────────────────────Satellites in use
│      │         │       │ │        │ └───────────────────────────────Fix quality (0=invalid, 1=GPS, 2=DGPS, etc.)
│      │         │       │ │        └───────────────────────────────── Longitude hemisphere (E/W)
│      │         │       │ └────────────────────────────────────────── Longitude (dddmm.mmmmm)
│      │         │       └──────────────────────────────────────────── Latitude hemisphere (N/S)
│      │         └──────────────────────────────────────────────────── Latitude (ddmm.mmmmm)
│      └────────────────────────────────────────────────────────────── UTC time (hhmmss or hhmmss.ss)
└───────────────────────────────────────────────────────────────────── Sentence ID (GNGGA = Global GNSS)
```

**Example**:
`$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71`

**Interpretation**:
- Time: 21:19:21.00 UTC
- Position: 34° 0.97900' N = 34.0163166° N, 117° 39.17621' W = -117.6529368° W
- Quality: 2 (DGPS fix), 12 satellites, HDOP 0.86
- Altitude: 247.6m above sea level, Geoid separation: -32.2m
- DGPS station: 0000
