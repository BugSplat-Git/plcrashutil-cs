using Xunit;
using PLCrashUtilLib.Models;

namespace PLCrashUtilLib.Tests
{
    public class PLCrashReportConstantsTests
    {
        [Fact]
        public void PLCrashReportConstants_HasCorrectMagic()
        {
            // Assert
            Assert.Equal("plcrash", PLCrashReportConstants.PLCRASH_REPORT_FILE_MAGIC);
        }

        [Fact]
        public void PLCrashReportConstants_HasCorrectVersion()
        {
            // Assert
            Assert.Equal(1, PLCrashReportConstants.PLCRASH_REPORT_FILE_VERSION);
        }
    }

    public class PLCrashReportTextFormatTests
    {
        [Fact]
        public void PLCrashReportTextFormat_iOSValueIsValid()
        {
            // Assert - Just ensure the enum value exists and has expected value
            Assert.Equal(0, (int)PLCrashReportTextFormat.PLCrashReportTextFormatiOS);
        }
    }

    public class PLCrashReportInfoTests
    {
        [Fact]
        public void PLCrashReportSystemInfo_Constructor_SetsAllProperties()
        {
            // Arrange
            var os = PLCrashReportOperatingSystem.PLCrashReportOperatingSystemiPhoneOS;
            var version = "14.0";
            var build = "18A373";
            var arch = PLCrashReportArchitecture.PLCrashReportArchitectureARMv7;
            var processorInfo = new PLCrashReportProcessorInfo(
                PLCrashReportProcessorTypeEncoding.PLCrashReportProcessorTypeEncodingMach,
                12,
                1);
            var timestamp = System.DateTime.Now;

            // Act
            var systemInfo = new PLCrashReportSystemInfo(os, version, build, arch, processorInfo, timestamp);

            // Assert
            Assert.Equal(os, systemInfo.OperatingSystem);
            Assert.Equal(version, systemInfo.OperatingSystemVersion);
            Assert.Equal(build, systemInfo.OperatingSystemBuild);
            Assert.Equal(arch, systemInfo.Architecture);
            Assert.Equal(processorInfo, systemInfo.ProcessorInfo);
            Assert.Equal(timestamp, systemInfo.Timestamp);
        }

        [Fact]
        public void PLCrashReportBinaryImageInfo_HasImageUUID_ReturnsTrueWhenUUIDExists()
        {
            // Arrange
            var imageInfo = new PLCrashReportBinaryImageInfo(null, 0x1000, 0x2000, "test.dylib", "test-uuid");

            // Act & Assert
            Assert.True(imageInfo.HasImageUUID);
        }

        [Fact]
        public void PLCrashReportBinaryImageInfo_HasImageUUID_ReturnsFalseWhenUUIDIsNull()
        {
            // Arrange
            var imageInfo = new PLCrashReportBinaryImageInfo(null, 0x1000, 0x2000, "test.dylib", null);

            // Act & Assert
            Assert.False(imageInfo.HasImageUUID);
        }

        [Fact]
        public void PLCrashReportBinaryImageInfo_HasImageUUID_ReturnsFalseWhenUUIDIsEmpty()
        {
            // Arrange
            var imageInfo = new PLCrashReportBinaryImageInfo(null, 0x1000, 0x2000, "test.dylib", "");

            // Act & Assert
            Assert.False(imageInfo.HasImageUUID);
        }
    }
} 