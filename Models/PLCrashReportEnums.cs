namespace PLCrashUtil.Models
{
    /// <summary>
    /// Indicates the Operating System under which a Crash Log was generated.
    /// </summary>
    public enum PLCrashReportOperatingSystem
    {
        /// <summary>Mac OS X</summary>
        PLCrashReportOperatingSystemMacOSX = 0,
        
        /// <summary>iPhone OS</summary>
        PLCrashReportOperatingSystemiPhoneOS = 1,
        
        /// <summary>iPhone Simulator (Mac OS X with additional simulator-specific runtime libraries)</summary>
        PLCrashReportOperatingSystemiPhoneSimulator = 2,
        
        /// <summary>Unknown operating system</summary>
        PLCrashReportOperatingSystemUnknown = 3,
        
        /// <summary>Apple tvOS</summary>
        PLCrashReportOperatingSystemAppleTVOS = 4
    }

    /// <summary>
    /// Legacy processor architecture type codes. These codes have been deprecated.
    /// The architecture value has been deprecated in v1.1 and later crash reports.
    /// </summary>
    public enum PLCrashReportArchitecture
    {
        /// <summary>x86-32</summary>
        PLCrashReportArchitectureX86_32 = 0,
        
        /// <summary>x86-64</summary>
        PLCrashReportArchitectureX86_64 = 1,
        
        /// <summary>ARMv6</summary>
        PLCrashReportArchitectureARMv6 = 2,
        
        /// <summary>PPC</summary>
        PLCrashReportArchitecturePPC = 3,
        
        /// <summary>PPC64</summary>
        PLCrashReportArchitecturePPC64 = 4,
        
        /// <summary>ARMv7</summary>
        PLCrashReportArchitectureARMv7 = 5,
        
        /// <summary>Unknown</summary>
        PLCrashReportArchitectureUnknown = 6
    }

    /// <summary>
    /// The type encodings supported for CPU types and subtypes. Currently only Apple
    /// Mach-O defined encodings are supported.
    /// </summary>
    public enum PLCrashReportProcessorTypeEncoding
    {
        /// <summary>Unknown cpu type encoding.</summary>
        PLCrashReportProcessorTypeEncodingUnknown = 0,
        
        /// <summary>Apple Mach-defined processor types.</summary>
        PLCrashReportProcessorTypeEncodingMach = 1
    }
} 