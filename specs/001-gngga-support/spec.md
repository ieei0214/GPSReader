# Feature Specification: GNGGA NMEA Sentence Support

**Feature Branch**: `001-gngga-support`
**Created**: 2025-10-25
**Status**: Draft
**Input**: User description: "To add GNGGA (NMEA 0183 format) support in GPSReader, unit test in GPSReader.Tests, and then add the example in ConsoleApp."

## Clarifications

### Session 2025-10-25

- Q: Should the specific GNGGA sentence `$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71` become the canonical test case reference? → A: Add this as an additional test case example alongside the existing one to demonstrate format variations (milliseconds in UTC, higher precision coordinates, populated DGPS station ID)
- Q: Should the parser strictly reject sentences with checksum mismatches or parse them with a warning? → A: Parse with warning - Parser accepts sentences with checksum mismatches but logs a warning and marks data as unvalidated
- Q: Should UTC time preserve original format or be standardized when displayed/accessed? → A: Preserve original format in a string field and provide an additional parsed DateTime object for programmatic access
- Q: Should the parser validate semantic correctness of values (e.g., latitude > 90 degrees, quality indicator out of range)? → A: Parse without semantic validation - Accept all structurally valid sentences but log warnings for values outside normal ranges
- Q: Should GNGGA have its own dedicated data model or share the existing GPGGA data model? → A: Dedicated model - Create GNGGAData as a separate class (even though fields are identical to GPGGA) for clarity and future flexibility

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Parse GNGGA Sentences from Multi-GNSS Receivers (Priority: P1)

Users with multi-constellation GPS receivers (GPS + GLONASS, GPS + Galileo, etc.) need to parse GNGGA sentences to obtain combined positioning data from multiple satellite systems. GNGGA is identical in structure to GPGGA but indicates data from multiple Global Navigation Satellite Systems (GNSS) rather than GPS alone.

**Why this priority**: This is the core requirement for supporting multi-GNSS receivers. Without GNGGA parsing, users cannot process data from modern GPS devices that combine multiple satellite constellations, which are increasingly common in commercial GPS hardware.

**Independent Test**: Can be fully tested by feeding a valid GNGGA sentence (e.g., `$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47`) to the GPSReader library and verifying that position, time, quality, and altitude data are correctly extracted and made available through events.

**Acceptance Scenarios**:

1. **Given** a GPSReader service is running, **When** it receives a valid GNGGA sentence with all fields populated, **Then** the system parses the sentence successfully and raises an event containing UTC time, latitude, longitude, fix quality, number of satellites, HDOP, altitude, geoid height, and checksum
2. **Given** a GPSReader service is running, **When** it receives a GNGGA sentence with optional fields empty (e.g., DGPS data age), **Then** the system parses the sentence successfully and represents empty fields as null values
3. **Given** a GPSReader service is running, **When** it receives a malformed GNGGA sentence with insufficient fields, **Then** the system logs a parsing failure and does not raise an event
4. **Given** a GPSReader service is running, **When** it receives a GNGGA sentence with an invalid checksum, **Then** the system parses the sentence, raises an event with the parsed data, logs a warning about checksum mismatch, and includes a validation status indicator in the event data

---

### User Story 2 - Verify GNGGA Parsing Through Automated Tests (Priority: P2)

Developers maintaining the GPSReader library need comprehensive unit tests for GNGGA parsing to ensure correctness, prevent regressions, and validate edge cases. Tests should follow the existing testing patterns used for GPGGA and other sentence types.

**Why this priority**: Testing is essential for reliability but can only be implemented after the core parsing functionality exists. Tests ensure the parser handles various real-world GNGGA formats correctly.

**Independent Test**: Can be fully tested by running the test suite and verifying that all GNGGA test cases pass, including tests for valid sentences, field validation, coordinate conversion accuracy, and event triggering.

**Acceptance Scenarios**:

1. **Given** a test harness with a mock input source, **When** a valid GNGGA sentence is injected, **Then** the OnGNGGAUpdated event is raised with correct parsed data
2. **Given** a test harness with sample GNGGA data, **When** the test validates parsed coordinates, **Then** the decimal degree conversion from NMEA format matches expected values within acceptable precision (0.0001 degrees)
3. **Given** a test harness with various GNGGA sentences, **When** tests validate all data fields, **Then** all fields (UTC, position, quality indicators, altitude, geoid height, checksum) are correctly extracted
4. **Given** a test harness with the real-world sentence `$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71`, **When** the test validates parsing, **Then** UTC time "211921.00" with milliseconds is parsed correctly, coordinates convert to approximately 34.0163166 N and -117.6529368 W, fix quality is 2 (DGPS), satellites count is 12, HDOP is 0.86, altitude is 247.6 M, geoid height is -32.2 M, and DGPS station ID is "0000"

---

### User Story 3 - View GNGGA Data in Console Application (Priority: P3)

Users of the ConsoleApp demonstration program need to see GNGGA data displayed in real-time to verify their multi-GNSS receiver is working correctly and to understand the data format. The display should match the existing pattern used for GPGGA data.

**Why this priority**: This is a demonstration feature that helps users understand GNGGA data but is not critical for library functionality. It provides visibility into GNGGA parsing but can be implemented after core parsing and testing are complete.

**Independent Test**: Can be fully tested by running the ConsoleApp with a GPS data source that emits GNGGA sentences and verifying that a dedicated window displays all GNGGA fields (raw sentence, UTC time formatted, latitude, longitude, quality, satellites, HDOP, altitude, geoid height, DGPS age, checksum) with proper updates when new sentences arrive.

**Acceptance Scenarios**:

1. **Given** the ConsoleApp is running with a GNGGA data source, **When** a GNGGA sentence is received, **Then** a dedicated GNGGA window displays the raw sentence and all parsed fields in a human-readable format
2. **Given** the GNGGA window is displayed, **When** new GNGGA sentences arrive, **Then** the window updates with the latest data without flickering or displaying stale information
3. **Given** the ConsoleApp displays GNGGA data, **When** the UTC time field is present, **Then** both the original NMEA format string (e.g., "211921.00") and the parsed DateTime in a readable format (e.g., "21:19:21.00") are displayed for user verification

---

### Edge Cases

- When a GNGGA sentence has valid structure but semantically invalid data (e.g., latitude > 90 degrees, quality indicator out of range), the system parses the data, raises the event, but logs a warning about values outside normal ranges
- When GNGGA sentences have field lengths different from the standard 15 fields (fewer fields), the system rejects parsing and logs an error
- When both GPGGA and GNGGA sentences are received in the same data stream, the system processes both independently using their respective parsers
- The parser distinguishes between GNGGA and GPGGA sentences by checking the sentence identifier prefix ($GNGGA vs $GPGGA)
- When coordinate fields contain non-numeric characters or invalid hemisphere indicators, the parsing will fail during numeric conversion and the system will log the error without raising an event

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST parse GNGGA sentences that begin with "$GNGGA" and contain at least 15 comma-separated fields
- **FR-002**: System MUST extract UTC time (preserving original string format AND providing a parsed DateTime object), latitude, longitude, fix quality, number of satellites, HDOP, altitude, altitude units, geoid height, geoid height units, DGPS data age, and checksum from GNGGA sentences
- **FR-003**: System MUST convert latitude and longitude from NMEA format (degrees and decimal minutes with hemisphere) to decimal degrees format, applying negative values for South and West hemispheres
- **FR-004**: System MUST handle optional fields (such as DGPS data age) gracefully by representing them as null when empty in the input sentence
- **FR-005**: System MUST raise an OnGNGGAUpdated event when a valid GNGGA sentence is successfully parsed, providing both the raw sentence and parsed data
- **FR-006**: System MUST validate GNGGA sentence structure and log failures when sentences have insufficient fields without raising parsing events
- **FR-007**: System MUST NOT perform semantic validation on data values (e.g., coordinate ranges, quality indicator values); the system MUST parse all structurally valid sentences but MUST log warnings when values fall outside normal ranges (latitude > 90 or < -90, longitude > 180 or < -180, quality indicator outside 0-9 range, etc.)
- **FR-008**: System MUST validate checksum for GNGGA sentences; if checksum is invalid, the system MUST still parse the sentence and raise an event but MUST log a warning and include a validation status indicator (checksum valid/invalid) in the parsed data
- **FR-009**: System MUST follow the same parsing architecture and patterns used for existing sentence types (GPGGA, GPGLL, GPGSA, GPGSV) to maintain code consistency
- **FR-010**: Test suite MUST include unit tests that verify GNGGA event triggering, data field accuracy, coordinate conversion precision, null handling for empty fields, checksum validation behavior, and semantic validation warnings; tests MUST cover multiple format variations including simple format (6-digit UTC), millisecond-precision format (8-digit UTC like "211921.00"), higher-precision coordinates (5+ decimal places), populated DGPS station ID field, invalid checksum scenarios, and out-of-range value warnings
- **FR-011**: ConsoleApp MUST display GNGGA data in a dedicated window showing all parsed fields with formatted UTC time and checksum validation status
- **FR-012**: ConsoleApp MUST update the GNGGA display in real-time as new sentences are received without blocking other sentence displays

### Key Entities

- **GNGGAData**: Represents parsed GNGGA sentence data with fields for UTC time (both original string format as received and parsed DateTime object), latitude (decimal degrees), longitude (decimal degrees), fix quality indicator, number of satellites in use, horizontal dilution of precision, altitude, altitude units, geoid height, geoid height units, DGPS data age, checksum, and checksum validation status (boolean or enum indicating whether the received checksum matches the calculated value)
- **GNGGAParser**: Component responsible for recognizing GNGGA sentences, validating structure and checksum, extracting fields, converting coordinates, and producing GNGGAData objects with validation status
- **GNGGAEventArgs**: Event data structure containing the raw GNGGA sentence string and associated parsed GNGGAData object
- **OnGNGGAUpdated Event**: Event raised by GPSReaderService when a GNGGA sentence is successfully parsed, allowing consumers to react to GNGGA data updates

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can successfully parse GNGGA sentences from multi-GNSS receivers with 100% of valid sentences triggering the OnGNGGAUpdated event
- **SC-002**: Coordinate conversion accuracy achieves precision within 0.0001 decimal degrees compared to reference calculations
- **SC-003**: All automated tests for GNGGA parsing pass, covering valid sentences, field extraction, coordinate conversion, empty field handling, and malformed sentence rejection
- **SC-004**: ConsoleApp displays GNGGA data in real-time with updates appearing within 100 milliseconds of sentence reception
- **SC-005**: Code follows existing architectural patterns such that GNGGA implementation is structurally identical to GPGGA implementation (same class structure, same event patterns, same testing approach)

## Assumptions

- GNGGA sentence structure is identical to GPGGA structure (15 fields with the same meanings and formats), differing only in the sentence identifier prefix
- The existing coordinate conversion algorithm used for GPGGA (degrees and decimal minutes to decimal degrees) applies identically to GNGGA
- GNGGA and GPGGA sentences may both be present in a data stream, and the system should support both simultaneously
- The existing BaseNMEAParser architecture can be extended to support GNGGA without modifications to the base class
- GNGGAData will be implemented as a separate dedicated class (not shared with GPGGA) to maintain clear separation of concerns, follow existing patterns where each sentence type has its own data model, and allow for future GNGGA-specific extensions
- ConsoleApp uses Terminal.Gui framework for display, and a new window for GNGGA will follow the same layout pattern as the existing GPGGA window
- Unit tests will use the same testing frameworks (xUnit, FluentAssertions, NSubstitute) and mocking patterns as existing tests
