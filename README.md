# PLCrashUtil - .NET Library and CLI Tool

A comprehensive .NET library and command-line utility for parsing and converting PLCrash reports to iOS-compatible crash logs. Based on the original PLCrashReporter project, this C# implementation provides both programmatic API access and a convenient CLI tool.

## Features

- ✅ Parse PLCrashReporter protobuf format files
- ✅ Convert to iOS-compatible text crash logs
- ✅ Access structured crash data programmatically
- ✅ Support for all PLCrash report versions
- ✅ Available as both .NET library and global CLI tool
- ✅ Cross-platform (.NET 6.0+)

## Installation

### As a Global CLI Tool

Install globally using the .NET CLI:

```bash
dotnet tool install --global PLCrashUtil
```

### As a Library Dependency

Add to your .NET project:

```bash
dotnet add package PLCrashUtil
```

Or via PackageManager:

```xml
<PackageReference Include="PLCrashUtil" Version="1.0.0" />
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
using PLCrashUtil.Models;

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
git clone https://github.com/BugSplatGit/plcrashutil-cs.git
cd plcrashutil-cs

# Build the project
dotnet build

# Run tests (if any)
dotnet test

# Create NuGet package
dotnet pack
```

### Regenerating Protobuf Files

If you need to update the protobuf definitions:

```bash
protoc --csharp_out=. PLCrashReport.proto
```

## Testing

Test the tool with the included sample file:

```bash
# CLI tool
plcrashutil convert --format=ios fuzz_report.plcrash

# From source
dotnet run -- convert --format=ios fuzz_report.plcrash
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