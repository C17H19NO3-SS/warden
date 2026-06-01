using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using JailBreak.Config;

namespace JailBreak.Helpers
{
    public static class LogHelper
    {
        private static PluginConfig? _config;
        private static string _logDirectory = string.Empty;
        private const string LogPrefix = "[JailBreak]";
        private static readonly SemaphoreSlim _fileSemaphore = new SemaphoreSlim(1, 1);
        private static string? _currentFilePath;
        private static int _currentLineCount;
        private static string? _currentDate;
        private const int MaxLines = 500;

        public static void Initialize(PluginConfig config, string pluginDirectory)
        {
            _config = config;
            _logDirectory = Path.Combine(pluginDirectory, "logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        private static async Task WriteToFileAsync(string level, string message)
        {
            string logLine = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            await _fileSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_currentFilePath == null || _currentDate != today)
                {
                    _currentDate = today;
                    int fileIndex = 0;
                    while (true)
                    {
                        string fileName = $"log_{_currentDate}_{fileIndex}.txt";
                        string tempPath = Path.Combine(_logDirectory, fileName);

                        if (!File.Exists(tempPath))
                        {
                            _currentFilePath = tempPath;
                            _currentLineCount = 0;
                            break;
                        }

                        var lines = await File.ReadAllLinesAsync(tempPath).ConfigureAwait(false);
                        if (lines.Length < MaxLines)
                        {
                            _currentFilePath = tempPath;
                            _currentLineCount = lines.Length;
                            break;
                        }
                        fileIndex++;
                    }
                }

                if (_currentLineCount >= MaxLines)
                {
                    int fileIndex = 0;
                    string fileName;
                    string tempPath;
                    do
                    {
                        fileIndex++;
                        fileName = $"log_{_currentDate}_{fileIndex}.txt";
                        tempPath = Path.Combine(_logDirectory, fileName);
                    } while (File.Exists(tempPath));

                    _currentFilePath = tempPath;
                    _currentLineCount = 0;
                }

                await File.AppendAllLinesAsync(_currentFilePath!, new[] { logLine }).ConfigureAwait(false);
                _currentLineCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{LogPrefix} [ERROR] [LogHelper] Failed to write to file: {ex.Message}");
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }

        public static void LogInfo(string message)
        {
            string msg = $"{LogPrefix} [INFO] [{DateTime.Now:HH:mm:ss}] {message}";
            Server.PrintToConsole(msg);
            _ = Task.Run(() => WriteToFileAsync("INFO", message));
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
            _ = Task.Run(() => WriteToFileAsync("ERROR", fileMessage));
        }

        public static void LogDebug(string message)
        {
            if (_config == null || _config.LogLevel < 2) return;
            Server.PrintToConsole($"{LogPrefix} [DEBUG] [{DateTime.Now:HH:mm:ss}] {message}");
            _ = Task.Run(() => WriteToFileAsync("DEBUG", message));
        }

        public static void LogTrace(string message)
        {
            if (_config == null || _config.LogLevel < 3) return;
            Server.PrintToConsole($"{LogPrefix} [TRACE] [{DateTime.Now:HH:mm:ss}] {message}");
            _ = Task.Run(() => WriteToFileAsync("TRACE", message));
        }
    }
}
