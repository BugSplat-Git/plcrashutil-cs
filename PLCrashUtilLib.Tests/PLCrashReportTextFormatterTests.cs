using System;
using System.IO;
using Xunit;
using PLCrashUtilLib.Models;

namespace PLCrashUtilLib.Tests
{
    public class PLCrashReportTextFormatterTests
    {
        [Fact]
        public void StringValueForCrashReport_ValidReport_ReturnsFormattedText()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.NotNull(formattedText);
            Assert.NotEmpty(formattedText);
            
            // Check for expected sections in the formatted output
            Assert.Contains("Incident Identifier:", formattedText);
            Assert.Contains("Hardware Model:", formattedText);
            Assert.Contains("Process:", formattedText);
            Assert.Contains("Path:", formattedText);
            Assert.Contains("Identifier:", formattedText);
            Assert.Contains("Version:", formattedText);
            Assert.Contains("Code Type:", formattedText);
            Assert.Contains("Date/Time:", formattedText);
            Assert.Contains("OS Version:", formattedText);
            Assert.Contains("Exception Type:", formattedText);
            Assert.Contains("Exception Codes:", formattedText);
            Assert.Contains("Thread", formattedText);
            Assert.Contains("Binary Images:", formattedText);
        }

        [Fact]
        public void StringValueForCrashReport_ContainsExpectedApplicationInfo()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.Contains("com.yourcompany.DemoCrash", formattedText);
            Assert.Contains("Version:         1.0", formattedText);
        }

        [Fact]
        public void StringValueForCrashReport_ContainsExpectedSignalInfo()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.Contains("Exception Type:  SIGBUS", formattedText);
            Assert.Contains("Exception Codes:", formattedText);
            Assert.Contains("BUS_ADRERR", formattedText);
        }

        [Fact]
        public void StringValueForCrashReport_ContainsCrashedThreadInfo()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.Contains("Crashed Thread:", formattedText);
            Assert.Contains("Thread 0 Crashed:", formattedText);
        }

        [Fact]
        public void StringValueForCrashReport_ContainsRegisterInfo()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.Contains("Thread State:", formattedText);
            // X86 registers should be present in the test data
            Assert.Contains("eax:", formattedText);
            Assert.Contains("ebx:", formattedText);
            Assert.Contains("ecx:", formattedText);
            Assert.Contains("edx:", formattedText);
        }

        [Fact]
        public void StringValueForCrashReport_ContainsBinaryImageList()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.Contains("Binary Images:", formattedText);
            Assert.Contains("DemoCrash", formattedText); // Should contain the main app
            Assert.Contains("CoreFoundation", formattedText); // Should contain system libraries
        }

        [Fact]
        public void StringValueForCrashReport_ContainsSystemInfo()
        {
            // Arrange
            var testDataPath = Path.Combine("..", "..", "..", "..", "fuzz_report.plcrash");
            var data = File.ReadAllBytes(testDataPath);
            var report = new PLCrashReport(data);

            // Act
            var formattedText = PLCrashReportTextFormatter.StringValueForCrashReport(report, PLCrashReportTextFormat.PLCrashReportTextFormatiOS);

            // Assert
            Assert.Contains("OS Version:", formattedText);
            Assert.Contains("Mac OS X", formattedText);
            Assert.Contains("10.5.6", formattedText);
            Assert.Contains("Report Version:  104", formattedText);
        }
    }
} 