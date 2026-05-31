using System;
using CounterStrikeSharp.API;
using JailBreak.Config;

namespace JailBreak.Helpers
{
    public static class LogHelper
    {
        private static PluginConfig? _config;
        private const string LogPrefix = "[JailBreak]";

        public static void Initialize(PluginConfig config)
        {
            _config = config;
        }

        public static void LogInfo(string message)
        {
            Server.PrintToConsole($"{LogPrefix} [INFO] [{DateTime.Now:HH:mm:ss}] {message}");
        }

        public static void LogError(string message, Exception? ex = null)
        {
            string logMessage = $"{LogPrefix} [ERROR] [{DateTime.Now:HH:mm:ss}] {message}";
            if (ex != null)
            {
                logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            Server.PrintToConsole(logMessage);
        }

        public static void LogDebug(string message)
        {
            if (_config != null && _config.DebugMode)
            {
                Server.PrintToConsole($"{LogPrefix} [DEBUG] [{DateTime.Now:HH:mm:ss}] {message}");
            }
        }
    }
}