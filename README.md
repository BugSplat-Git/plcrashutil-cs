# PLCrashUtil - .NET Library and CLI Tool

A comprehensive .NET library and command-line utility for parsing and converting PLCrash reports to iOS-compatible crash logs. Based on the original PLCrashReporter project, this C# implementation provides both programmatic API access via **PLCrashUtilLib** and a convenient CLI tool via **PLCrashUtil**.

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **PLCrashUtilLib** | 1.1.0 | Class library for programmatic integration |
| **PLCrashUtil** | 1.1.0 | Global CLI tool for command-line usage |

## Features

- ✅ Parse PLCrashReporter protobuf format files
- ✅ Convert to iOS-compatible text crash logs
- ✅ Access structured crash data programmatically
- ✅ Support for all PLCrash report versions
- ✅ Available as both .NET library and global CLI tool
- ✅ Cross-platform (.NET 6.0+)

## Installation

### CLI Tool

Install the global CLI tool:

```bash
dotnet tool install --global PLCrashUtil
```

### Library

Add the library to your .NET project:

```bash
dotnet add package PLCrashUtilLib
```

Or via PackageManager:

```xml
<PackageReference Include="PLCrashUtilLib" Version="1.1.0" />
```

## CLI Usage

Once installed as a global tool, use the `plcrashutil` command:

```bash
# Convert a PLCrash report to iOS format
plcrashutil convert --format=ios crash_report.plcrash

# Convert and save to file
plcrashutil convert --format=ios crash_report.plcrash > formatted_crash.txt
```

### CLI Options

- `--format=ios` or `--format=iphone` - Output iOS-compatible crash log format

## Library Usage

### Basic Example

```csharp
using PLCrashUtilLib.Models;

// Load and parse a crash report
byte[] crashData = File.ReadAllBytes("crash_report.plcrash");
var crashReport = new PLCrashReport(crashData);

// Access structured data
Console.WriteLine($"App: {crashReport.ApplicationInfo.ApplicationIdentifier}");
Console.WriteLine($"Version: {crashReport.ApplicationInfo.ApplicationVersion}");
Console.WriteLine($"Crashed Thread: {crashReport.Threads.First(t => t.Crashed).ThreadNumber}");

// Convert to formatted text
string formattedReport = PLCrashReportTextFormatter.StringValueForCrashReport(
    crashReport, 
    PLCrashReportTextFormat.PLCrashReportTextFormatiOS
);
Console.WriteLine(formattedReport);
```



## Building from Source

### Requirements

- .NET 6.0 SDK or later
- Protocol Buffers compiler (`protoc`) - only needed if regenerating protobuf files

### Build Steps

```bash
# Clone the repository
git clone https://github.com/BugSplatGit/plcrashutil-dotnet.git
cd plcrashutil-dotnet

# Build all projects
dotnet build

# Run unit tests
dotnet test

# Create NuGet packages
dotnet pack PLCrashUtilLib/PLCrashUtilLib.csproj --configuration Release
dotnet pack PLCrashUtil/PLCrashUtil.csproj --configuration Release
```

### Project Structure

- `PLCrashUtilLib/` - Core library with crash report parsing logic
- `PLCrashUtil/` - CLI tool that uses the library
- `PLCrashUtilLib.Tests/` - Unit tests for the library

### Regenerating Protobuf Files

If you need to update the protobuf definitions:

```bash
protoc --csharp_out=. PLCrashReport.proto
```

## Testing

### Unit Tests

The library includes comprehensive unit tests:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run tests for specific project
dotnet test PLCrashUtilLib.Tests/
```

### Manual Testing

Test the CLI tool with the included sample files:

```bash
# CLI tool (if installed globally)
plcrashutil convert --format=ios fuzz_report.plcrash

# From source
dotnet run --project PLCrashUtil -- convert --format=ios fuzz_report.plcrash
```

## Compatibility

- **Platforms:** Windows, macOS, Linux (any platform supporting .NET 6.0+)
- **PLCrash Versions:** All versions supported by PLCrashReporter
- **.NET Versions:** .NET 6.0 and later

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Based on the original PLCrashReporter project by Plausible Labs Cooperative, Inc.

Copyright (c) BugSplat LLC.

## Third-Party Dependencies

See [ThirdPartyNotices.txt](ThirdPartyNotices.txt) for information about third-party dependencies. 