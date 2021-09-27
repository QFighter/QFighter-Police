using System;

namespace QFighterPolice
{
    public static class Logger
    {
        public static void LogMessage(string message)
            => Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }
}
