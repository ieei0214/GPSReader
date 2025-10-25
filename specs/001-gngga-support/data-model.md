# Data Model: GNGGA NMEA Sentence Support

**Feature**: 001-gngga-support | **Date**: 2025-10-25
**Purpose**: Define data structures and relationships for GNGGA parser implementation

## Overview

This feature introduces three new data structures following the established NMEA parser pattern:
1. **GNGGAData** - Model representing parsed GNGGA sentence fields
2. **GNGGAEventArgs** - Event arguments containing raw sentence + parsed data
3. **GNGGAParser** - Parser component (documented here for completeness, implementation details in tasks.md)

All models inherit from existing base classes to maintain architectural consistency.

---

## Entity Definitions

### 1. GNGGAData

**Purpose**: Represents a parsed GNGGA sentence with all extracted field values

**Base Class**: `NMEAData` (provides `Checksum` property and `IsFieldNotEmpty()` helper)

**Namespace**: `GPSReader.Models`

**Fields**:

| Field | Type | Nullable | Description | Example | Validation Rules |
|-------|------|----------|-------------|---------|------------------|
| `UTC` | `string` | Yes | Original UTC time string from NMEA sentence | "211921.00", "123519" | Format: hhmmss or hhmmss.ss |
| `UTCDateTime` | `DateTime` | Yes | Parsed UTC time as DateTime object (date portion is arbitrary, only time is meaningful) | DateTime(1900,1,1,21,19,21,0) | Parsed from UTC string; null if UTC empty |
| `Latitude` | `string` | Yes | Latitude in decimal degrees (converted from NMEA degrees-minutes format) | "34.0163166", "-45.123" | Decimal degrees [-90, 90]; log warning if out of range |
| `Longitude` | `string` | Yes | Longitude in decimal degrees (converted from NMEA degrees-minutes format) | "-117.6529368", "12.456" | Decimal degrees [-180, 180]; log warning if out of range |
| `Quality` | `int` | Yes | GPS fix quality indicator | 1, 2 | 0=Invalid, 1=GPS, 2=DGPS, 3=PPS, 4-9=Other; log warning if >9 |
| `Satellites` | `int` | Yes | Number of satellites in use | 12, 8 | Typically 0-24; log warning if >30 |
| `HDOP` | `double` | Yes | Horizontal Dilution of Precision | 0.86, 1.5 | Typically 0.5-50; log warning if >100 |
| `Altitude` | `double` | Yes | Altitude above mean sea level | 247.6, -15.2 | Meters; can be negative |
| `AltitudeUnits` | `string` | Yes | Unit of altitude measurement | "M" | Typically "M" (meters) |
| `GeoidHeight` | `double` | Yes | Geoid height (separation between geoid and WGS84 ellipsoid) | -32.2, 46.9 | Meters; can be negative |
| `GeoidHeightUnits` | `string` | Yes | Unit of geoid height measurement | "M" | Typically "M" (meters) |
| `DGPSDataAge` | `string` | Yes | Age of DGPS data in seconds (empty if not using DGPS) | "", "5.2" | Empty or numeric; optional field |
| `DGPSStationID` | `string` | Yes | DGPS station ID (empty if not using DGPS) | "0000", "" | Empty or 4-digit code; optional field |
| `Checksum` | `string` | Yes | NMEA checksum value | "71", "47" | Inherited from NMEAData; 2-character hex |
| `ChecksumValid` | `bool` | No | Indicates whether received checksum matches calculated checksum | true, false | Computed during parsing; false triggers warning log |

**Relationships**:
- **Inherits from**: `NMEAData` (base class for all NMEA data models)
- **Used by**: `GNGGAEventArgs` (composition: event args contain GNGGAData instance)
- **Created by**: `GNGGAParser.TryParse()` via `GNGGAData.CreateFromFields()` factory method

**State Transitions**: N/A (immutable data model - properties set once during parsing, no lifecycle)

**Factory Method**:
```csharp
public static GNGGAData CreateFromFields(string[] fields)
```
- **Input**: `fields` - Array of 15 NMEA sentence fields split by comma
- **Output**: Populated `GNGGAData` instance
- **Logic**:
  1. Validate field count >= 15 (else return null or throw)
  2. Extract each field with null checks via `IsFieldNotEmpty()`
  3. Convert coordinates from NMEA format (ddmm.mmmmm) to decimal degrees
  4. Parse numeric fields (Quality, Satellites, HDOP, Altitude, GeoidHeight)
  5. Parse UTC string to DateTime (handle both 6-digit and 8-digit formats)
  6. Return initialized GNGGAData instance

**Coordinate Conversion Logic** (reused from GPGGAData):
```csharp
private static string ConvertToDecimalDegrees(string degreesMinutes, string direction)
{
    // Input: "3400.97900", "N" → Output: "34.0163166"
    // Input: "11739.17621", "W" → Output: "-117.6529368"
    var parts = degreesMinutes.Split('.');
    var degrees = int.Parse(parts[0].Substring(0, parts[0].Length - 2));  // First N-2 digits
    var minutes = double.Parse(parts[0].Substring(parts[0].Length - 2) + "." + parts[1]); // Last 2 digits + decimal
    var decimalDegrees = degrees + minutes / 60;
    if (direction == "S" || direction == "W")
        decimalDegrees = -decimalDegrees;
    return decimalDegrees.ToString();
}
```

**Example Instance**:
```csharp
var data = new GNGGAData
{
    UTC = "211921.00",
    UTCDateTime = DateTime.ParseExact("211921.00", "HHmmss.ff", CultureInfo.InvariantCulture),
    Latitude = "34.0163166",
    Longitude = "-117.6529368",
    Quality = 2,
    Satellites = 12,
    HDOP = 0.86,
    Altitude = 247.6,
    AltitudeUnits = "M",
    GeoidHeight = -32.2,
    GeoidHeightUnits = "M",
    DGPSDataAge = null,  // Empty field
    DGPSStationID = "0000",
    Checksum = "71",
    ChecksumValid = true
};
```

---

### 2. GNGGAEventArgs

**Purpose**: Event arguments raised when a GNGGA sentence is successfully parsed

**Base Class**: `NMEAEventArgs` (provides `RawData` property containing original NMEA sentence string)

**Namespace**: `GPSReader.EventArgs`

**Fields**:

| Field | Type | Nullable | Description | Source |
|-------|------|----------|-------------|--------|
| `RawData` | `string` | No | Original unparsed NMEA sentence | Inherited from NMEAEventArgs |
| `GNGGAData` | `GNGGAData` | No | Parsed GNGGA sentence data | Constructor parameter |

**Constructor**:
```csharp
public GNGGAEventArgs(string rawData, GNGGAData data) : base(rawData)
{
    GNGGAData = data;
}
```

**Relationships**:
- **Inherits from**: `NMEAEventArgs`
- **Contains**: `GNGGAData` instance (composition)
- **Raised by**: `GPSReaderService.OnGNGGAUpdated` event
- **Consumed by**: ConsoleApp event handlers (OnGNGGAUpdated.cs) and user application code

**Usage Example**:
```csharp
gpsReaderService.OnGNGGAUpdated += (sender, e) =>
{
    Console.WriteLine($"Raw: {e.RawData}");
    Console.WriteLine($"Position: {e.GNGGAData.Latitude}, {e.GNGGAData.Longitude}");
    Console.WriteLine($"Quality: {e.GNGGAData.Quality} ({e.GNGGAData.Satellites} satellites)");
    if (!e.GNGGAData.ChecksumValid)
        Console.WriteLine("Warning: Checksum invalid!");
};
```

---

### 3. OnGNGGAUpdated Event

**Purpose**: Event raised by GPSReaderService when a GNGGA sentence is successfully parsed

**Signature**:
```csharp
public event EventHandler<GNGGAEventArgs>? OnGNGGAUpdated;
```

**Location**: `GPSReaderService` class (GPSReader/GPSReader.cs)

**Trigger Conditions**:
- GNGGA sentence identified (starts with "$GNGGA")
- Sentence has >= 15 fields
- Fields successfully parsed (numeric conversions succeed)
- Event raised **even if** checksum is invalid (validation status included in GNGGAData.ChecksumValid)

**Event Lifecycle**:
1. User subscribes: `gpsReader.OnGNGGAUpdated += MyHandler;`
2. GNGGA sentence received from INMEAInput source
3. GNGGAParser.TryParse() validates and parses sentence
4. GPSReaderService raises OnGNGGAUpdated with populated GNGGAEventArgs
5. All subscribed handlers execute sequentially

**Concurrency**: Event handlers execute on the same thread as the parser (serial port read thread or file input thread). ConsoleApp uses `Application.MainLoop.Invoke()` to marshal UI updates to main thread.

---

### 4. GNGGAParser (Component Overview)

**Purpose**: Stateless parser component that recognizes and parses GNGGA sentences

**Base Class**: `BaseNMEAParser`

**Namespace**: `GPSReader.Parsers`

**Key Properties**:
- `SentenceId` (override): Returns `"GNGGA"`

**Key Methods**:
- `TryParse(string sentence, out NMEAEventArgs? eventArgs)` (override)

**Parsing Flow**:
```
1. Check if sentence starts with "$GNGGA"
   ↓ No → return false
   ↓ Yes
2. Extract fields and checksum via GetFieldAndChecksum()
   ↓
3. Validate field count >= 15
   ↓ No → Log error, return false
   ↓ Yes
4. Compute expected checksum, compare with received checksum
   ↓
5. Call GNGGAData.CreateFromFields(fields)
   ↓ Parsing fails → Log error, return false
   ↓ Success
6. Set ChecksumValid flag on GNGGAData instance
   ↓
7. If ChecksumValid == false → Log warning (but continue)
   ↓
8. Perform semantic validation (coordinate ranges, quality range)
   ↓ Out of range → Log warning (but continue)
   ↓
9. Create GNGGAEventArgs(sentence, data)
   ↓
10. Set out parameter eventArgs, return true
```

**Stateless Design**: No instance variables; each TryParse() call is independent. Aligns with constitution Principle IV (performance - no memory retention).

**Error Handling**:
- Invalid sentence structure: Log error, return false (no event raised)
- Invalid checksum: Log warning, continue parsing, set ChecksumValid=false (event raised)
- Out-of-range values: Log warning, continue parsing (event raised)
- Numeric parsing exceptions: Catch, log error, return false (no event raised)

---

## Data Flow Diagram

```
┌─────────────────────┐
│  GPS Receiver       │
│  (Serial/File)      │
└──────────┬──────────┘
           │ NMEA Sentence String:
           │ "$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71"
           ↓
┌──────────────────────────────────────────────────────────────┐
│  GPSReaderService                                            │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  GNGGAParser.TryParse()                                │  │
│  │  1. Validate structure                                 │  │
│  │  2. Extract fields                                     │  │
│  │  3. Compute/validate checksum                          │  │
│  │  4. GNGGAData.CreateFromFields()                       │  │
│  │     ├─ Convert coordinates                             │  │
│  │     ├─ Parse UTC to DateTime                           │  │
│  │     ├─ Parse numeric fields                            │  │
│  │     └─ Set ChecksumValid flag                          │  │
│  │  5. Semantic validation (log warnings)                 │  │
│  │  6. Create GNGGAEventArgs(rawData, GNGGAData)          │  │
│  └────────────────────────────────────────────────────────┘  │
│           │                                                   │
│           ↓                                                   │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  OnGNGGAUpdated.Invoke(sender, GNGGAEventArgs)        │  │
│  └────────────────────────────────────────────────────────┘  │
└───────────────────────────┬──────────────────────────────────┘
                            │ Event with GNGGAEventArgs:
                            │   - RawData: "$GNGGA,..."
                            │   - GNGGAData: { UTC="211921.00", Latitude="34.0163166", ... }
           ┌────────────────┼────────────────┐
           ↓                ↓                ↓
┌─────────────────┐ ┌──────────────┐ ┌──────────────────────┐
│ ConsoleApp      │ │ Unit Tests   │ │ User Application     │
│ (Terminal.Gui)  │ │ (Assertions) │ │ (Custom Logic)       │
│ Display window  │ │ Validate     │ │ Store to database,   │
│ with labels     │ │ parsed data  │ │ trigger alerts, etc. │
└─────────────────┘ └──────────────┘ └──────────────────────┘
```

---

## Field Mapping: NMEA Sentence → GNGGAData

**Example Sentence**: `$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71`

**Split by comma**: `["$GNGGA", "211921.00", "3400.97900", "N", "11739.17621", "W", "2", "12", "0.86", "247.6", "M", "-32.2", "M", "", "0000*71"]`

| Field Index | NMEA Value | Extracted Value | GNGGAData Property | Notes |
|-------------|------------|-----------------|-------------------|-------|
| 0 | `$GNGGA` | N/A | N/A | Sentence identifier (parser match criterion) |
| 1 | `211921.00` | "211921.00" | `UTC` | Original string |
| 1 | `211921.00` | DateTime(1900,1,1,21,19,21,0) | `UTCDateTime` | Parsed via DateTime.ParseExact("HHmmss.ff") |
| 2 | `3400.97900` | "34.0163166" | `Latitude` | Converted: 34 + 0.97900/60 |
| 3 | `N` | (used in conversion) | N/A | Hemisphere (positive latitude) |
| 4 | `11739.17621` | "-117.6529368" | `Longitude` | Converted: -(117 + 39.17621/60) |
| 5 | `W` | (used in conversion) | N/A | Hemisphere (negative longitude) |
| 6 | `2` | 2 | `Quality` | DGPS fix |
| 7 | `12` | 12 | `Satellites` | 12 satellites in use |
| 8 | `0.86` | 0.86 | `HDOP` | Horizontal dilution of precision |
| 9 | `247.6` | 247.6 | `Altitude` | 247.6 meters above sea level |
| 10 | `M` | "M" | `AltitudeUnits` | Meters |
| 11 | `-32.2` | -32.2 | `GeoidHeight` | Geoid separation |
| 12 | `M` | "M" | `GeoidHeightUnits` | Meters |
| 13 | `` (empty) | null | `DGPSDataAge` | No DGPS age data |
| 14 | `0000*71` | "0000" | `DGPSStationID` | DGPS station 0000 (split before '*') |
| 14 | `0000*71` | "71" | `Checksum` | Checksum value (split after '*') |
| (computed) | N/A | true/false | `ChecksumValid` | XOR validation result |

---

## Validation Rules Summary

| Validation Type | Rule | Action on Violation |
|----------------|------|---------------------|
| **Structural** | Sentence starts with "$GNGGA" | Reject parse (return false) |
| **Structural** | Field count >= 15 | Reject parse (return false) |
| **Structural** | Numeric fields parseable (int/double) | Reject parse (return false) |
| **Checksum** | Computed checksum == received checksum | Log warning; set ChecksumValid=false; **continue parse** |
| **Semantic** | Latitude in [-90, 90] | Log warning; **continue parse** |
| **Semantic** | Longitude in [-180, 180] | Log warning; **continue parse** |
| **Semantic** | Quality in [0, 9] | Log warning; **continue parse** |
| **Semantic** | Satellites <= 30 | Log warning; **continue parse** |
| **Semantic** | HDOP <= 100 | Log warning; **continue parse** |

**Philosophy**: Structural failures prevent parsing (garbage data). Checksum/semantic failures warn but preserve data (diagnostic value).

---

## Test Data Examples

### Valid Sentence (Simple Format)
```
Input: $GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47
Expected GNGGAData:
  UTC = "123519"
  UTCDateTime = DateTime(1900,1,1,12,35,19)
  Latitude = "48.1173"
  Longitude = "11.5221"
  Quality = 1
  Satellites = 8
  HDOP = 0.9
  Altitude = 545.4
  AltitudeUnits = "M"
  GeoidHeight = 46.9
  GeoidHeightUnits = "M"
  DGPSDataAge = null
  DGPSStationID = null
  ChecksumValid = true
```

### Valid Sentence (High-Precision Format)
```
Input: $GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71
Expected GNGGAData:
  UTC = "211921.00"
  UTCDateTime = DateTime(1900,1,1,21,19,21,0)
  Latitude = "34.0163166"
  Longitude = "-117.6529368"
  Quality = 2
  Satellites = 12
  HDOP = 0.86
  Altitude = 247.6
  AltitudeUnits = "M"
  GeoidHeight = -32.2
  GeoidHeightUnits = "M"
  DGPSDataAge = null
  DGPSStationID = "0000"
  ChecksumValid = true
```

### Invalid Checksum (Should Parse with Warning)
```
Input: $GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*FF
Expected Behavior:
  - Parse succeeds
  - ChecksumValid = false
  - Logger.LogWarning("Checksum mismatch for GNGGA sentence...")
  - Event raised with parsed data
```

### Out-of-Range Coordinates (Should Parse with Warning)
```
Input: $GNGGA,123519,9507.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*XX
Expected Behavior:
  - Parse succeeds
  - Latitude = "95.1173" (invalid)
  - Logger.LogWarning("Latitude out of valid range [-90, 90]: 95.1173")
  - Event raised with parsed data
```

### Insufficient Fields (Should Reject)
```
Input: $GNGGA,123519,4807.038,N
Expected Behavior:
  - TryParse returns false
  - Logger.LogError("Insufficient fields in GNGGA sentence")
  - No event raised
```

---

## Persistence Considerations

**Storage**: N/A - This is a real-time parsing library. No data persistence within GPSReader library.

**Lifetime**: GNGGAData instances exist only for the duration of event processing. GPSReaderService does not retain history.

**User Responsibility**: Applications consuming OnGNGGAUpdated events are responsible for persisting data if needed (e.g., logging to database, writing to file).

---

## Dependencies

### Data Model Dependencies (NuGet Packages)
- **None** - GNGGAData, GNGGAEventArgs, GNGGAParser use only .NET standard types

### Internal Dependencies (GPSReader Library)
- `NMEAData` (base class - Models/NMEAData.cs)
- `NMEAEventArgs` (base class - EventArgs/NMEAEventArgs.cs)
- `BaseNMEAParser` (base class - Abstracts/BaseNMEAParser.cs)
- `Microsoft.Extensions.Logging.ILogger` (injected into GPSReaderService, passed to parser implicitly via service context)

---

## Extensibility Notes

**Future Enhancements** (not in scope for this feature):
- Add `IsValid` computed property: `get => ChecksumValid && LatitudeInRange && LongitudeInRange`
- Add structured validation result object instead of boolean flags
- Add altitude validation (typical range -500m to 9000m for surface GPS)
- Add DGPS data age parsing to double instead of string

**Backward Compatibility**: Adding new optional properties to GNGGAData in future versions is non-breaking (MINOR version bump). Changing types or removing properties is breaking (MAJOR version bump).
