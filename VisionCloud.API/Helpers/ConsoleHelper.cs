using System;
using System.Collections.Generic;
using VisionCloud.API.Models;

namespace VisionCloud.API
{
    public class ConsoleHelper
    {
        public static void WritePrompt()
        {
            Write(">_", ConsoleColor.Green);
        }

        public static void WriteInstruction(string s)
        {
            WriteLine(s, ConsoleColor.Cyan);
        }

        public static void WriteOptions(List<string> descs)
        {
            foreach (var desc in descs)
            {
                WriteLine(desc, ConsoleColor.Blue);
            }
        }

        public static void WriteStep(string s)
        {
            WriteLine(s, ConsoleColor.White);
        }

        public static void WriteStepIo(string s)
        {
            WriteLine(s, ConsoleColor.DarkYellow);
        }

        public static void WriteStepResult(string s)
        {
            WriteLine(s, ConsoleColor.White);
        }

        public static void WriteNotice(string s)
        {
            WriteLine(s, ConsoleColor.Magenta);
        }

        public static void WriteSuccess(string s)
        {
            WriteLine($"Success: {s}", ConsoleColor.Green);
        }

        public static void WriteFailure(string s)
        {
            WriteLine(s, ConsoleColor.Red);
        }

        public static void WriteWarning(string s)
        {
            WriteLine(s, ConsoleColor.Yellow);
        }

        public static void WriteError(string s)
        {
            WriteLine($"Error: {s}", ConsoleColor.Red);
        }

        public static void WriteException(Exception ex)
        {
            WriteException("An exception was thrown", ex);
        }

        public static void WriteException(string message, Exception ex)
        {
            WriteLine($"Application error: {message}", ConsoleColor.Red);
            WriteLine($"Exception details: {ex}", ConsoleColor.Red);
        }

        public static void WriteObjectInfo(object element)
        {
            WriteLine(ConsoleObjectDumper.Dump(element), ConsoleColor.DarkGreen);
        }

        public static void WriteApps(List<RunningApps> apps)
        {
            foreach (var app in apps)
            {
                WriteLine(ConsoleObjectDumper.Dump(app), ConsoleColor.Magenta);
            }
        }

        private static void WriteLine(string s, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            ResetColor();
        }

        private static void Write(string s, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(s);
            ResetColor();
        }

        private static void ResetColor()
        {
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
