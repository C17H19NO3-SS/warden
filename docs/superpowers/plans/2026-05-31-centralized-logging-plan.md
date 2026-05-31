# Centralized Debug Logger Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a centralized logging system (`LogHelper`) that routes all logs through `Server.PrintToConsole`, features a `DebugMode` (default true), and refactors existing disparate logging calls to use this new structure.

**Architecture:** A new static class `LogHelper` will provide `LogInfo`, `LogDebug`, and `LogError` methods. The plugin's main configuration will hold the `DebugMode` state. Existing services using `ILogger` or direct `Server.PrintToConsole` for errors will be updated to call `LogHelper` instead, ensuring uniform, parseable output with stack traces for exceptions.

**Tech Stack:** C#, .NET 8.0, CounterStrikeSharp API

---

### Task 1: Add DebugMode to Configuration

**Files:**
- Modify: `Config/PluginConfig.cs`

- [ ] **Step 1: Add DebugMode property**

Edit `Config/PluginConfig.cs` to add the `DebugMode` property, defaulting to `true`.

```csharp
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace JailBreak.Config
{
    public class PluginConfig : BasePluginConfig
    {
        // ... (existing properties like ChatPrefix)
        
        [JsonPropertyName("DebugMode")]
        public bool DebugMode { get; set; } = true;

        [JsonPropertyName("ChatPrefix")]
        public string ChatPrefix { get; set; } = "[{Red}JailBreak{Default}]";

        [JsonPropertyName("Database")]
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();
        
        [JsonPropertyName("Warden")]
        public WardenConfig Warden { get; set; } = new WardenConfig();
        
        [JsonPropertyName("Rebel")]
        public RebelConfig Rebel { get; set; } = new RebelConfig();
        
        [JsonPropertyName("Marker")]
        public MarkerConfig Marker { get; set; } = new MarkerConfig();
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Config/PluginConfig.cs
git commit -m "feat: add DebugMode to PluginConfig with default true"
```

---

### Task 2: Create LogHelper Class

**Files:**
- Create: `Helpers/LogHelper.cs`

- [ ] **Step 1: Create the LogHelper class implementation**

Create `Helpers/LogHelper.cs`. Note: We will inject the configuration dependency or provide a static way to access `DebugMode`. Since we cannot easily inject into static without setup, we will add an `Initialize` method to pass the config reference.

```csharp
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
```

- [ ] **Step 2: Build the project to verify compilation**

Run: `dotnet build -c Release`
Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add Helpers/LogHelper.cs
git commit -m "feat: create centralized LogHelper"
```

---

### Task 3: Initialize LogHelper and Refactor JailBreakPlugin

**Files:**
- Modify: `JailBreakPlugin.cs`

- [ ] **Step 1: Initialize LogHelper and update logs**

Edit `JailBreakPlugin.cs`. 
1. Remove `using Microsoft.Extensions.Logging;` if it's only used for logging.
2. In `OnLoad`, call `LogHelper.Initialize(Config);` right after config is loaded.
3. Replace existing `Logger.LogError` and redundant `Server.PrintToConsole` calls for errors with `LogHelper.LogError`.
4. Add `LogHelper.LogInfo("Eklenti başarıyla yüklendi.");` at the end of `OnLoad`.

```csharp
// Top of file
// Remove: using Microsoft.Extensions.Logging;
using JailBreak.Helpers;

// ... inside JailBreakPlugin
        public override void Load(bool hotReload)
        {
            try
            {
                LoadConfigs();
                LogHelper.Initialize(Config); // Add this
                
                // ... (rest of load logic)

                LogHelper.LogInfo("JailBreak eklentisi başarıyla yüklendi.");
            }
            catch (Exception ex)
            {
                // Remove Logger.LogError
                // Remove Server.PrintToConsole
                LogHelper.LogError("Error loading configs during OnLoad.", ex);
            }
        }
        
        // ... inside OnConfigParsed or where reload happens
        [ConsoleCommand("css_jailbreak_reload", "Reloads the plugin configuration")]
        [RequiresPermissions("@css/root")]
        public void OnReloadCommand(CCSPlayerController? player, CommandInfo command)
        {
            try
            {
                LoadConfigs();
                LogHelper.Initialize(Config); // Update ref just in case
                
                // ...
            }
            catch (Exception ex)
            {
                 // Remove Logger.LogError
                 // Remove Server.PrintToConsole
                 LogHelper.LogError("Error reloading config.", ex);
                 // ... keep PrintToChat
            }
        }
```

- [ ] **Step 2: Build the project to verify compilation**

Run: `dotnet build -c Release`
Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit**

```bash
git add JailBreakPlugin.cs
git commit -m "refactor: initialize LogHelper and replace logs in main plugin file"
```

---

### Task 4: Refactor Services Logging

**Files:**
- Modify: `Services/DispatcherService.cs`
- Modify: `Services/RebelService.cs`
- Modify: `Services/WardenService.cs`

- [ ] **Step 1: Update DispatcherService**

Edit `Services/DispatcherService.cs`. Replace `_logger.LogError(...)` with `LogHelper.LogError(...)` and add debug logs.

```csharp
using JailBreak.Helpers;
// ...
        public void ExecuteCommand(CCSPlayerController? player, string commandName, string args)
        {
            LogHelper.LogDebug($"DispatcherService: Attempting to execute command '{commandName}' with args '{args}' for player '{player?.PlayerName ?? "Console"}'");
            try
            {
                // ... (existing logic)
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error executing command '{commandName}' for player '{player?.PlayerName ?? "N/A"}'", ex);
                player?.PrintToChat($" {ChatColors.Red}Hata: Bu komutu çalıştırırken bir sorun oluştu. Konsolunuzu kontrol edin.");
            }
        }
```

- [ ] **Step 2: Update RebelService**

Edit `Services/RebelService.cs`. Replace `_plugin.Logger.LogError` and `Server.PrintToConsole` with `LogHelper.LogError` and add debug logs.

```csharp
using JailBreak.Helpers;
// ...
        public void LoadData()
        {
            LogHelper.LogDebug("RebelService: Loading rebel data...");
            try
            {
                // ...
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Rebel data load error.", ex);
            }
        }

        public void SaveData()
        {
            LogHelper.LogDebug("RebelService: Saving rebel data...");
            try
            {
                // ...
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Rebel data save error.", ex);
            }
        }
```

- [ ] **Step 3: Update WardenService**

Edit `Services/WardenService.cs`. Replace `_plugin.Logger.LogError` and `Server.PrintToConsole` with `LogHelper.LogError` and add debug logs.

```csharp
using JailBreak.Helpers;
// ...
        public void LoadStats()
        {
            LogHelper.LogDebug("WardenService: Loading warden stats...");
            try
            {
                // ...
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Error loading warden stats.", ex);
            }
        }

        public void SaveStats()
        {
             LogHelper.LogDebug("WardenService: Saving warden stats...");
            try
            {
                // ...
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Error saving warden stats.", ex);
            }
        }
        
        public void LoadAdminStats()
        {
             LogHelper.LogDebug("WardenService: Loading warden admin stats...");
            try
            {
                // ...
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Error loading warden admin stats.", ex);
            }
        }

        public void SaveAdminStats()
        {
            LogHelper.LogDebug("WardenService: Saving warden admin stats...");
            try
            {
                // ...
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Error saving warden admin stats.", ex);
            }
        }
```

- [ ] **Step 4: Build the project to verify compilation**

Run: `dotnet build -c Release`
Expected: Build succeeds with 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Services/DispatcherService.cs Services/RebelService.cs Services/WardenService.cs
git commit -m "refactor: update services to use LogHelper and add debug traces"
```
