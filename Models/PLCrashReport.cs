using Google.Protobuf;
using PLCrashUtil.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PLCrashUtil.Models
{
    /// <summary>
    /// Crash file magic identifier
    /// </summary>
    public static class PLCrashReportConstants
    {
        public const string PLCRASH_REPORT_FILE_MAGIC = "plcrash";
        public const byte PLCRASH_REPORT_FILE_VERSION = 1;
    }

    /// <summary>
    /// Represents a parsed PLCrash crash report.
    /// </summary>
    public class PLCrashReport
    {
        private readonly CrashReport _crashReport;

        /// <summary>
        /// Initialize with the provided crash log data.
        /// </summary>
        /// <param name="encodedData">Encoded plcrash crash log data</param>
        /// <exception cref="InvalidDataException">Thrown when the crash log data is invalid</exception>
        public PLCrashReport(byte[] encodedData)
        {
            _crashReport = DecodeCrashData(encodedData);
        }

        /// <summary>
        /// System information.
        /// </summary>
        public PLCrashReportSystemInfo SystemInfo { get; private set; } = null!;

        /// <summary>
        /// YES if machine information is available.
        /// </summary>
        public bool HasMachineInfo => MachineInfo != null;

        /// <summary>
        /// Machine information. Only available in later (v1.1+) crash report format versions.
        /// </summary>
        public PLCrashReportMachineInfo? MachineInfo { get; private set; }

        /// <summary>
        /// Application information.
        /// </summary>
        public PLCrashReportApplicationInfo ApplicationInfo { get; private set; } = null!;

        /// <summary>
        /// YES if process information is available.
        /// </summary>
        public bool HasProcessInfo => ProcessInfo != null;

        /// <summary>
        /// Process information. Only available in later (v1.1+) crash report format versions.
        /// </summary>
        public PLCrashReportProcessInfo? ProcessInfo { get; private set; }

        /// <summary>
        /// Signal information. This provides the signal and signal code of the fatal signal.
        /// </summary>
        public PLCrashReportSignalInfo SignalInfo { get; private set; } = null!;

        /// <summary>
        /// Mach exception information, if available.
        /// </summary>
        public PLCrashReportMachExceptionInfo? MachExceptionInfo { get; private set; }

        /// <summary>
        /// Thread information. Returns a list of PLCrashReportThreadInfo instances.
        /// </summary>
        public PLCrashReportThreadInfo[] Threads { get; private set; } = null!;

        /// <summary>
        /// Binary image information. Returns a list of PLCrashReportBinaryImageInfo instances.
        /// </summary>
        public PLCrashReportBinaryImageInfo[] Images { get; private set; } = null!;

        /// <summary>
        /// YES if exception information is available.
        /// </summary>
        public bool HasExceptionInfo => ExceptionInfo != null;

        /// <summary>
        /// Exception information. Only available if a crash was caused by an uncaught exception.
        /// </summary>
        public PLCrashReportExceptionInfo? ExceptionInfo { get; private set; }

        /// <summary>
        /// Custom user data. Only available if user explicitly assigned it before crash happened.
        /// </summary>
        public byte[]? CustomData { get; private set; }

        /// <summary>
        /// A client-generated 16-byte UUID. May be used to filter duplicate reports submitted or generated
        /// by a single client. Only available in later (v1.2+) crash report format versions.
        /// </summary>
        public Guid? UuidRef { get; private set; }

        /// <summary>
        /// Return the binary image containing the given address, or null if no binary image is found.
        /// </summary>
        /// <param name="address">The address to search for</param>
        /// <returns>Binary image info or null</returns>
        public PLCrashReportBinaryImageInfo? ImageForAddress(ulong address)
        {
            foreach (var imageInfo in Images)
            {
                ulong normalizedBaseAddress = imageInfo.ImageBaseAddress;
                if (normalizedBaseAddress <= address && 
                    address < (normalizedBaseAddress + imageInfo.ImageSize))
                {
                    return imageInfo;
                }
            }

            return null;
        }

        private CrashReport DecodeCrashData(byte[] data)
        {
            if (data.Length < 8) // magic (7) + version (1)
            {
                throw new InvalidDataException("Could not decode truncated crash log");
            }

            // Check the file magic
            var magic = Encoding.ASCII.GetString(data, 0, 7);
            if (magic != PLCrashReportConstants.PLCRASH_REPORT_FILE_MAGIC)
            {
                throw new InvalidDataException("Could not decode invalid crash log header");
            }

            // Check the version
            var version = data[7];
            if (version != PLCrashReportConstants.PLCRASH_REPORT_FILE_VERSION)
            {
                throw new InvalidDataException($"Could not decode unsupported crash report version: {version}");
            }

            // Parse the protobuf data
            var protobufData = new ReadOnlySpan<byte>(data, 8, data.Length - 8);
            var crashReport = CrashReport.Parser.ParseFrom(protobufData.ToArray());

            // Extract all the information
            ExtractInformation(crashReport);

            return crashReport;
        }

        private void ExtractInformation(CrashReport crashReport)
        {
            // Report UUID
            if (crashReport.ReportInfo?.Uuid?.Length == 16)
            {
                UuidRef = new Guid(crashReport.ReportInfo.Uuid.ToByteArray());
            }

            // Machine info
            if (crashReport.MachineInfo != null)
            {
                MachineInfo = ExtractMachineInfo(crashReport.MachineInfo);
            }

            // System info
            SystemInfo = ExtractSystemInfo(crashReport.SystemInfo, MachineInfo?.ProcessorInfo);

            // Application info
            ApplicationInfo = ExtractApplicationInfo(crashReport.ApplicationInfo);

            // Process info
            if (crashReport.ProcessInfo != null)
            {
                ProcessInfo = ExtractProcessInfo(crashReport.ProcessInfo);
            }

            // Signal info
            SignalInfo = ExtractSignalInfo(crashReport.Signal);

            // Mach exception info
            if (crashReport.Signal?.MachException != null)
            {
                MachExceptionInfo = ExtractMachExceptionInfo(crashReport.Signal.MachException);
            }

            // Thread info
            Threads = ExtractThreadInfo(crashReport);

            // Image info
            Images = ExtractImageInfo(crashReport);

            // Exception info
            if (crashReport.Exception != null)
            {
                ExceptionInfo = ExtractExceptionInfo(crashReport.Exception);
            }

            // Custom data
            if (crashReport.CustomData?.Length > 0)
            {
                CustomData = crashReport.CustomData.ToByteArray();
            }
        }

        private PLCrashReportSystemInfo ExtractSystemInfo(CrashReport.Types.SystemInfo systemInfo, PLCrashReportProcessorInfo? processorInfo)
        {
            var timestamp = systemInfo.Timestamp != 0 ? DateTimeOffset.FromUnixTimeSeconds(systemInfo.Timestamp).DateTime : (DateTime?)null;

            // For v1 crash logs without machine info, synthesize processor info from architecture
            if (processorInfo == null)
            {
                processorInfo = SynthesizeProcessorInfoFromArchitecture(systemInfo.Architecture);
            }

            return new PLCrashReportSystemInfo(
                (PLCrashReportOperatingSystem)systemInfo.OperatingSystem,
                systemInfo.OsVersion,
                string.IsNullOrEmpty(systemInfo.OsBuild) ? null : systemInfo.OsBuild,
                (PLCrashReportArchitecture)systemInfo.Architecture,
                processorInfo,
                timestamp
            );
        }

        private PLCrashReportProcessorInfo SynthesizeProcessorInfoFromArchitecture(Architecture architecture)
        {
            // Default values for unknown architectures
            var typeEncoding = PLCrashReportProcessorTypeEncoding.PLCrashReportProcessorTypeEncodingMach;
            ulong type = 0;
            ulong subtype = 0;

            switch (architecture)
            {
                case Architecture.X8632:
                    type = 7; // CPU_TYPE_X86
                    subtype = 3; // CPU_SUBTYPE_X86_ALL
                    break;
                case Architecture.X8664:
                    type = 0x01000007; // CPU_TYPE_X86_64
                    subtype = 3; // CPU_SUBTYPE_X86_64_ALL
                    break;
                case Architecture.Armv6:
                    type = 12; // CPU_TYPE_ARM
                    subtype = 6; // CPU_SUBTYPE_ARM_V6
                    break;
                case Architecture.Armv7:
                    type = 12; // CPU_TYPE_ARM
                    subtype = 9; // CPU_SUBTYPE_ARM_V7
                    break;
                case Architecture.Ppc:
                    type = 18; // CPU_TYPE_POWERPC
                    subtype = 0; // CPU_SUBTYPE_POWERPC_ALL
                    break;
                case Architecture.Ppc64:
                    type = 0x01000012; // CPU_TYPE_POWERPC64
                    subtype = 0; // CPU_SUBTYPE_POWERPC_ALL
                    break;
                default:
                    typeEncoding = PLCrashReportProcessorTypeEncoding.PLCrashReportProcessorTypeEncodingUnknown;
                    break;
            }

            return new PLCrashReportProcessorInfo(typeEncoding, type, subtype);
        }

        private PLCrashReportMachineInfo ExtractMachineInfo(CrashReport.Types.MachineInfo machineInfo)
        {
            PLCrashReportProcessorInfo? processorInfo = null;
            if (machineInfo.Processor != null)
            {
                processorInfo = new PLCrashReportProcessorInfo(
                    (PLCrashReportProcessorTypeEncoding)machineInfo.Processor.Encoding,
                    machineInfo.Processor.Type,
                    machineInfo.Processor.Subtype
                );
            }

            return new PLCrashReportMachineInfo(
                string.IsNullOrEmpty(machineInfo.Model) ? null : machineInfo.Model,
                processorInfo,
                machineInfo.ProcessorCount,
                machineInfo.LogicalProcessorCount
            );
        }

        private PLCrashReportApplicationInfo ExtractApplicationInfo(CrashReport.Types.ApplicationInfo applicationInfo)
        {
            return new PLCrashReportApplicationInfo(
                applicationInfo.Identifier,
                applicationInfo.Version,
                string.IsNullOrEmpty(applicationInfo.MarketingVersion) ? null : applicationInfo.MarketingVersion
            );
        }

        private PLCrashReportProcessInfo ExtractProcessInfo(CrashReport.Types.ProcessInfo processInfo)
        {
            var startTime = processInfo.StartTime != 0 ? DateTimeOffset.FromUnixTimeSeconds((long)processInfo.StartTime).DateTime : (DateTime?)null;

            return new PLCrashReportProcessInfo(
                string.IsNullOrEmpty(processInfo.ProcessName) ? null : processInfo.ProcessName,
                processInfo.ProcessId,
                string.IsNullOrEmpty(processInfo.ProcessPath) ? null : processInfo.ProcessPath,
                startTime,
                string.IsNullOrEmpty(processInfo.ParentProcessName) ? null : processInfo.ParentProcessName,
                processInfo.ParentProcessId,
                processInfo.Native
            );
        }

        private PLCrashReportSignalInfo ExtractSignalInfo(CrashReport.Types.Signal signal)
        {
            return new PLCrashReportSignalInfo(
                signal.Name,
                signal.Code,
                signal.Address
            );
        }

        private PLCrashReportMachExceptionInfo? ExtractMachExceptionInfo(CrashReport.Types.Signal.Types.MachException machException)
        {
            return new PLCrashReportMachExceptionInfo(
                machException.Type,
                machException.Codes.ToArray()
            );
        }

        private PLCrashReportThreadInfo[] ExtractThreadInfo(CrashReport crashReport)
        {
            var threads = new PLCrashReportThreadInfo[crashReport.Threads.Count];
            
            for (int i = 0; i < crashReport.Threads.Count; i++)
            {
                var thread = crashReport.Threads[i];
                
                // Extract stack frames
                var frames = new PLCrashReportStackFrameInfo[thread.Frames.Count];
                for (int j = 0; j < thread.Frames.Count; j++)
                {
                    var frame = thread.Frames[j];
                    PLCrashReportSymbolInfo? symbolInfo = null;
                    
                    if (frame.Symbol != null)
                    {
                        symbolInfo = new PLCrashReportSymbolInfo(
                            frame.Symbol.Name,
                            frame.Symbol.StartAddress,
                            frame.Symbol.EndAddress
                        );
                    }
                    
                    frames[j] = new PLCrashReportStackFrameInfo(frame.Pc, symbolInfo);
                }

                // Extract registers
                var registers = new PLCrashReportRegisterInfo[thread.Registers.Count];
                for (int k = 0; k < thread.Registers.Count; k++)
                {
                    var reg = thread.Registers[k];
                    registers[k] = new PLCrashReportRegisterInfo(reg.Name, reg.Value);
                }

                threads[i] = new PLCrashReportThreadInfo(
                    (int)thread.ThreadNumber,
                    frames,
                    thread.Crashed,
                    registers
                );
            }

            return threads;
        }

        private PLCrashReportBinaryImageInfo[] ExtractImageInfo(CrashReport crashReport)
        {
            var images = new PLCrashReportBinaryImageInfo[crashReport.BinaryImages.Count];
            
            for (int i = 0; i < crashReport.BinaryImages.Count; i++)
            {
                var image = crashReport.BinaryImages[i];
                
                PLCrashReportProcessorInfo? codeType = null;
                if (image.CodeType != null)
                {
                    codeType = new PLCrashReportProcessorInfo(
                        (PLCrashReportProcessorTypeEncoding)image.CodeType.Encoding,
                        image.CodeType.Type,
                        image.CodeType.Subtype
                    );
                }

                string? uuid = null;
                if (image.Uuid?.Length == 16)
                {
                    var guidBytes = image.Uuid.ToByteArray();
                    uuid = new Guid(guidBytes).ToString("D").ToUpper();
                }

                images[i] = new PLCrashReportBinaryImageInfo(
                    codeType,
                    image.BaseAddress,
                    image.Size,
                    image.Name,
                    uuid
                );
            }

            return images;
        }

        private PLCrashReportExceptionInfo ExtractExceptionInfo(CrashReport.Types.Exception exception)
        {
            PLCrashReportStackFrameInfo[]? frames = null;
            
            if (exception.Frames.Count > 0)
            {
                frames = new PLCrashReportStackFrameInfo[exception.Frames.Count];
                for (int i = 0; i < exception.Frames.Count; i++)
                {
                    var frame = exception.Frames[i];
                    PLCrashReportSymbolInfo? symbolInfo = null;
                    
                    if (frame.Symbol != null)
                    {
                        symbolInfo = new PLCrashReportSymbolInfo(
                            frame.Symbol.Name,
                            frame.Symbol.StartAddress,
                            frame.Symbol.EndAddress
                        );
                    }
                    
                    frames[i] = new PLCrashReportStackFrameInfo(frame.Pc, symbolInfo);
                }
            }

            return new PLCrashReportExceptionInfo(exception.Name, exception.Reason, frames);
        }
    }
} 