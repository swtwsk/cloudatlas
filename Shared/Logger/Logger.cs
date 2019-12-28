using System;
using System.Linq;

namespace Shared.Logger
{
    // TODO: make proper logger class
    public static class Logger
    {
        public static LoggerLevel LoggerLevel = LoggerLevel.Error;
        public static LoggerVerbosity LoggerVerbosity = LoggerVerbosity.WithoutFilePath;

        private static void PrintLog(string logName, string memberName, string filePath, string message)
        {
            var printablePath = LoggerVerbosity == LoggerVerbosity.WithoutFilePath
                ? ""
                : ", " + (LoggerVerbosity == LoggerVerbosity.WithFileName ? filePath.Split("\\").Last() : filePath);
            Console.WriteLine($"{logName}, {memberName}{printablePath}: {message}");
        }
        
        public static void Log(string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.All)
                return;
            PrintLog("LOG", memberName, filePath, message);
        }

        public static void LogWarning(string message,
            [System.Runtime.CompilerServices.CallerMemberName]
            string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]
            string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.Warning)
                return;
            PrintLog("WARNING", memberName, filePath, message);
            Console.WriteLine(Environment.StackTrace);
        }
        
        public static void LogError(string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.Error)
                return;
            PrintLog("ERROR", memberName, filePath, message);
            Console.WriteLine(Environment.StackTrace);
        }
        public static void LogException(Exception e,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.Exception)
                return;
            PrintLog("EXCEPTION", memberName, filePath, e.ToString());
        }
    }

    public enum LoggerLevel
    {
        Nothing,
        Exception,
        Error,
        Warning,
        All,
    }

    public enum LoggerVerbosity
    {
        WithFilePath,
        WithFileName,
        WithoutFilePath,
    }
}
