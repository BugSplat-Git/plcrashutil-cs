using System;
using System.IO;
using Xunit;
using PLCrashUtilLib.Models;

namespace PLCrashUtilLib.Tests
{
    public class PLCrashReportTests
    {
        [Fact]
        public void PLCrashReport_ValidData_ParsesSuccessfully()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);

            // Act
            var report = new PLCrashReport(data);

            // Assert
            Assert.NotNull(report);
            Assert.NotNull(report.SystemInfo);
            Assert.NotNull(report.ApplicationInfo);
            Assert.NotNull(report.SignalInfo);
            Assert.NotNull(report.Threads);
            Assert.NotNull(report.Images);
            Assert.True(report.Threads.Length > 0);
            Assert.True(report.Images.Length > 0);
        }

        [Fact]
        public void PLCrashReport_SystemInfo_HasValidData()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);

            // Act
            var report = new PLCrashReport(data);

            // Assert
            Assert.NotNull(report.SystemInfo);
            Assert.NotNull(report.SystemInfo.OperatingSystemVersion);
            Assert.True(report.SystemInfo.Timestamp.HasValue);
        }

        [Fact]
        public void PLCrashReport_ApplicationInfo_HasValidData()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);

            // Act
            var report = new PLCrashReport(data);

            // Assert
            Assert.NotNull(report.ApplicationInfo);
            Assert.NotNull(report.ApplicationInfo.ApplicationIdentifier);
            Assert.NotNull(report.ApplicationInfo.ApplicationVersion);
            Assert.Equal("com.yourcompany.DemoCrash", report.ApplicationInfo.ApplicationIdentifier);
        }

        [Fact]
        public void PLCrashReport_SignalInfo_HasValidData()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);

            // Act
            var report = new PLCrashReport(data);

            // Assert
            Assert.NotNull(report.SignalInfo);
            Assert.NotNull(report.SignalInfo.Name);
            Assert.NotNull(report.SignalInfo.Code);
            Assert.Equal("SIGBUS", report.SignalInfo.Name);
        }

        [Fact]
        public void PLCrashReport_Threads_ContainsCrashedThread()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);

            // Act
            var report = new PLCrashReport(data);

            // Assert
            Assert.NotNull(report.Threads);
            Assert.True(report.Threads.Length > 0);
            
            var crashedThread = Array.Find(report.Threads, t => t.Crashed);
            Assert.NotNull(crashedThread);
            Assert.True(crashedThread.StackFrames.Length > 0);
        }

        [Fact]
        public void PLCrashReport_Images_ContainsBinaryImages()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);

            // Act
            var report = new PLCrashReport(data);

            // Assert
            Assert.NotNull(report.Images);
            Assert.True(report.Images.Length > 0);
            
            foreach (var image in report.Images)
            {
                Assert.NotNull(image.ImageName);
                Assert.True(image.ImageBaseAddress > 0);
                Assert.True(image.ImageSize > 0);
            }
        }

        [Fact]
        public void PLCrashReport_ImageForAddress_FindsCorrectImage()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act - Use an address that should be within the first image
            var firstImage = report.Images[0];
            var testAddress = firstImage.ImageBaseAddress + 100; // Address within the first image
            var foundImage = report.ImageForAddress(testAddress);

            // Assert
            Assert.NotNull(foundImage);
            Assert.Equal(firstImage.ImageName, foundImage.ImageName);
            Assert.Equal(firstImage.ImageBaseAddress, foundImage.ImageBaseAddress);
        }

        [Fact]
        public void PLCrashReport_ImageForAddress_ReturnsNullForInvalidAddress()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act - Use an address that's clearly outside any image range
            var foundImage = report.ImageForAddress(0xFFFFFFFFFFFFFFFF);

            // Assert
            Assert.Null(foundImage);
        }

        [Fact]
        public void PLCrashReport_InvalidMagic_ThrowsException()
        {
            // Arrange
            var invalidData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() => new PLCrashReport(invalidData));
            Assert.Contains("invalid crash log header", exception.Message);
        }

        [Fact]
        public void PLCrashReport_TruncatedData_ThrowsException()
        {
            // Arrange
            var truncatedData = new byte[] { 0x01, 0x02, 0x03 }; // Too short

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() => new PLCrashReport(truncatedData));
            Assert.Contains("truncated crash log", exception.Message);
        }

        [Fact]
        public void PLCrashReport_InvalidVersion_ThrowsException()
        {
            // Arrange - Valid magic but invalid version
            var invalidVersionData = new byte[] { 
                0x70, 0x6C, 0x63, 0x72, 0x61, 0x73, 0x68, // "plcrash"
                0xFF // Invalid version
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidDataException>(() => new PLCrashReport(invalidVersionData));
            Assert.Contains("unsupported crash report version", exception.Message);
        }
    }
} 