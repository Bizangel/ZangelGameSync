namespace ZangelGameSyncClient.ConsoleLogging
{
    /**
     * I intend to keep this Client with as minimal dependencies as possible, so I'm creating an insanely barebones logging class.
     * Might want to upgrade to Serilog/NLog if the client grows bigger.
     * **/
    internal static class Logger
    {
        private static string GetLogEntry(string msg, string level)
        {
            return $"[{DateTime.Now}] [{level}]: {msg}";
        }

        internal static void LogInfo(string msg, bool print = false)
        {
            string entry = GetLogEntry(msg, "INFO");
            if (print)
                ConsolePrinter.Info(entry);
            LogToFile(entry);
        }

        internal static void LogWarn(string msg)
        {
            string entry = GetLogEntry(msg, "WARN");
            ConsolePrinter.Warn(entry);
            LogToFile(entry);
        }

        internal static void LogError(string msg)
        {
            string entry = GetLogEntry(msg, "ERROR");
            ConsolePrinter.Error(entry);
            LogToFile(entry);
        }

        private static void LogToFile(string logLine)
        {
            string logFilePath = "zangelgamesync.log";
            try
            {
                File.AppendAllText(logFilePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}
