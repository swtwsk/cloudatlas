using System;

namespace Shared.Logger
{
    // TODO: make proper logger class
    public static class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine("LOG: " + message);
        }
        public static void LogError(string message)
        {
            Console.WriteLine("ERROR: " + message);
            Console.WriteLine(Environment.StackTrace);
        }
    }
}
