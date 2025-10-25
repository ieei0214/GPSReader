# Research: GNGGA NMEA Sentence Support

**Feature**: 001-gngga-support | **Date**: 2025-10-25
**Purpose**: Document technical research findings and architectural decisions for GNGGA parser implementation

## Research Summary

All technical details for this feature are well-defined. The implementation follows the established GPGGA pattern with no unknowns or architectural decisions required. This document captures validation of assumptions and confirms the approach.

## Findings

### 1. NMEA 0183 GNGGA Sentence Format

**Research Question**: Confirm GNGGA sentence structure matches GPGGA

**Finding**: ✅ Confirmed - GNGGA and GPGGA are structurally identical

**Details**:
- **Format**: `$GNGGA,hhmmss.ss,llll.ll,a,yyyyy.yy,a,x,xx,x.x,x.x,M,x.x,M,x.x,xxxx*hh`
- **Field Count**: 15 fields (including sentence ID and checksum)
- **Field Meanings**: Identical to GPGGA (UTC time, latitude, longitude, fix quality, satellites, HDOP, altitude, geoid height, DGPS data age, DGPS station ID)
- **Only Difference**: Sentence identifier prefix ($GNGGA vs $GPGGA)
- **Talker ID**: "GN" indicates GNSS (multi-constellation) vs "GP" for GPS-only

**Source**: NMEA 0183 standard, real-world GPS receiver data provided in spec (test sentence `$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71`)

**Decision**: Reuse GPGGAData field structure and coordinate conversion logic for GNGGAData

**Rationale**: No structural differences exist; code reuse maximizes consistency and reduces bugs

**Alternatives Considered**:
- Create abstract base class shared by both → **Rejected**: Adds complexity without benefit; constitution favors simplicity
- Share GPGGAData model directly → **Rejected**: Violates separation of concerns; clarification session confirmed dedicated classes

---

### 2. Checksum Validation Strategy

**Research Question**: How should checksum validation handle mismatches?

**Finding**: ✅ Decided - Parse with warning (lenient validation)

**Details**:
- NMEA checksum is XOR of all characters between '$' and '*'
- Existing BaseNMEAParser.GetFieldAndChecksum() extracts checksum from sentence
- Checksum validation logic exists in GPGGA implementation (can be reused)

**Decision**: Parse sentences with invalid checksums but log warnings and set validation status flag

**Rationale**:
- Preserves diagnostic data from GPS receivers experiencing signal issues
- Allows application-level decisions on how to handle questionable data
- Aligns with graceful error handling principle (constitution Principle V)
- Clarification session confirmed: "Parse with warning - Parser accepts sentences with checksum mismatches but logs a warning and marks data as unvalidated"

**Alternatives Considered**:
- Strict rejection → **Rejected**: Loses potentially useful diagnostic information
- No validation → **Rejected**: Fails to alert users to data integrity issues

---

### 3. UTC Time Representation

**Research Question**: How to handle variable UTC formats (6-digit "123519" vs 8-digit "211921.00")?

**Finding**: ✅ Decided - Dual representation (string + DateTime)

**Details**:
- NMEA allows both "hhmmss" (6 digits) and "hhmmss.ss" (8 digits with fractional seconds)
- Existing GPGGA implementation stores UTC as string only
- ConsoleApp currently parses UTC string to DateTime for display formatting

**Decision**: Store both original string AND parsed DateTime object in GNGGAData

**Rationale**:
- Preserves data fidelity (original format retained for debugging/validation)
- Enables programmatic access (DateTime for application logic, comparisons, formatting)
- Clarification session confirmed: "Preserve original format in a string field and provide an additional parsed DateTime object for programmatic access"

**Implementation Approach**:
```csharp
public class GNGGAData : NMEAData
{
    public string? UTC { get; set; }           // Original: "211921.00" or "123519"
    public DateTime? UTCDateTime { get; set; }  // Parsed: DateTime(1900,1,1,21,19,21,0)
    // ... other fields
}
```

**Alternatives Considered**:
- String only → **Rejected**: Forces every consumer to reparse
- DateTime only → **Rejected**: Loses original format precision information
- Standardize to 8-digit format → **Rejected**: Alters source data

---

### 4. Semantic Validation (Coordinate Ranges, Quality Indicators)

**Research Question**: Should parser reject semantically invalid values (e.g., latitude > 90 degrees)?

**Finding**: ✅ Decided - No rejection; log warnings only

**Details**:
- Valid ranges: Latitude [-90, 90], Longitude [-180, 180], Quality [0-9]
- GPS receivers can output edge-case values during signal acquisition or errors
- Existing parsers do not perform semantic validation

**Decision**: Accept all structurally valid sentences; log warnings for out-of-range values

**Rationale**:
- GPS diagnostic scenarios benefit from seeing invalid data
- Application layer is better positioned to decide handling strategy based on use case
- Maintains consistency with existing parser behavior
- Clarification session confirmed: "Parse without semantic validation - Accept all structurally valid sentences but log warnings for values outside normal ranges"

**Implementation Approach**:
- Validate ranges after parsing: `if (latitude > 90 || latitude < -90) logger.LogWarning(...)`
- Continue processing and raise event with parsed data
- Do not add validation status flags (keep model simple; logging sufficient)

**Alternatives Considered**:
- Strict validation with rejection → **Rejected**: Loses diagnostic data
- Validation with status flags → **Rejected**: Adds model complexity without clear benefit

---

### 5. Coordinate Conversion Algorithm

**Research Question**: Confirm NMEA coordinate conversion approach

**Finding**: ✅ Validated - Reuse existing GPGGAData.ConvertToDecimalDegrees()

**Details**:
- NMEA format: `ddmm.mmmmm` for latitude, `dddmm.mmmmm` for longitude
- Algorithm: `degrees + (minutes / 60)`, apply hemisphere sign
- Existing GPGGAData implementation:
  ```csharp
  var degrees = int.Parse(parts[0].Substring(0, parts[0].Length - 2));
  var minutes = double.Parse(parts[0].Substring(parts[0].Length - 2) + "." + parts[1]);
  var decimalDegrees = degrees + minutes / 60;
  if (direction == "S" || direction == "W") decimalDegrees = -decimalDegrees;
  ```

**Decision**: Copy coordinate conversion logic directly from GPGGAData to GNGGAData

**Rationale**:
- Proven correct (existing tests validate accuracy to 0.0001 degrees)
- GNGGA uses identical coordinate format
- Duplication acceptable for dedicated class architecture (per clarification)

**Validation**:
- Test sentence: `$GNGGA,211921.00,3400.97900,N,11739.17621,W,...`
- Expected: 34.0163166 N (34 + 0.97900/60), -117.6529368 W (-(117 + 39.17621/60))
- Spec acceptance scenario 4 confirms this calculation

**Alternatives Considered**:
- Extract to shared utility method → **Considered**: Would reduce duplication but adds extra dependency; acceptable tradeoff for simplicity

---

### 6. Testing Strategy

**Research Question**: What test coverage is required?

**Finding**: ✅ Defined - Comprehensive unit test suite

**Details**: FR-010 mandates tests covering:
1. Event triggering (OnGNGGAUpdated fires)
2. Data field accuracy (all 15 fields extracted correctly)
3. Coordinate conversion precision (within 0.0001 degrees)
4. Null handling for empty fields (DGPS age, station ID)
5. Checksum validation behavior (valid + invalid checksums)
6. Semantic validation warnings (out-of-range coordinates)
7. Format variations:
   - Simple UTC (6-digit "123519")
   - Millisecond UTC (8-digit "211921.00")
   - High-precision coordinates (5+ decimal places)
   - Populated DGPS station ID

**Decision**: Follow existing test pattern from Test_GPGGAEventArgs

**Test Implementation Plan**:
```csharp
[Theory]
[InlineData("$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47")]
[InlineData("$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71")]
public void Test_OnGNGGAUpdated(string inputNmea)
public void Test_GNGGAEventArgs(string inputNmea)
public void Test_GNGGAInvalidChecksum(string inputNmea)
public void Test_GNGGAOutOfRangeValues(string inputNmea)
```

**Rationale**: Existing test infrastructure (NSubstitute mocking, FluentAssertions) reusable; pattern proven effective

**Alternatives Considered**:
- Integration tests with real serial data → **Deferred**: Unit tests sufficient for parser logic; integration tests exist for input sources

---

### 7. ConsoleApp Display Requirements

**Research Question**: How should GNGGA window display data?

**Finding**: ✅ Defined - Mirror GPGGA window pattern

**Details**:
- Existing OnGPGGAUpdated.cs creates Terminal.Gui window with labels
- Display format: `Field Name: Value` on separate lines
- UTC shown as both raw string and formatted DateTime
- Window updates via Application.MainLoop.Invoke() for thread safety

**Decision**: Create OnGNGGAUpdated.cs with identical structure to OnGPGGAUpdated.cs

**Display Fields**:
```
Raw Data: $GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71
UTC: 211921.00 (21:19:21.00 en-US)
Latitude: 34.0163166
Longitude: -117.6529368
Quality: 2
Satellites: 12
HDOP: 0.86
Altitude: 247.6M
GeoidHeight: -32.2M
DGPSDataAge:
DGPSStationID: 0000
Checksum: 71 (Valid/Invalid)
```

**Rationale**: Consistency with existing UI; users familiar with GPGGA window can immediately understand GNGGA window

**Alternatives Considered**:
- Combine GPGGA and GNGGA in single window → **Rejected**: Cluttered; both can arrive simultaneously from dual-mode receivers

---

## Technology Stack Validation

| Component | Technology | Version | Status | Notes |
|-----------|-----------|---------|--------|-------|
| **Language** | C# | .NET 8.0 | ✅ Confirmed | GPSReader.csproj specifies `<TargetFramework>net8.0</TargetFramework>` |
| **Logging** | Microsoft.Extensions.Logging | 7.0.0 | ✅ Confirmed | ILogger<T> injection pattern established in GPSReaderService |
| **Serial I/O** | System.IO.Ports | 8.0.0 | ✅ Confirmed | Used by SerialInput.cs (unchanged for this feature) |
| **Testing Framework** | xUnit | Latest | ✅ Confirmed | GPSReader.Tests project uses xUnit patterns |
| **Assertion Library** | FluentAssertions | Latest | ✅ Confirmed | Existing tests use `.Should().Be()` syntax |
| **Mocking** | NSubstitute | Latest | ✅ Confirmed | Existing tests use `Substitute.For<T>()` |
| **Console UI** | Terminal.Gui | Latest | ✅ Confirmed | ConsoleApp uses Window, Label, Application.MainLoop |

**No new dependencies required** - All technologies already present in solution

---

## Best Practices Applied

### 1. Parser Implementation
- ✅ Inherit from BaseNMEAParser
- ✅ Override SentenceId property (return "GNGGA")
- ✅ Override TryParse method with field validation
- ✅ Use GetFieldAndChecksum() helper from base class
- ✅ Log errors via ILogger (not throwing exceptions for invalid sentences)

### 2. Data Model Design
- ✅ Inherit from NMEAData (provides Checksum property + IsFieldNotEmpty helper)
- ✅ Use nullable types for optional fields (DGPSDataAge, DGPSStationID)
- ✅ Provide static CreateFromFields factory method (matches GPGGA pattern)
- ✅ Include both UTC string and UTCDateTime for dual access

### 3. Event Handling
- ✅ Inherit from NMEAEventArgs (provides RawData property)
- ✅ Include strongly-typed GNGGAData property
- ✅ Constructor accepts (string rawData, GNGGAData data)

### 4. Testing
- ✅ Use [Theory] with [InlineData] for parameterized tests
- ✅ Mock INMEAInput with NSubstitute
- ✅ Raise events via `Raise.Event<EventHandler<T>>()`
- ✅ Assert with FluentAssertions `.Should()` syntax
- ✅ Validate coordinate conversion with `BeApproximately(expected, 0.0001)`

### 5. ConsoleApp Integration
- ✅ Create dedicated partial class for event handler (OnGNGGAUpdated.cs)
- ✅ Use Application.MainLoop.Invoke() for UI thread safety
- ✅ Format UTC DateTime with CultureInfo("en-US")
- ✅ Clear and repopulate window on each update (gnggaWindow.RemoveAll())

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Coordinate conversion precision errors | Low | Medium | Reusing proven GPGGA algorithm; comprehensive test validation with real-world sentence |
| Checksum validation bugs | Low | Low | Reusing BaseNMEAParser.GetFieldAndChecksum(); existing parsers validate approach |
| Missing UTC parsing edge cases | Low | Medium | Dual representation (string + DateTime) ensures fallback; tests cover both formats |
| Terminal.Gui window display issues | Low | Low | Pattern proven in OnGPGGAUpdated; copy-paste approach minimizes risk |
| Test coverage gaps | Low | High | FR-010 explicit test requirements; checklist validation before PR |

**Overall Risk Level**: **Low** - Straightforward extension with established patterns and comprehensive requirements

---

## Open Questions

**None** - All technical decisions resolved during clarification session and research phase.

---

## References

1. NMEA 0183 Standard - GGA Sentence Definition
2. Existing GPSReader implementation:
   - GPGGAParser.cs (reference implementation)
   - GPGGAData.cs (field structure and coordinate conversion)
   - GPGGAEventArgs.cs (event args pattern)
   - Tests.cs (testing patterns)
   - OnGPGGAUpdated.cs (ConsoleApp display pattern)
3. Project constitution: `.specify/memory/constitution.md`
4. Feature specification: `specs/001-gngga-support/spec.md`
5. Real-world test sentence: `$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71`
