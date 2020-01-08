using System;
using System.Linq;

namespace Shared.Logger
{
    // TODO: make proper logger class
    public static class Logger
    {
        public static LoggerLevel LoggerLevel = LoggerLevel.Error;
        public static LoggerVerbosity LoggerVerbosity = LoggerVerbosity.WithoutFilePath;

        private static void PrintLog(string logName, string memberName, string filePath, string message, bool printStackTrace)
        {
            var printablePath = LoggerVerbosity == LoggerVerbosity.WithoutFilePath
                ? ""
                : ", " + (LoggerVerbosity == LoggerVerbosity.WithFileName
                      ? filePath.Split(new char[] {'\\', '/'}).Last()
                      : filePath);
            Console.WriteLine($"{logName}, {DateTime.Now.ToLongTimeString()}, {memberName}{printablePath}: {message}");
            if (printStackTrace)
                Console.WriteLine(Environment.StackTrace);
        }
        
        public static void Log(string message, bool printStackTrace = false,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.All)
                return;
            PrintLog("LOG", memberName, filePath, message, printStackTrace);
        }

        public static void LogWarning(string message, bool printStackTrace = false,
            [System.Runtime.CompilerServices.CallerMemberName]
            string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]
            string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.Warning)
                return;
            PrintLog("WARNING", memberName, filePath, message, printStackTrace);
        }
        
        public static void LogError(string message, bool printStackTrace = true,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.Error)
                return;
            PrintLog("ERROR", memberName, filePath, message, printStackTrace);
        }
        public static void LogException(Exception e,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            if (LoggerLevel < LoggerLevel.Exception)
                return;
            PrintLog("EXCEPTION", memberName, filePath, e.ToString(), false);
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
