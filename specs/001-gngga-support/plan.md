# Implementation Plan: GNGGA NMEA Sentence Support

**Branch**: `001-gngga-support` | **Date**: 2025-10-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-gngga-support/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Add support for parsing GNGGA (Global Navigation Satellite System GGA) sentences in the GPSReader library. GNGGA sentences are structurally identical to GPGGA but represent combined positioning data from multiple satellite constellations (GPS + GLONASS, GPS + Galileo, etc.). Implementation will follow the existing GPGGA pattern by creating a dedicated GNGGAParser class, GNGGAData model, GNGGAEventArgs, and OnGNGGAUpdated event. The parser will include checksum validation with lenient handling (parse with warnings), semantic value range warnings (without rejection), and dual UTC representation (string + DateTime). Comprehensive unit tests will validate multiple sentence formats, and the ConsoleApp will display GNGGA data in real-time.

## Technical Context

**Language/Version**: C# / .NET 8.0 (from GPSReader.csproj `<TargetFramework>net8.0</TargetFramework>`)
**Primary Dependencies**:
  - Microsoft.Extensions.Logging 7.0.0 (logging abstraction)
  - System.IO.Ports 8.0.0 (serial communication)
  - Terminal.Gui (ConsoleApp UI framework)
  - xUnit, FluentAssertions, NSubstitute (testing)
**Storage**: N/A (stateless parsing, no persistence)
**Testing**: xUnit with FluentAssertions for assertions, NSubstitute for mocking
**Target Platform**: .NET 8.0 compatible platforms (Windows, Linux, macOS)
**Project Type**: Multi-project .NET solution
  - GPSReader (class library - NuGet package)
  - GPSReader.Tests (xUnit test project)
  - ConsoleApp (console application demonstration)
**Performance Goals**:
  - Parse throughput: 100+ NMEA sentences/second (per constitution Principle IV)
  - Event latency: <5ms from parse completion to event invocation
  - Real-time display updates: <100ms (per SC-004)
**Constraints**:
  - Maintain architectural consistency with existing parsers (GPGGA, GPGLL, GPGSA, GPGSV)
  - No breaking changes to public APIs (MINOR version bump per semantic versioning)
  - Coordinate conversion accuracy: 0.0001 decimal degrees precision (SC-002)
  - Test coverage: 80%+ for parser logic, 70%+ overall (constitution Principle II)
**Scale/Scope**:
  - Single new parser type (GNGGAParser)
  - 1 new data model (GNGGAData)
  - 1 new event type (GNGGAEventArgs)
  - 6-8 unit test methods covering format variations and edge cases
  - 1 new ConsoleApp UI window for GNGGA display

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Evaluation

| Principle | Requirement | Status | Notes |
|-----------|-------------|--------|-------|
| **I. Code Quality** | Nullable reference types, explicit interfaces, single responsibility, XML docs, .NET naming | ✅ PASS | GNGGAParser inherits BaseNMEAParser (established interface pattern); GNGGAData inherits NMEAData; single responsibility maintained (one parser per sentence type) |
| **II. Testing (NON-NEGOTIABLE)** | Unit tests for all parsers; integration tests for input sources; 80% parser coverage, 70% overall | ✅ PASS | FR-010 mandates comprehensive unit tests covering valid sentences, malformed input, coordinate conversion, checksum validation, semantic warnings, format variations |
| **III. API Contract Stability** | Semantic versioning; backward compatibility; no breaking changes in MINOR releases | ✅ PASS | Adding new parser/events = MINOR version bump (1.0.1 → 1.1.0); no modifications to existing public APIs; follows extension pattern (new parser class + new event) |
| **IV. Performance** | 100 sentences/sec throughput; <5ms event latency; stateless parsing; no sentence history buffering | ✅ PASS | GNGGA parser follows same stateless pattern as GPGGA (no instance state); GNGGAData inherits from NMEAData (consistent object model); no buffering beyond most recent parsed data |
| **V. User Experience** | Event-driven architecture; typed events; graceful error handling; ILogger usage; working ConsoleApp example | ✅ PASS | OnGNGGAUpdated event with GNGGAEventArgs (typed); checksum/semantic validation log warnings without exceptions (FR-007, FR-008); ConsoleApp updated with GNGGA window (FR-011, FR-012) |

**Overall Gate Status**: ✅ **PASS** - All constitutional principles satisfied

**Justification**: This feature is a straightforward extension following established patterns. No new architectural patterns introduced, no breaking changes, testing requirements met, performance characteristics identical to existing parsers.

## Project Structure

### Documentation (this feature)

```text
specs/001-gngga-support/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (generated below)
├── data-model.md        # Phase 1 output (generated below)
├── quickstart.md        # Phase 1 output (generated below)
├── contracts/           # Phase 1 output (N/A for this feature - no HTTP/GraphQL APIs)
├── checklists/
│   └── requirements.md  # Specification quality checklist (completed)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
GPSReader/                          # Main library project
├── GPSReader.csproj
├── GPSReader.cs                     # Main GPSReaderService
├── Abstracts/
│   └── BaseNMEAParser.cs            # Base class for all parsers (existing)
├── Parsers/
│   ├── GPGGAParser.cs               # Existing GPGGA parser (reference implementation)
│   ├── GPGLLParser.cs               # Existing parsers
│   ├── GPGSAParser.cs
│   ├── GPGSVParser.cs
│   └── GNGGAParser.cs               # NEW: GNGGA parser
├── Models/
│   ├── NMEAData.cs                  # Base class for all NMEA data models (existing)
│   ├── GPGGAData.cs                 # Existing GPGGA model (reference implementation)
│   ├── GPGLLData.cs                 # Existing models
│   ├── GPGSAData.cs
│   ├── GPGSVData.cs
│   └── GNGGAData.cs                 # NEW: GNGGA data model
├── EventArgs/
│   ├── NMEAEventArgs.cs             # Base class for all NMEA event args (existing)
│   ├── GPGGAEventArgs.cs            # Existing GPGGA event args (reference)
│   ├── GPGLLEventArgs.cs            # Existing event args
│   ├── GPGSAEventArgs.cs
│   ├── GPGSVEventArgs.cs
│   ├── GPGSVListEventArgs.cs
│   └── GNGGAEventArgs.cs            # NEW: GNGGA event args
├── Input/
│   ├── SerialInput.cs               # Existing input sources (unchanged)
│   └── FileInput.cs
├── Interfaces/
│   └── INMEAInput.cs                # Existing interface (unchanged)
├── Exceptions/
│   └── GPSException.cs              # Existing exception (unchanged)
└── Usings.cs                        # Global usings (unchanged)

GPSReader.Tests/                     # Test project
├── GPSReader.Tests.csproj
├── Tests.cs                         # NEW: Add GNGGA test methods here
├── MockInputSource.cs               # Existing test infrastructure (reused)
└── GlobalUsings.cs                  # Existing (unchanged)

ConsoleApp/                          # Demonstration application
├── ConsoleApp.csproj
├── Program.cs                       # NEW: Wire up GNGGA window + event subscription
├── OnGPGGAUpdated.cs                # Existing GPGGA event handler (reference implementation)
├── OnGPGLLUpdated.cs                # Existing event handlers
├── OnGPGSAUpdated.cs
├── OnGPGSVListUpdated.cs
├── OnGNGGAUpdated.cs                # NEW: GNGGA event handler + window display logic
├── OnDataReceived.cs                # Existing (unchanged)
├── Log.cs                           # Existing (unchanged)
└── Windows.cs                       # Existing (unchanged)
```

**Structure Decision**: The repository uses a standard .NET multi-project structure with clear separation between library code (GPSReader), tests (GPSReader.Tests), and demonstration app (ConsoleApp). The GNGGA implementation will follow the exact same folder/file organization as the existing GPGGA implementation to maintain architectural consistency per FR-009 and constitution Principle V.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations detected. All constitutional gates pass. No complexity justification required.

---

## Post-Design Constitution Re-Check

*Re-evaluation after Phase 1 design completion (data-model.md, quickstart.md)*

### Design Artifacts Review

| Principle | Post-Design Status | Evidence |
|-----------|-------------------|----------|
| **I. Code Quality** | ✅ PASS | data-model.md confirms: GNGGAData inherits NMEAData (nullable types enforced), GNGGAParser inherits BaseNMEAParser (interface pattern), factory method pattern used (CreateFromFields), coordinate conversion method documented with clear logic |
| **II. Testing** | ✅ PASS | quickstart.md documents comprehensive test plan: Test_OnGNGGAUpdated (event firing), Test_GNGGAEventArgs (field validation with 2 test sentences), Test_GNGGAInvalidChecksum (checksum behavior), Test_GNGGAOutOfRangeValues (semantic warnings); aligns with FR-010 requirements |
| **III. API Contract Stability** | ✅ PASS | Design adds new types (GNGGAData, GNGGAEventArgs, GNGGAParser, OnGNGGAUpdated event) without modifying existing APIs; MINOR version bump appropriate; backward compatible |
| **IV. Performance** | ✅ PASS | data-model.md confirms stateless parsing (no instance state in GNGGAParser), no sentence history buffering, same object model as existing parsers (GNGGAData mirrors GPGGAData structure); performance characteristics identical to established baseline |
| **V. User Experience** | ✅ PASS | quickstart.md provides complete usage examples: event subscription patterns, error handling, filtering valid data, UTC time access patterns; ConsoleApp integration documented (OnGNGGAUpdated.cs with Terminal.Gui window); graceful error handling confirmed (checksum/semantic validation logs warnings without exceptions) |

### Design Quality Gates

| Gate | Status | Validation |
|------|--------|------------|
| **Data Model Completeness** | ✅ PASS | All 14 GNGGA fields documented with types, validation rules, examples; factory method CreateFromFields specified; coordinate conversion algorithm detailed |
| **Event Contract Clarity** | ✅ PASS | GNGGAEventArgs structure defined: inherits NMEAEventArgs, contains GNGGAData property, constructor signature documented |
| **Parser Interface Compliance** | ✅ PASS | GNGGAParser overrides SentenceId ("GNGGA") and TryParse; parsing flow documented with 10-step sequence; error handling matrix provided (structural/checksum/semantic validation) |
| **Test Coverage Specification** | ✅ PASS | quickstart.md specifies 4 test methods covering event triggering, field validation, checksum handling, semantic warnings; test data examples provided with expected outputs |
| **User Documentation** | ✅ PASS | quickstart.md includes basic usage, advanced patterns (filtering, UTC handling, dual GPGGA/GNGGA), error handling, common pitfalls, troubleshooting guide |
| **Performance Documentation** | ✅ PASS | research.md confirms coordinate conversion algorithm reuse (proven performance), stateless design, no new dependencies; performance tips in quickstart.md |

### Design Decisions Alignment with Constitution

1. **Separation of Concerns** (Principle I): ✅
   - Dedicated GNGGAData class (not shared with GPGGA) per clarification decision
   - Single Responsibility: GNGGAParser handles only GNGGA sentences
   - Factory pattern isolates field parsing logic

2. **Testability** (Principle II): ✅
   - Mock-friendly design: INMEAInput injection allows test harness with NSubstitute
   - Event-driven: OnGNGGAUpdated can be tested via Raise.Event pattern
   - Test data provided: 2 real-world GNGGA sentences with expected coordinate conversions

3. **API Stability** (Principle III): ✅
   - Extension pattern: New parser/events added, zero modifications to existing types
   - Follows established conventions: GNGGAEventArgs mirrors GPGGAEventArgs structure
   - Version bump plan: 1.0.1 → 1.1.0 (MINOR - new feature)

4. **Performance** (Principle IV): ✅
   - Coordinate conversion: Reuses proven GPGGAData algorithm (no performance regression)
   - Memory: GNGGAData uses same nullable struct pattern as existing models
   - Throughput: Parsing logic identical complexity to GPGGA (should maintain 100+ sentences/sec)

5. **User Experience** (Principle V): ✅
   - Consistent patterns: GNGGAEventArgs.GNGGAData matches GPGGAEventArgs.GPGGAData structure
   - Dual UTC access: Original string (debugging) + DateTime (application logic) per user feedback
   - Graceful errors: Checksum warnings logged, semantic validation warnings logged, no exceptions thrown
   - ConsoleApp example: OnGNGGAUpdated.cs documented with window layout matching OnGPGGAUpdated.cs

### Risk Re-Assessment Post-Design

| Risk | Mitigation Status | Notes |
|------|------------------|-------|
| Coordinate conversion bugs | ✅ MITIGATED | Algorithm copied verbatim from GPGGAData; test sentence validation specified with expected values (34.0163166 ±0.0001) |
| Checksum validation errors | ✅ MITIGATED | Reuses BaseNMEAParser.GetFieldAndChecksum(); validation flow documented in data-model.md parsing sequence |
| UTC parsing edge cases | ✅ MITIGATED | Dual representation design eliminates single-format dependency; parsing handles both 6-digit and 8-digit formats |
| Test coverage gaps | ✅ MITIGATED | 4 test methods specified with explicit assertions; covers format variations, checksum scenarios, semantic warnings per FR-010 |

### Final Gate Decision

**Status**: ✅ **ALL GATES PASS** - Design ready for implementation

**Confidence Level**: HIGH
- Design artifacts complete (data-model.md, research.md, quickstart.md)
- All constitutional principles validated
- No architectural deviations from established patterns
- Test plan comprehensive and executable
- User documentation covers common scenarios and pitfalls

**Approval**: Proceed to Phase 2 task generation (`/speckit.tasks` command)
