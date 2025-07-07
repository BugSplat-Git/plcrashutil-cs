using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PLCrashUtilLib.Models
{
    /// <summary>
    /// Formats PLCrashReport data as human-readable text.
    /// </summary>
    public static class PLCrashReportTextFormatter
    {
        // Apple Mach CPU types (from mach/machine.h)
        private const ulong CPU_TYPE_ARM = 12;
        private const ulong CPU_TYPE_ARM64 = 0x0100000C;
        private const ulong CPU_TYPE_X86 = 7;
        private const ulong CPU_TYPE_X86_64 = 0x01000007;
        private const ulong CPU_TYPE_POWERPC = 18;
        
        // ARM64 CPU subtypes
        private const ulong CPU_SUBTYPE_ARM64_ALL = 0;
        private const ulong CPU_SUBTYPE_ARM64_V8 = 1;
        private const ulong CPU_SUBTYPE_ARM64E = 2;
        
        private const ulong CPU_ARCH_MASK = 0xff000000;
        private const ulong CPU_SUBTYPE_MASK = 0xff000000;

        /// <summary>
        /// Formats the provided report as human-readable text in the given textFormat, and return
        /// the formatted result as a string.
        /// </summary>
        /// <param name="report">The report to format</param>
        /// <param name="textFormat">The text format to use</param>
        /// <returns>Returns the formatted result on success</returns>
        public static string StringValueForCrashReport(PLCrashReport report, PLCrashReportTextFormat textFormat)
        {
            var text = new StringBuilder();
            bool lp64 = true; // Default to 64-bit

            // Header

            // Map to apple style OS name
            string osName = report.SystemInfo.OperatingSystem switch
            {
                PLCrashReportOperatingSystem.PLCrashReportOperatingSystemMacOSX => "Mac OS X",
                PLCrashReportOperatingSystem.PLCrashReportOperatingSystemiPhoneOS => "iPhone OS",
                PLCrashReportOperatingSystem.PLCrashReportOperatingSystemiPhoneSimulator => "Mac OS X",
                PLCrashReportOperatingSystem.PLCrashReportOperatingSystemAppleTVOS => "Apple tvOS",
                _ => $"Unknown ({(int)report.SystemInfo.OperatingSystem})"
            };

            // Map to Apple-style code type, and mark whether architecture is LP64 (64-bit)
            string codeType = "Unknown";

            // Try to get the code type from machine info first (v1.1+ crash reports)
            if (report.HasMachineInfo && report.MachineInfo?.ProcessorInfo != null)
            {
                var processorInfo = report.MachineInfo.ProcessorInfo;
                if (processorInfo.TypeEncoding == PLCrashReportProcessorTypeEncoding.PLCrashReportProcessorTypeEncodingMach)
                {
                    var (type, is64Bit) = GetAppleCodeType(processorInfo.Type, processorInfo.Subtype);
                    codeType = type;
                    lp64 = is64Bit;
                }
            }
            else if (report.SystemInfo.ProcessorInfo.TypeEncoding == PLCrashReportProcessorTypeEncoding.PLCrashReportProcessorTypeEncodingMach)
            {
                // Fallback to system info processor info
                var processorInfo = report.SystemInfo.ProcessorInfo;
                var (type, is64Bit) = GetAppleCodeType(processorInfo.Type, processorInfo.Subtype);
                codeType = type;
                lp64 = is64Bit;
            }

            // Hardware model and incident identifier
            string hardwareModel = "???";
            if (report.HasMachineInfo && !string.IsNullOrEmpty(report.MachineInfo?.ModelName))
            {
                hardwareModel = report.MachineInfo.ModelName;
            }

            string incidentIdentifier = "???";
            if (report.UuidRef.HasValue)
            {
                incidentIdentifier = report.UuidRef.Value.ToString("D").ToUpper();
            }

            text.AppendLine($"Incident Identifier: {incidentIdentifier}");
            text.AppendLine($"Hardware Model:      {hardwareModel}");

            // Application and process info
            string unknownString = "???";
            string processName = unknownString;
            string processId = unknownString;
            string processPath = unknownString;
            string parentProcessName = unknownString;
            string parentProcessId = unknownString;

            // Process information was not available in earlier crash report versions
            if (report.HasProcessInfo && report.ProcessInfo != null)
            {
                if (!string.IsNullOrEmpty(report.ProcessInfo.ProcessName))
                    processName = report.ProcessInfo.ProcessName;

                processId = report.ProcessInfo.ProcessID.ToString();

                if (!string.IsNullOrEmpty(report.ProcessInfo.ProcessPath))
                    processPath = report.ProcessInfo.ProcessPath;

                if (!string.IsNullOrEmpty(report.ProcessInfo.ParentProcessName))
                    parentProcessName = report.ProcessInfo.ParentProcessName;

                parentProcessId = report.ProcessInfo.ParentProcessID.ToString();
            }

            string versionString = report.ApplicationInfo.ApplicationVersion;
            // Marketing version is optional
            if (!string.IsNullOrEmpty(report.ApplicationInfo.ApplicationMarketingVersion))
            {
                versionString = $"{report.ApplicationInfo.ApplicationMarketingVersion} ({report.ApplicationInfo.ApplicationVersion})";
            }

            text.AppendLine($"Process:         {processName} [{processId}]");
            text.AppendLine($"Path:            {processPath}");
            text.AppendLine($"Identifier:      {report.ApplicationInfo.ApplicationIdentifier}");
            text.AppendLine($"Version:         {versionString}");
            text.AppendLine($"Code Type:       {codeType}");
            text.AppendLine($"Parent Process:  {parentProcessName} [{parentProcessId}]");
            text.AppendLine();

            // System info
            string osBuild = "???";
            if (!string.IsNullOrEmpty(report.SystemInfo.OperatingSystemBuild))
            {
                osBuild = report.SystemInfo.OperatingSystemBuild;
            }

            string timestamp = report.SystemInfo.Timestamp?.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") ?? "???";

            text.AppendLine($"Date/Time:       {timestamp}");
            text.AppendLine($"OS Version:      {osName} {report.SystemInfo.OperatingSystemVersion} ({osBuild})");
            text.AppendLine("Report Version:  104");
            text.AppendLine();

            // Exception code
            text.AppendLine($"Exception Type:  {report.SignalInfo.Name}");
            text.AppendLine($"Exception Codes: {report.SignalInfo.Code} at 0x{report.SignalInfo.Address:x16}");

            var crashedThread = report.Threads.FirstOrDefault(t => t.Crashed);
            if (crashedThread != null)
            {
                text.AppendLine($"Crashed Thread:  {crashedThread.ThreadNumber}");
            }
            text.AppendLine();

            // Uncaught Exception
            if (report.HasExceptionInfo && report.ExceptionInfo != null)
            {
                text.AppendLine("Application Specific Information:");
                text.AppendLine($"*** Terminating app due to uncaught exception '{report.ExceptionInfo.ExceptionName}', reason: '{report.ExceptionInfo.ExceptionReason}'");
                text.AppendLine();
            }

            // If an exception stack trace is available, output an Apple-compatible backtrace
            if (report.ExceptionInfo?.StackFrames != null && report.ExceptionInfo.StackFrames.Length > 0)
            {
                text.AppendLine("Last Exception Backtrace:");
                for (int i = 0; i < report.ExceptionInfo.StackFrames.Length; i++)
                {
                    var frameInfo = report.ExceptionInfo.StackFrames[i];
                    text.Append(FormatStackFrame(frameInfo, i, report, lp64));
                }
                text.AppendLine();
            }

            // Threads
            int maxThreadNum = 0;
            foreach (var thread in report.Threads)
            {
                if (thread.Crashed)
                {
                    text.AppendLine($"Thread {thread.ThreadNumber} Crashed:");
                }
                else
                {
                    text.AppendLine($"Thread {thread.ThreadNumber}:");
                }

                for (int i = 0; i < thread.StackFrames.Length; i++)
                {
                    var frameInfo = thread.StackFrames[i];
                    text.Append(FormatStackFrame(frameInfo, i, report, lp64));
                }
                text.AppendLine();

                maxThreadNum = Math.Max(maxThreadNum, thread.ThreadNumber);
            }

            // Registers
            if (crashedThread != null)
            {
                text.AppendLine($"Thread {crashedThread.ThreadNumber} crashed with {codeType} Thread State:");

                int regColumn = 0;
                foreach (var reg in crashedThread.Registers)
                {
                    string regFmt = lp64 ? "{0,6}: 0x{1:x16} " : "{0,6}: 0x{1:x8} ";

                    // Remap register names to match Apple's crash reports
                    string regName = reg.RegisterName;
                    if (report.HasMachineInfo && report.MachineInfo?.ProcessorInfo != null)
                    {
                        var pinfo = report.MachineInfo.ProcessorInfo;
                        var archType = pinfo.Type & ~CPU_ARCH_MASK;

                        // Apple uses 'ip' rather than 'r12' on ARM
                        if (archType == CPU_TYPE_ARM && regName == "r12")
                        {
                            regName = "ip";
                        }
                    }

                    text.AppendFormat(regFmt, regName, reg.RegisterValue);

                    regColumn++;
                    if (regColumn == 4)
                    {
                        text.AppendLine();
                        regColumn = 0;
                    }
                }

                if (regColumn != 0)
                    text.AppendLine();

                text.AppendLine();
            }

            // Images. The iPhone crash report format sorts these in ascending order, by the base address
            text.AppendLine("Binary Images:");

            var sortedImages = report.Images.OrderBy(img => img.ImageBaseAddress).ToArray();
            foreach (var imageInfo in sortedImages)
            {
                string archName = "???";

                if (imageInfo.CodeType?.TypeEncoding == PLCrashReportProcessorTypeEncoding.PLCrashReportProcessorTypeEncodingMach)
                {
                    archName = GetArchitectureName(imageInfo.CodeType.Type, imageInfo.CodeType.Subtype);
                }

                // Determine if this is the main executable
                string binaryDesignator = " ";
                if (report.HasProcessInfo && 
                    report.ProcessInfo != null && 
                    imageInfo.ImageName == report.ProcessInfo.ProcessPath)
                {
                    binaryDesignator = "+";
                }

                string uuid = imageInfo.ImageUUID ?? "???";

                // base_address - terminating_address [designator]file_name arch <uuid> file_path
                string fmt = lp64 ? 
                    "0x{0,16:x} - 0x{1,16:x} {2}{3} {4}  <{5}> {6}" : 
                    "0x{0,8:x} - 0x{1,8:x} {2}{3} {4}  <{5}> {6}";

                var imageName = Path.GetFileName(imageInfo.ImageName);
                var terminatingAddress = imageInfo.ImageBaseAddress + Math.Max(1UL, imageInfo.ImageSize) - 1;

                text.AppendLine(string.Format(fmt,
                    imageInfo.ImageBaseAddress,
                    terminatingAddress,
                    binaryDesignator,
                    imageName,
                    archName,
                    uuid,
                    imageInfo.ImageName));
            }

            return text.ToString();
        }

        private static (string codeType, bool lp64) GetAppleCodeType(ulong type, ulong subtype)
        {
            return type switch
            {
                CPU_TYPE_ARM => ("ARM", false),
                CPU_TYPE_ARM64 => GetArm64CodeType(subtype),
                CPU_TYPE_X86 => ("X86", false),
                CPU_TYPE_X86_64 => ("X86-64", true),
                CPU_TYPE_POWERPC => ("PPC", false),
                _ => ($"Unknown ({type})", true)
            };
        }

        private static (string codeType, bool lp64) GetArm64CodeType(ulong subtype)
        {
            var cleanSubtype = subtype & ~CPU_SUBTYPE_MASK;
            return cleanSubtype switch
            {
                CPU_SUBTYPE_ARM64_ALL => ("ARM-64", true),
                CPU_SUBTYPE_ARM64_V8 => ("ARM-64", true),
                CPU_SUBTYPE_ARM64E => ("ARM-64", true),
                _ => ("ARM-64", true)
            };
        }

        private static string GetArchitectureName(ulong type, ulong subtype)
        {
            return type switch
            {
                CPU_TYPE_ARM => "armv7", // Default for ARM
                CPU_TYPE_ARM64 => GetArm64ArchName(subtype),
                CPU_TYPE_X86 => "i386",
                CPU_TYPE_X86_64 => "x86_64",
                CPU_TYPE_POWERPC => "powerpc",
                _ => "unknown"
            };
        }

        private static string GetArm64ArchName(ulong subtype)
        {
            var cleanSubtype = subtype & ~CPU_SUBTYPE_MASK;
            return cleanSubtype switch
            {
                CPU_SUBTYPE_ARM64_ALL => "arm64",
                CPU_SUBTYPE_ARM64_V8 => "armv8",
                CPU_SUBTYPE_ARM64E => "arm64e",
                _ => "arm64-unknown"
            };
        }

        private static string FormatStackFrame(PLCrashReportStackFrameInfo frameInfo, int frameIndex, PLCrashReport report, bool lp64)
        {
            // Base image address containing instruction pointer, offset of the IP from that base
            // address, and the associated image name
            ulong baseAddress = 0x0;
            ulong pcOffset = 0x0;
            string imageName = "???";
            string symbolString;

            var imageInfo = report.ImageForAddress(frameInfo.InstructionPointer);
            if (imageInfo != null)
            {
                imageName = Path.GetFileName(imageInfo.ImageName);
                baseAddress = imageInfo.ImageBaseAddress;
                pcOffset = frameInfo.InstructionPointer - imageInfo.ImageBaseAddress;
            }

            // If symbol info is available, the format used in Apple's reports is Sym + OffsetFromSym. Otherwise,
            // the format used is imageBaseAddress + offsetToIP
            if (frameInfo.SymbolInfo != null)
            {
                string symbolName = frameInfo.SymbolInfo.SymbolName;

                // Apple strips the _ symbol prefix in their reports.
                if (symbolName.StartsWith("_") && symbolName.Length > 1)
                {
                    switch (report.SystemInfo.OperatingSystem)
                    {
                        case PLCrashReportOperatingSystem.PLCrashReportOperatingSystemMacOSX:
                        case PLCrashReportOperatingSystem.PLCrashReportOperatingSystemiPhoneOS:
                        case PLCrashReportOperatingSystem.PLCrashReportOperatingSystemAppleTVOS:
                        case PLCrashReportOperatingSystem.PLCrashReportOperatingSystemiPhoneSimulator:
                            symbolName = symbolName.Substring(1);
                            break;
                    }
                }

                ulong symOffset = frameInfo.InstructionPointer - frameInfo.SymbolInfo.StartAddress;
                symbolString = $"{symbolName} + {symOffset}";
            }
            else
            {
                symbolString = $"0x{baseAddress:x} + {pcOffset}";
            }

            // Format: frame_index image_name instruction_pointer symbol_string
            var paddedImageName = imageName.PadRight(35);
            if (paddedImageName.Length > 35)
                paddedImageName = paddedImageName.Substring(0, 35);

            var ipWidth = lp64 ? 16 : 8;
            var ipFormat = $"0x{{0:x{ipWidth}}}";

            return $"{frameIndex,-4}{paddedImageName} {string.Format(ipFormat, frameInfo.InstructionPointer)} {symbolString}\n";
        }
    }
}