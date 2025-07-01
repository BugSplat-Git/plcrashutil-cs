using System;

namespace PLCrashUtil.Models
{
    /// <summary>
    /// Crash log system information.
    /// </summary>
    public class PLCrashReportSystemInfo
    {
        public PLCrashReportSystemInfo(
            PLCrashReportOperatingSystem operatingSystem,
            string operatingSystemVersion,
            string? operatingSystemBuild,
            PLCrashReportArchitecture architecture,
            PLCrashReportProcessorInfo processorInfo,
            DateTime? timestamp)
        {
            OperatingSystem = operatingSystem;
            OperatingSystemVersion = operatingSystemVersion;
            OperatingSystemBuild = operatingSystemBuild;
            Architecture = architecture;
            ProcessorInfo = processorInfo;
            Timestamp = timestamp;
        }

        /// <summary>The operating system.</summary>
        public PLCrashReportOperatingSystem OperatingSystem { get; }

        /// <summary>The operating system's release version.</summary>
        public string OperatingSystemVersion { get; }

        /// <summary>The operating system's build identifier (eg, 10J869). This may be unavailable.</summary>
        public string? OperatingSystemBuild { get; }

        /// <summary>Architecture. (Deprecated in v1.1+ in favor of ProcessorInfo)</summary>
        public PLCrashReportArchitecture Architecture { get; }

        /// <summary>Processor information.</summary>
        public PLCrashReportProcessorInfo ProcessorInfo { get; }

        /// <summary>Date and time that the crash report was generated. May be null if unavailable.</summary>
        public DateTime? Timestamp { get; }
    }

    /// <summary>
    /// Crash log processor information.
    /// </summary>
    public class PLCrashReportProcessorInfo
    {
        public PLCrashReportProcessorInfo(
            PLCrashReportProcessorTypeEncoding typeEncoding,
            ulong type,
            ulong subtype)
        {
            TypeEncoding = typeEncoding;
            Type = type;
            Subtype = subtype;
        }

        /// <summary>CPU type encoding.</summary>
        public PLCrashReportProcessorTypeEncoding TypeEncoding { get; }

        /// <summary>CPU type.</summary>
        public ulong Type { get; }

        /// <summary>CPU subtype.</summary>
        public ulong Subtype { get; }
    }

    /// <summary>
    /// Crash log machine information.
    /// </summary>
    public class PLCrashReportMachineInfo
    {
        public PLCrashReportMachineInfo(
            string? modelName,
            PLCrashReportProcessorInfo? processorInfo,
            uint processorCount,
            uint logicalProcessorCount)
        {
            ModelName = modelName;
            ProcessorInfo = processorInfo;
            ProcessorCount = processorCount;
            LogicalProcessorCount = logicalProcessorCount;
        }

        /// <summary>Hardware model (eg, MacBookPro6,1). May be null if unavailable.</summary>
        public string? ModelName { get; }

        /// <summary>Host processor information. May be null if unavailable.</summary>
        public PLCrashReportProcessorInfo? ProcessorInfo { get; }

        /// <summary>The number of actual physical processor cores.</summary>
        public uint ProcessorCount { get; }

        /// <summary>The number of logical processors.</summary>
        public uint LogicalProcessorCount { get; }
    }

    /// <summary>
    /// Crash log application information.
    /// </summary>
    public class PLCrashReportApplicationInfo
    {
        public PLCrashReportApplicationInfo(
            string applicationIdentifier,
            string applicationVersion,
            string? applicationMarketingVersion)
        {
            ApplicationIdentifier = applicationIdentifier;
            ApplicationVersion = applicationVersion;
            ApplicationMarketingVersion = applicationMarketingVersion;
        }

        /// <summary>The application identifier. This is usually the application's CFBundleIdentifier value.</summary>
        public string ApplicationIdentifier { get; }

        /// <summary>The application version. This is usually the application's CFBundleVersion value.</summary>
        public string ApplicationVersion { get; }

        /// <summary>The application marketing version. This is usually the application's CFBundleShortVersionString value if available.</summary>
        public string? ApplicationMarketingVersion { get; }
    }

    /// <summary>
    /// Crash log process information.
    /// </summary>
    public class PLCrashReportProcessInfo
    {
        public PLCrashReportProcessInfo(
            string? processName,
            uint processID,
            string? processPath,
            DateTime? processStartTime,
            string? parentProcessName,
            uint parentProcessID,
            bool native)
        {
            ProcessName = processName;
            ProcessID = processID;
            ProcessPath = processPath;
            ProcessStartTime = processStartTime;
            ParentProcessName = parentProcessName;
            ParentProcessID = parentProcessID;
            Native = native;
        }

        /// <summary>Application process name. May be null if unavailable.</summary>
        public string? ProcessName { get; }

        /// <summary>Application process ID.</summary>
        public uint ProcessID { get; }

        /// <summary>Application process path. May be null if unavailable.</summary>
        public string? ProcessPath { get; }

        /// <summary>Application process start time. May be null if unavailable.</summary>
        public DateTime? ProcessStartTime { get; }

        /// <summary>Application parent process name. May be null if unavailable.</summary>
        public string? ParentProcessName { get; }

        /// <summary>Application parent process ID.</summary>
        public uint ParentProcessID { get; }

        /// <summary>If false, the process is being run via process-level CPU emulation (such as Rosetta).</summary>
        public bool Native { get; }
    }

    /// <summary>
    /// Crash log signal information.
    /// </summary>
    public class PLCrashReportSignalInfo
    {
        public PLCrashReportSignalInfo(string name, string code, ulong address)
        {
            Name = name;
            Code = code;
            Address = address;
        }

        /// <summary>The signal name.</summary>
        public string Name { get; }

        /// <summary>The signal code.</summary>
        public string Code { get; }

        /// <summary>The faulting instruction or address.</summary>
        public ulong Address { get; }
    }

    /// <summary>
    /// Crash log Mach exception information.
    /// </summary>
    public class PLCrashReportMachExceptionInfo
    {
        public PLCrashReportMachExceptionInfo(ulong type, ulong[] codes)
        {
            Type = type;
            Codes = codes;
        }

        /// <summary>Mach exception type.</summary>
        public ulong Type { get; }

        /// <summary>Mach exception codes.</summary>
        public ulong[] Codes { get; }
    }

    /// <summary>
    /// Crash log symbol information.
    /// </summary>
    public class PLCrashReportSymbolInfo
    {
        public PLCrashReportSymbolInfo(string symbolName, ulong startAddress, ulong endAddress)
        {
            SymbolName = symbolName;
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        /// <summary>The symbol name.</summary>
        public string SymbolName { get; }

        /// <summary>The symbol start address.</summary>
        public ulong StartAddress { get; }

        /// <summary>The symbol end address. May be 0 if unavailable.</summary>
        public ulong EndAddress { get; }
    }

    /// <summary>
    /// Crash log stack frame information.
    /// </summary>
    public class PLCrashReportStackFrameInfo
    {
        public PLCrashReportStackFrameInfo(ulong instructionPointer, PLCrashReportSymbolInfo? symbolInfo)
        {
            InstructionPointer = instructionPointer;
            SymbolInfo = symbolInfo;
        }

        /// <summary>Frame instruction pointer.</summary>
        public ulong InstructionPointer { get; }

        /// <summary>Frame symbol information. May be null if unavailable.</summary>
        public PLCrashReportSymbolInfo? SymbolInfo { get; }
    }

    /// <summary>
    /// Crash log register information.
    /// </summary>
    public class PLCrashReportRegisterInfo
    {
        public PLCrashReportRegisterInfo(string registerName, ulong registerValue)
        {
            RegisterName = registerName;
            RegisterValue = registerValue;
        }

        /// <summary>Register name.</summary>
        public string RegisterName { get; }

        /// <summary>Register value.</summary>
        public ulong RegisterValue { get; }
    }

    /// <summary>
    /// Crash log per-thread state information.
    /// </summary>
    public class PLCrashReportThreadInfo
    {
        public PLCrashReportThreadInfo(
            int threadNumber,
            PLCrashReportStackFrameInfo[] stackFrames,
            bool crashed,
            PLCrashReportRegisterInfo[] registers)
        {
            ThreadNumber = threadNumber;
            StackFrames = stackFrames;
            Crashed = crashed;
            Registers = registers;
        }

        /// <summary>Application thread number.</summary>
        public int ThreadNumber { get; }

        /// <summary>Thread backtrace. Provides an array of PLCrashReportStackFrameInfo instances.</summary>
        public PLCrashReportStackFrameInfo[] StackFrames { get; }

        /// <summary>If this thread crashed, set to true.</summary>
        public bool Crashed { get; }

        /// <summary>State of the general purpose and related registers.</summary>
        public PLCrashReportRegisterInfo[] Registers { get; }
    }

    /// <summary>
    /// Crash log binary image information.
    /// </summary>
    public class PLCrashReportBinaryImageInfo
    {
        public PLCrashReportBinaryImageInfo(
            PLCrashReportProcessorInfo? codeType,
            ulong imageBaseAddress,
            ulong imageSize,
            string imageName,
            string? imageUUID)
        {
            CodeType = codeType;
            ImageBaseAddress = imageBaseAddress;
            ImageSize = imageSize;
            ImageName = imageName;
            ImageUUID = imageUUID;
        }

        /// <summary>Image code type, or null if unavailable.</summary>
        public PLCrashReportProcessorInfo? CodeType { get; }

        /// <summary>Image base address.</summary>
        public ulong ImageBaseAddress { get; }

        /// <summary>Segment size.</summary>
        public ulong ImageSize { get; }

        /// <summary>Image name (absolute path).</summary>
        public string ImageName { get; }

        /// <summary>YES if this image has an associated UUID.</summary>
        public bool HasImageUUID => !string.IsNullOrEmpty(ImageUUID);

        /// <summary>128-bit object UUID (matches Mach-O DWARF dSYM files). May be null if unavailable.</summary>
        public string? ImageUUID { get; }
    }

    /// <summary>
    /// Crash log exception information.
    /// </summary>
    public class PLCrashReportExceptionInfo
    {
        public PLCrashReportExceptionInfo(
            string exceptionName,
            string exceptionReason,
            PLCrashReportStackFrameInfo[]? stackFrames = null)
        {
            ExceptionName = exceptionName;
            ExceptionReason = exceptionReason;
            StackFrames = stackFrames;
        }

        /// <summary>The exception name.</summary>
        public string ExceptionName { get; }

        /// <summary>The exception reason.</summary>
        public string ExceptionReason { get; }

        /// <summary>The exception's original call stack, or null if unavailable.</summary>
        public PLCrashReportStackFrameInfo[]? StackFrames { get; }
    }
} 