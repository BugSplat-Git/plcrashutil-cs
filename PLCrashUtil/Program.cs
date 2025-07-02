using System;
using System.IO;
using System.Linq;
using PLCrashUtilLib.Models;
using PLCrashUtilLib;

namespace PLCrashUtil;

class Program
{
    /// <summary>
    /// Print command line usage information.
    /// </summary>
    private static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: plcrashutil <command> <options>");
        Console.Error.WriteLine("Commands:");
        Console.Error.WriteLine("  convert --format=<format> <file>");
        Console.Error.WriteLine("      Convert a plcrash file to the given format.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("      Supported formats:");
        Console.Error.WriteLine("        ios - Standard Apple iOS-compatible text crash log");
        Console.Error.WriteLine("        iphone - Synonym for 'iOS'.");
    }

    /// <summary>
    /// Run a conversion command.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code</returns>
    private static int ConvertCommand(string[] args)
    {
        string format = "iphone";
        string? inputFile = null;

        // Parse arguments manually to match the original getopt behavior
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--format="))
            {
                format = args[i].Substring("--format=".Length);
            }
            else if (args[i] == "--format" || args[i] == "-f")
            {
                if (i + 1 < args.Length)
                {
                    format = args[++i];
                }
                else
                {
                    Console.Error.WriteLine("Option --format requires an argument");
                    PrintUsage();
                    return 1;
                }
            }
            else if (!args[i].StartsWith("-"))
            {
                // This is the input file
                inputFile = args[i];
            }
        }

        // Ensure there's an input file specified
        if (string.IsNullOrEmpty(inputFile))
        {
            Console.Error.WriteLine("No input file supplied");
            PrintUsage();
            return 1;
        }

        // Verify that the format is supported
        PLCrashReportTextFormat textFormat;
        if (string.Equals(format, "iphone", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(format, "ios", StringComparison.OrdinalIgnoreCase))
        {
            textFormat = PLCrashReportTextFormat.PLCrashReportTextFormatiOS;
        }
        else
        {
            Console.Error.WriteLine("Unsupported format requested");
            PrintUsage();
            return 1;
        }

        try
        {
            // Try reading the file
            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine($"Could not read input file: File '{inputFile}' not found");
                return 1;
            }

            byte[] data = File.ReadAllBytes(inputFile);
            
            // Decode the crash report
            var crashLog = new PLCrashReport(data);
            
            // Format the report
            string report = PLCrashReportTextFormatter.StringValueForCrashReport(crashLog, textFormat);
            Console.Write(report);
            
            return 0;
        }
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine($"Could not read input file: File '{inputFile}' not found");
            return 1;
        }
        catch (UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Could not read input file: Access denied to '{inputFile}'");
            return 1;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Could not read input file: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not decode crash log: {ex.Message}");
            return 1;
        }
    }

    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            PrintUsage();
            return 1;
        }

        // Convert command
        if (string.Equals(args[0], "convert", StringComparison.OrdinalIgnoreCase))
        {
            return ConvertCommand(args.Skip(1).ToArray());
        }
        else
        {
            PrintUsage();
            return 1;
        }
    }
} 