# PLCrashUtil - C# Port

A C# port of the plcrashutil tool for converting PLCrash report files to readable formats.

## Requirements

- .NET 6.0 or later
- Protocol Buffers compiler (`protoc`) - only needed if regenerating protobuf files

## Building

```bash
dotnet build
```

Or build the solution:

```bash
dotnet build plcrashutil.sln
```

## Usage

Convert a PLCrash report file to iOS format:

```bash
dotnet run convert --format=ios <crash_file.plcrash>
```

Or using the built executable:

```bash
dotnet bin/Debug/net6.0/plcrashutil.dll convert --format=ios <crash_file.plcrash>
```

### Supported Formats

- `ios` - Standard Apple iOS-compatible text crash log
- `iphone` - Synonym for 'iOS'

### Example

Test with the included sample file:

```bash
dotnet run convert --format=ios fuzz_report.plcrash
```

## Files

- `Program.cs` - Main application entry point
- `PlCrashUtil.csproj` - Project file
- `Models/` - Data model classes for crash report components
- `PLCrashReport.proto` - Protocol buffer definition
- `PLCrashReport.cs` - Generated protobuf C# classes
- `fuzz_report.plcrash` - Sample crash file for testing

## Building from Source

If you need to regenerate the protobuf files:

```bash
protoc --csharp_out=. PLCrashReport.proto
```

## License

This C# port is developed by BugSplat LLC and is based on the original PLCrashReporter project.

Copyright (c) BugSplat LLC.

This project is licensed under the same MIT-style license as the original PLCrashReporter project. See the [LICENSE](LICENSE) file for the complete license terms.

For third-party dependencies, see [ThirdPartyNotices.txt](ThirdPartyNotices.txt). 