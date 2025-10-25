# Tasks: GNGGA NMEA Sentence Support

**Input**: Design documents from `/specs/001-gngga-support/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This is a multi-project .NET solution:
- **GPSReader/**: Main library project (NuGet package)
- **GPSReader.Tests/**: xUnit test project
- **ConsoleApp/**: Console application demonstration

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify project structure and ensure development environment is ready

**Tasks**:

- [X] T001 Verify solution builds successfully with `dotnet build GPSReader.sln` at repository root
- [X] T002 Verify existing tests pass with `dotnet test GPSReader.Tests/GPSReader.Tests.csproj`
- [X] T003 Review GPSReader/Parsers/GPGGAParser.cs as reference implementation for GNGGA parser
- [X] T004 Review GPSReader/Models/GPGGAData.cs as reference for GNGGA data model structure

**Independent Test**: All tasks complete when solution builds, tests pass, and reference files reviewed

---

## Phase 2: User Story 1 - Parse GNGGA Sentences from Multi-GNSS Receivers (Priority: P1)

**Goal**: Implement core GNGGA parsing capability with data model, parser, and event infrastructure

**Independent Test**: Feed GNGGA sentence `$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47` to GPSReader library; verify OnGNGGAUpdated event fires with correct parsed data (UTC=123519, Latitude=48.1173, Longitude=11.5221, Quality=1, Satellites=8, HDOP=0.9, Altitude=545.4M, GeoidHeight=46.9M)

### 2.1 Data Model Implementation

- [X] T005 [P] [US1] Create GPSReader/Models/GNGGAData.cs inheriting from NMEAData with all 14 properties: UTC (string), UTCDateTime (DateTime?), Latitude (string), Longitude (string), Quality (int?), Satellites (int?), HDOP (double?), Altitude (double?), AltitudeUnits (string), GeoidHeight (double?), GeoidHeightUnits (string), DGPSDataAge (string), DGPSStationID (string), ChecksumValid (bool)
- [X] T006 [US1] Add private static ConvertToDecimalDegrees method to GPSReader/Models/GNGGAData.cs (copy from GPGGAData.cs: parse NMEA ddmm.mmmmm format to decimal degrees, apply hemisphere sign)
- [X] T007 [US1] Implement public static CreateFromFields factory method in GPSReader/Models/GNGGAData.cs to parse string array of 15 fields into GNGGAData instance (extract UTC, coordinates, numeric fields, handle nulls via IsFieldNotEmpty)
- [X] T008 [US1] Add UTC DateTime parsing logic in CreateFromFields to handle both 6-digit (hhmmss) and 8-digit (hhmmss.ss) formats using DateTime.ParseExact with CultureInfo.InvariantCulture

### 2.2 Event Args Implementation

- [X] T009 [P] [US1] Create GPSReader/EventArgs/GNGGAEventArgs.cs inheriting from NMEAEventArgs with GNGGAData property and constructor accepting (string rawData, GNGGAData data)

### 2.3 Parser Implementation

- [X] T010 [US1] Create GPSReader/Parsers/GNGGAParser.cs inheriting from BaseNMEAParser with override SentenceId property returning "GNGGA"
- [X] T011 [US1] Implement TryParse method in GPSReader/Parsers/GNGGAParser.cs with sentence identifier check ($GNGGA prefix validation)
- [X] T012 [US1] Add field extraction and validation in TryParse: call GetFieldAndChecksum from base class, validate field count >= 15, log error and return false if insufficient fields
- [X] T013 [US1] Implement checksum validation in TryParse: compute expected checksum (XOR of characters between $ and *), compare with received checksum, set ChecksumValid flag on GNGGAData
- [X] T014 [US1] Add lenient checksum handling: if ChecksumValid is false, log warning via ILogger but continue parsing and raise event (do not reject sentence)
- [X] T015 [US1] Call GNGGAData.CreateFromFields in TryParse, handle parsing exceptions (catch, log error, return false), set ChecksumValid flag, create GNGGAEventArgs, set out parameter, return true
- [X] T016 [US1] Add semantic validation warnings in TryParse: check parsed latitude in [-90, 90], longitude in [-180, 180], quality in [0, 9], satellites <= 30, HDOP <= 100; log warnings for out-of-range values but continue processing

### 2.4 Service Integration

- [X] T017 [US1] Add public event EventHandler<GNGGAEventArgs>? OnGNGGAUpdated to GPSReader/GPSReader.cs (GPSReaderService class)
- [X] T018 [US1] Instantiate GNGGAParser in GPSReaderService constructor in GPSReader/GPSReader.cs
- [X] T019 [US1] Wire GNGGAParser to GPSReaderService parsing loop: when TryParse succeeds, raise OnGNGGAUpdated event with GNGGAEventArgs in GPSReader/GPSReader.cs

### 2.5 Build and Manual Verification

- [X] T020 [US1] Build GPSReader project with `dotnet build GPSReader/GPSReader.csproj` and verify no compilation errors
- [ ] T021 [US1] Manually test parsing by creating a console test harness: instantiate GPSReaderService with FileInput using test file containing `$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*47`, subscribe to OnGNGGAUpdated, verify event fires and data fields match expected values

**Story 1 Completion Criteria**:
- ✅ GNGGAData model created with all 14 properties
- ✅ GNGGAEventArgs created inheriting NMEAEventArgs
- ✅ GNGGAParser implements TryParse with checksum validation, semantic warnings
- ✅ OnGNGGAUpdated event added to GPSReaderService
- ✅ Manual test confirms event fires with correct parsed data

---

## Phase 3: User Story 2 - Verify GNGGA Parsing Through Automated Tests (Priority: P2)

**Goal**: Comprehensive unit test coverage for GNGGA parsing including format variations, edge cases, and validation behavior

**Independent Test**: Run `dotnet test GPSReader.Tests/GPSReader.Tests.csproj --filter "FullyQualifiedName~GNGGA"` and verify all 6 GNGGA test methods pass with 100% success rate

**Dependencies**: Requires User Story 1 (P1) complete - GNGGAData, GNGGAParser, GNGGAEventArgs, OnGNGGAUpdated event must exist

### 3.1 Basic Event and Field Tests

- [X] T022 [US2] Add Test_OnGNGGAUpdated test method in GPSReader.Tests/Tests.cs with [Theory] and [InlineData] for simple format sentence `$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*5C`; verify OnGNGGAUpdated event fires when mock input source raises DataReceived event
- [X] T023 [US2] Add Test_GNGGAEventArgs_SimpleFormat test method in GPSReader.Tests/Tests.cs to validate all fields for simple GNGGA sentence: assert UTC="123519", Latitude≈48.1173 (BeApproximately 0.0001), Longitude≈11.5221, Quality=1, Satellites=8, HDOP≈0.9, Altitude≈545.4, GeoidHeight≈46.9, DGPSDataAge=null, Checksum="5C", ChecksumValid=true
- [X] T024 [US2] Add Test_GNGGAEventArgs_HighPrecisionFormat test method in GPSReader.Tests/Tests.cs with sentence `$GNGGA,211921.00,3400.97900,N,11739.17621,W,2,12,0.86,247.6,M,-32.2,M,,0000*71`; verify UTC="211921.00", UTCDateTime milliseconds parsed correctly, Latitude≈34.0163166, Longitude≈-117.6529368, Quality=2, Satellites=12, HDOP≈0.86, Altitude≈247.6, GeoidHeight≈-32.2, DGPSStationID="0000"

### 3.2 Edge Case and Validation Tests

- [X] T025 [US2] Add Test_GNGGAInvalidChecksum test method in GPSReader.Tests/Tests.cs with sentence having wrong checksum `$GNGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*FF`; verify OnGNGGAUpdated event STILL fires, ChecksumValid=false, logger.LogWarning called (use NSubstitute to verify logging)
- [X] T026 [US2] Add Test_GNGGAInsufficientFields test method in GPSReader.Tests/Tests.cs with malformed sentence `$GNGGA,123519,4807.038,N`; verify OnGNGGAUpdated event does NOT fire, TryParse returns false, logger.LogError called
- [X] T027 [US2] Add Test_GNGGAOutOfRangeValues test method in GPSReader.Tests/Tests.cs with sentence containing latitude=95 (invalid), longitude=200 (invalid), quality=15 (invalid); verify OnGNGGAUpdated event fires (lenient parsing), logger.LogWarning called for each out-of-range value (verify 3 warning calls with NSubstitute)

### 3.3 Test Execution and Validation

- [X] T028 [US2] Run all GNGGA tests with `dotnet test GPSReader.Tests/GPSReader.Tests.csproj --filter "FullyQualifiedName~GNGGA"` and verify 6 tests pass
- [X] T029 [US2] Run full test suite with `dotnet test GPSReader.Tests/GPSReader.Tests.csproj` and verify no regressions in existing GPGGA/GPGLL/GPGSA/GPGSV tests

**Story 2 Completion Criteria**:
- ✅ 6 unit test methods added covering: event triggering, simple format, high-precision format, invalid checksum, insufficient fields, out-of-range values
- ✅ All tests use FluentAssertions (.Should().Be(), .Should().BeApproximately()) and NSubstitute (mock input, verify logging)
- ✅ Test coverage validates coordinate conversion precision (0.0001 degrees), checksum lenient handling, semantic validation warnings
- ✅ All GNGGA tests pass; no test regressions

---

## Phase 4: User Story 3 - View GNGGA Data in Console Application (Priority: P3)

**Goal**: Real-time GNGGA data display in ConsoleApp with Terminal.Gui window showing all parsed fields

**Independent Test**: Run ConsoleApp with FileInput pointing to test file containing GNGGA sentences; verify dedicated GNGGA window appears displaying raw sentence, UTC (both original + formatted DateTime), latitude, longitude, quality, satellites, HDOP, altitude, geoid height, DGPS data, checksum, and validation status; verify window updates when new GNGGA sentences arrive

**Dependencies**: Requires User Story 1 (P1) complete - OnGNGGAUpdated event must exist

### 4.1 Event Handler Implementation

- [X] T030 [P] [US3] Create ConsoleApp/OnGNGGAUpdated.cs as partial Program class with static OnGNGGAUpdated method accepting (GPSReaderService gpsReaderService, Window gnggaWindow) parameters
- [X] T031 [US3] Subscribe to gpsReaderService.OnGNGGAUpdated event in OnGNGGAUpdated method in ConsoleApp/OnGNGGAUpdated.cs; event handler uses Application.MainLoop.Invoke for thread-safe UI updates
- [X] T032 [US3] Implement event handler UI logic in OnGNGGAUpdated.cs: call gnggaWindow.RemoveAll() to clear previous data, create Label instances for each field (Raw Data, UTC with formatted DateTime using CultureInfo("en-US"), Latitude, Longitude, Quality, Satellites, HDOP, Altitude, GeoidHeight, DGPSDataAge, DGPSStationID, Checksum, ChecksumValid status), position labels vertically (Y = 0, 1, 2...), add to window
- [X] T033 [US3] Format UTC DateTime in event handler: if UTCDateTime.HasValue, display both original string and parsed time formatted as "HH:mm:ss.ff"; example: `UTC: 211921.00 (21:19:21.00 en-US)`
- [X] T034 [US3] Display checksum validation status in event handler: show "Checksum: 71 (Valid)" or "Checksum: 71 (Invalid)" based on ChecksumValid flag

### 4.2 Program Integration

- [X] T035 [US3] Add GNGGA window creation in ConsoleApp/Program.cs Main method: instantiate Terminal.Gui Window with title "GNGGA", position next to existing GPGGA window (use appropriate X/Y coordinates)
- [X] T036 [US3] Call OnGNGGAUpdated static method in ConsoleApp/Program.cs after GPSReaderService instantiation, passing gpsReader and gnggaWindow as parameters
- [X] T037 [US3] Add gnggaWindow to Application.Top in ConsoleApp/Program.cs so it displays alongside existing parser windows (GPGGA, GPGLL, GPGSA, GPGSV)

### 4.3 Manual UI Testing

- [X] T038 [US3] Create test data file test_gngga.txt with multiple GNGGA sentences including simple format, high-precision format, invalid checksum, and out-of-range values
- [ ] T039 [US3] Run ConsoleApp with `dotnet run --project ConsoleApp/ConsoleApp.csproj`, select FileInput mode, browse to test_gngga.txt, verify GNGGA window displays and updates correctly for each sentence type
- [ ] T040 [US3] Verify window update performance: GNGGA data should appear within 100ms of sentence reception (per SC-004); confirm no flickering or stale data

**Story 3 Completion Criteria**:
- ✅ OnGNGGAUpdated.cs created with event handler + UI update logic
- ✅ GNGGA window displays all 14 fields + raw data + checksum validation status
- ✅ UTC shown in dual format (original string + formatted DateTime)
- ✅ Window integrated into ConsoleApp Program.cs and appears alongside existing windows
- ✅ Manual testing confirms real-time updates (<100ms latency), no flickering

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, documentation, and release preparation

**Tasks**:

- [X] T041 [P] Add XML documentation comments to all public members in GPSReader/Models/GNGGAData.cs (properties, factory method, coordinate conversion method)
- [X] T042 [P] Add XML documentation comments to GPSReader/EventArgs/GNGGAEventArgs.cs (class, property, constructor)
- [X] T043 [P] Add XML documentation comments to GPSReader/Parsers/GNGGAParser.cs (class, SentenceId property, TryParse method)
- [ ] T044 Run full test suite with `dotnet test` and verify 80%+ parser coverage, 70%+ overall coverage (use `dotnet test --collect:"XPlat Code Coverage"`)
- [X] T045 Build entire solution in Release mode with `dotnet build GPSReader.sln --configuration Release` and verify no warnings
- [ ] T046 Run ConsoleApp with real GPS receiver (serial port) if available, or with comprehensive test file containing mixed GPGGA/GNGGA sentences; verify both sentence types process correctly and display in respective windows
- [X] T047 Update GPSReader/GPSReader.csproj version from 1.0.1 to 1.1.0 (MINOR version bump per semantic versioning - new feature added)
- [X] T048 Review plan.md, data-model.md, quickstart.md documentation for accuracy; update if implementation deviated from design

**Independent Test**: Solution builds in Release mode with no warnings, all tests pass with required coverage, XML docs complete, version bumped, documentation reviewed

---

## Implementation Strategy

### MVP Scope (Minimum Viable Product)

**Recommended MVP**: User Story 1 (P1) only
- Implement core GNGGA parsing (GNGGAData, GNGGAParser, GNGGAEventArgs, OnGNGGAUpdated event)
- Users can programmatically subscribe to OnGNGGAUpdated and process GNGGA data
- Manually verify parsing works with test harness

**Rationale**: Delivers core library functionality; users can integrate GNGGA parsing into applications immediately without waiting for tests or UI

### Incremental Delivery

1. **MVP Release (Story 1)**: Core parsing capability - users can consume GNGGA data via events
2. **Stability Release (Story 2)**: Add unit tests - confidence in correctness and regression prevention
3. **Full Release (Story 3 + Polish)**: ConsoleApp demo + documentation - complete user experience

### Parallel Execution Opportunities

**Within User Story 1** (can run in parallel after T004):
- T005 (GNGGAData model) + T009 (GNGGAEventArgs) → Different files, no dependencies
- After T007 completes: T008 (UTC parsing) can run independently

**Within User Story 2** (all test tasks T022-T027 can run in parallel after Story 1 complete):
- All 6 test methods are independent - can be written/run concurrently

**Within User Story 3** (can run in parallel):
- T030 (OnGNGGAUpdated.cs file creation) + T035 (Program.cs window creation) → Different files
- After T031-T034 complete: T035-T037 (Program.cs integration) sequential

**Polish Phase** (all documentation tasks T041-T043 can run in parallel):
- XML docs for each file are independent

**Example Parallel Workflow**:
```
Session 1: T001-T004 (Setup)
Session 2: T005 + T009 in parallel
Session 3: T006-T008 sequentially (depend on T005)
Session 4: T010-T016 sequentially (parser logic)
Session 5: T017-T019 sequentially (service integration)
Session 6: T020-T021 (build + manual test)
Session 7: T022-T027 all in parallel (6 test methods)
Session 8: T028-T029 sequentially (test execution)
Session 9: T030 + T035 in parallel
Session 10: T031-T034 sequentially, then T036-T037
Session 11: T038-T040 (UI testing)
Session 12: T041-T043 in parallel, then T044-T048 sequentially
```

**Total Estimated Tasks**: 48
- Setup: 4 tasks
- User Story 1 (P1): 17 tasks
- User Story 2 (P2): 8 tasks
- User Story 3 (P3): 11 tasks
- Polish: 8 tasks

**Parallel Opportunities**: 12 tasks can run concurrently (25% parallelization potential)

---

## Dependencies

**Story Completion Order**:
1. **Phase 1 (Setup)**: Must complete first - verifies environment ready
2. **Phase 2 (Story 1 - P1)**: No dependencies on other stories - can start after Setup
3. **Phase 3 (Story 2 - P2)**: **DEPENDS ON Story 1** - requires GNGGAData, GNGGAParser, OnGNGGAUpdated to exist
4. **Phase 4 (Story 3 - P3)**: **DEPENDS ON Story 1** - requires OnGNGGAUpdated event to exist (does NOT depend on Story 2 tests)
5. **Phase 5 (Polish)**: Depends on all stories complete

**Critical Path**: Setup → Story 1 (P1) → Polish
**Parallel Path**: After Story 1 complete, Story 2 (P2) and Story 3 (P3) can run in parallel

**Dependency Diagram**:
```
Setup (T001-T004)
    ↓
User Story 1 - P1 (T005-T021)
    ↓
    ├─→ User Story 2 - P2 (T022-T029)
    │
    └─→ User Story 3 - P3 (T030-T040)
         ↓
         └─→ Polish (T041-T048)
```

---

## Task Validation Checklist

✅ All tasks follow format: `- [ ] [ID] [P?] [Story?] Description with file path`
✅ Tasks organized by user story for independent implementation
✅ Each user story phase has clear goal and independent test criteria
✅ File paths specified for every implementation task
✅ Parallel opportunities identified with [P] marker
✅ Dependencies documented (Story 2 and 3 depend on Story 1)
✅ MVP scope defined (Story 1 only)
✅ Total task count: 48 tasks
✅ Parallelization potential: 12 tasks (25%)

---

## Notes

- **Testing Philosophy**: Constitution requires tests (Principle II). All 6 test methods in Story 2 are mandatory, not optional.
- **Reference Implementation**: GPGGA parser provides proven pattern - copy structure verbatim for consistency
- **Checksum Handling**: Lenient validation is critical - parse with warnings, don't reject (per clarification decision)
- **Coordinate Precision**: Use BeApproximately(0.0001) in tests - floating-point exact equality unreliable
- **UI Thread Safety**: ConsoleApp MUST use Application.MainLoop.Invoke() for Terminal.Gui updates from parser thread
- **Version Bump**: MINOR version (1.0.1 → 1.1.0) required - new parser type added (backward compatible, non-breaking)
