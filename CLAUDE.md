# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GPSReader is a C# library for parsing NMEA 0183 sentences from GPS devices. It's published as a NuGet package and supports both real-time serial port reading and file-based simulation.

## Solution Structure

The solution contains three projects:
- **GPSReader** - Main library (C# / .NET 8.0)
- **GPSReader.Tests** - Test project using xUnit, FluentAssertions, and NSubstitute
- **ConsoleApp** - Example application demonstrating library usage (in Examples folder)

## Build and Test Commands

```bash
# Build the entire solution
dotnet build GPSReader.sln

# Build a specific project
dotnet build GPSReader/GPSReader.csproj
dotnet build GPSReader.Tests/GPSReader.Tests.csproj

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run the example console app
dotnet run --project ConsoleApp/ConsoleApp.csproj
```

## Architecture

### Core Components

**GPSReaderService** (`GPSReader.cs:7`)
- Main entry point for the library
- Takes an `INMEAInput` source and a list of `BaseNMEAParser` parsers
- Event-driven architecture with events for each sentence type (OnGPGGAUpdated, OnGNGGAUpdated, OnGPGSAUpdated, OnGPGLLUpdated, OnGPGSVUpdated)
- Default constructor provides all standard parsers automatically

**INMEAInput Interface** (`Interfaces/INMEAInput.cs:3`)
- Abstraction for data sources (serial port or file)
- Implementations: `SerialInput` and `FileInput`
- Provides `DataReceived` event for streaming data

**BaseNMEAParser Abstract Class** (`Abstracts/BaseNMEAParser.cs:6`)
- Base for all sentence parsers
- Defines `SentenceId` and `TryParse` method
- Provides `GetFieldAndChecksum` helper for parsing NMEA sentences

### Parser Pattern

Each NMEA sentence type has:
1. A parser class (e.g., `GPGGAParser`, `GNGGAParser`) extending `BaseNMEAParser`
2. A data model (e.g., `GPGGAData`, `GNGGAData`) extending `NMEAData`
3. An event args class (e.g., `GPGGAEventArgs`, `GNGGAEventArgs`) extending `NMEAEventArgs`

### Supported NMEA Sentences

- GPGGA - GPS Fix Data
- GNGGA - Global Navigation Satellite System Fix Data
- GPGSA - GPS DOP and Active Satellites
- GPGLL - Geographic Position
- GPGSV - GPS Satellites in View (accumulates multiple messages)

## Adding New NMEA Sentence Support

To add a new sentence type (e.g., GPRMC):
1. Create model: `Models/GPRMCData.cs` extending `NMEAData`
2. Create event args: `EventArgs/GPRMCEventArgs.cs` extending `NMEAEventArgs`
3. Create parser: `Parsers/GPRMCParser.cs` extending `BaseNMEAParser`
4. Add event to `GPSReaderService`: `public event EventHandler<GPRMCEventArgs>? OnGPRMCUpdated;`
5. Add case in `DataReceived` method to invoke the event
6. Add parser to default constructor list if it should be included by default
7. Create unit tests in `GPSReader.Tests/Tests.cs`

## Testing

The test project uses:
- **xUnit** for test framework
- **FluentAssertions** for readable assertions
- **NSubstitute** for mocking
- **MockInputSource** (`GPSReader.Tests/MockInputSource.cs`) - Custom mock input for testing

## Project Configuration

- Target Framework: .NET 8.0
- Nullable Reference Types: Enabled
- Implicit Usings: Enabled
- NuGet Package: Published to nuget.org as "GPSReader"
- License: MIT

## Feature Development

This project uses a spec-driven workflow with feature specs in `specs/` directory. Each feature has:
- `spec.md` - Feature specification
- `plan.md` - Implementation plan
- `tasks.md` - Task breakdown
- `checklists/` - Requirement checklists

## Dependencies

**Main Library:**
- Microsoft.Extensions.Logging 7.0.0
- System.IO.Ports 8.0.0

**Test Project:**
- xUnit 2.5.1
- FluentAssertions 6.12.0
- NSubstitute 5.1.0

**Console App:**
- Serilog (logging)
- Terminal.Gui 1.16.0 (TUI)
