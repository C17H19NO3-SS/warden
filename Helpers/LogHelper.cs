using System;
using System.IO;
using CounterStrikeSharp.API;
using JailBreak.Config;

namespace JailBreak.Helpers
{
    public static class LogHelper
    {
        private static PluginConfig? _config;
        private static string _logDirectory = string.Empty;
        private const string LogPrefix = "[JailBreak]";

        public static void Initialize(PluginConfig config, string pluginDirectory)
        {
            _config = config;
            _logDirectory = Path.Combine(pluginDirectory, "logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        private static void WriteToFile(string level, string message)
        {
            int maxLines = 500;
            string logLine = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";

            string? filePath = null;
            int fileIndex = 0;
            
            while (filePath == null)
            {
                string fileName = $"log_{DateTime.Now:yyyy-MM-dd}_{fileIndex}.txt";
                string tempPath = Path.Combine(_logDirectory, fileName);
                
                if (!File.Exists(tempPath) || File.ReadAllLines(tempPath).Length < maxLines)
                {
                    filePath = tempPath;
                }
                else
                {
                    fileIndex++;
                }
            }

            try
            {
                File.AppendAllLines(filePath, new[] { logLine });
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"{LogPrefix} [ERROR] [LogHelper] Failed to write to file: {ex.Message}");
            }
        }

        public static void LogInfo(string message)
        {
            string msg = $"{LogPrefix} [INFO] [{DateTime.Now:HH:mm:ss}] {message}";
            Server.PrintToConsole(msg);
            WriteToFile("INFO", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            if (_config == null || _config.LogLevel < 1) return;
            string logMessage = $"{LogPrefix} [ERROR] [{DateTime.Now:HH:mm:ss}] {message}";
            string fileMessage = message;
            if (ex != null)
            {
                string details = "\nException: " + ex.Message + "\nStackTrace: " + ex.StackTrace;
                logMessage += details;
                fileMessage += details;
            }
            Server.PrintToConsole(logMessage);
            WriteToFile("ERROR", fileMessage);
        }

        public static void LogDebug(string message)
        {
            if (_config == null || _config.LogLevel < 2) return;
            Server.PrintToConsole($"{LogPrefix} [DEBUG] [{DateTime.Now:HH:mm:ss}] {message}");
            WriteToFile("DEBUG", message);
        }

        public static void LogTrace(string message)
        {
            if (_config == null || _config.LogLevel < 3) return;
            Server.PrintToConsole($"{LogPrefix} [TRACE] [{DateTime.Now:HH:mm:ss}] {message}");
            WriteToFile("TRACE", message);
        }
    }
}
