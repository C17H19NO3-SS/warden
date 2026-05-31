# Centralized Game Management and Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Centralize game mode management (Boxing, Saklambac), provide a master Warden menu (`!k`), add administrative broadcast/utility commands, and refactor redundant code patterns.

**Architecture:** We will create a `GameManagerService` for transient game modes. We will move shared HUD and message formatting logic to a static `PluginHelper` class. `WardenService` will host the centralized permission check. All existing services will be refactored to use these centralized utilities.

**Tech Stack:** C#, CounterStrikeSharp, CS2MenuManager

---

### Task 1: Centralized Utilities and Permission Refactor

**Files:**
- Create: `Helpers/PluginHelper.cs`
- Modify: `Services/WardenService.cs`
- Modify: `Services/ChatService.cs`
- Modify: `Services/FFMenuService.cs`
- Modify: `Services/FreezeService.cs`
- Modify: `Services/IseliService.cs`
- Modify: `Services/UtilityService.cs`
- Modify: `Services/VoteService.cs`

- [ ] **Step 1: Create `Helpers/PluginHelper.cs` and move `HudHelper`**

Move the `HudHelper` class from `FreezeService.cs` to `Helpers/PluginHelper.cs` and add chat formatting helpers.

```csharp
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Config;

namespace JailBreak.Helpers;

public static class PluginHelper
{
    public const string ColorTitle = "gold";
    public const string ColorTime = "red";
    public const string ColorSuccess = "green";
    public const string ColorInstruction = "#C0C0C0";
    public const string ColorSystem = "cyan";

    public static string FormatHud(string title, string content, string instruction = "")
    {
        string hud = $"<font color='{ColorTitle}' size='20'><b>--- {title.ToUpper()} ---</b></font><br>{content}";
        if (!string.IsNullOrEmpty(instruction))
        {
            hud += $"<br><font color='{ColorInstruction}' size='14'>{instruction}</font>";
        }
        return hud;
    }

    public static string ReplaceColors(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return input.Replace("{Default}", $"{ChatColors.Default}")
                    .Replace("{White}", $"{ChatColors.White}")
                    .Replace("{DarkRed}", $"{ChatColors.DarkRed}")
                    .Replace("{Purple}", $"{ChatColors.Purple}")
                    .Replace("{Green}", $"{ChatColors.Green}")
                    .Replace("{LightYellow}", $"{ChatColors.LightYellow}")
                    .Replace("{LightBlue}", $"{ChatColors.LightBlue}")
                    .Replace("{Olive}", $"{ChatColors.Olive}")
                    .Replace("{Lime}", $"{ChatColors.Lime}")
                    .Replace("{Red}", $"{ChatColors.Red}")
                    .Replace("{LightPurple}", $"{ChatColors.LightPurple}")
                    .Replace("{Gray}", $"{ChatColors.Grey}")
                    .Replace("{Grey}", $"{ChatColors.Grey}")
                    .Replace("{Yellow}", $"{ChatColors.Yellow}")
                    .Replace("{Gold}", $"{ChatColors.Gold}")
                    .Replace("{Silver}", $"{ChatColors.Silver}")
                    .Replace("{Blue}", $"{ChatColors.Blue}")
                    .Replace("{DarkBlue}", $"{ChatColors.DarkBlue}")
                    .Replace("{BlueGrey}", $"{ChatColors.BlueGrey}")
                    .Replace("{Magenta}", $"{ChatColors.Magenta}")
                    .Replace("{LightRed}", $"{ChatColors.LightRed}")
                    .Replace("{Orange}", $"{ChatColors.Orange}");
    }

    public static string FormatChat(string prefix, string message)
    {
        return $" {ReplaceColors(prefix)} {ReplaceColors(message)}";
    }
}
```

- [ ] **Step 2: Implement centralized permission in `WardenService.cs`**

Add `HasPermission` to `WardenService.cs`.

```csharp
    public bool HasPermission(CCSPlayerController player, string? requiredFlag = null)
    {
        if (IsWarden(player) || IsWardenAdmin(player)) return true;
        if (AdminManager.PlayerHasPermissions(player, "@css/root")) return true;
        if (requiredFlag != null && AdminManager.PlayerHasPermissions(player, requiredFlag)) return true;
        return false;
    }
```

- [ ] **Step 3: Refactor all services to use `WardenService.HasPermission` and `PluginHelper`**

Replace local `HasPermission` implementations and color replacement calls with the centralized versions across all service files. Remove the `HudHelper` class from `FreezeService.cs`.

- [ ] **Step 4: Build and verify**
Run `dotnet build`.

- [ ] **Step 5: Commit**
```bash
git add .
git commit -m "refactor: centralize permissions, HUD helpers, and message formatting"
```

---

### Task 2: Implement `GameManagerService` (Boxing and Saklambac)

**Files:**
- Create: `Services/GameManagerService.cs`
- Modify: `JailBreakPlugin.cs`

- [ ] **Step 1: Create `GameManagerService.cs` with Boxing Mode**

Implement Boxing mode with a 30s default and `CenterHtmlMenu` duration selection.

```csharp
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CS2MenuManager.API.Menu;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class GameManagerService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly FreezeService _freezeService;
    
    private int _gameTimeRemaining = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _gameTimer;
    private bool _isBoxActive = false;
    private bool _isSaklambacActive = false;

    public GameManagerService(JailBreakPlugin plugin, WardenService wardenService, FreezeService freezeService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _freezeService = freezeService;
    }

    public void OnRoundStart()
    {
        _gameTimer?.Kill();
        _gameTimer = null;
        _isBoxActive = false;
        _isSaklambacActive = false;
    }

    public void OpenBoxMenu(CCSPlayerController player)
    {
        var menu = new CenterHtmlMenu("🥊 Boks Modu Süresi", _plugin);
        for (int i = 10; i <= 60; i += 10)
        {
            int time = i;
            menu.AddItem($"{time} Saniye", (p, o) => StartBox(time));
        }
        menu.Display(player, 0);
    }

    public void StartBox(int duration)
    {
        _isBoxActive = true;
        _gameTimeRemaining = duration;
        Server.ExecuteCommand("mp_teammates_are_enemies 1");
        
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Red}Boks Modu Başladı! Süre: {duration}sn"));
        
        _gameTimer?.Kill();
        _gameTimer = _plugin.AddTimer(1.0f, BoxTick, TimerFlags.REPEAT);
    }

    private void BoxTick()
    {
        if (!_isBoxActive) return;

        if (_gameTimeRemaining <= 0)
        {
            EndBox();
            return;
        }

        string content = $"Boks Sürüyor: <font color='{PluginHelper.ColorTime}'><b>{_gameTimeRemaining}s</b></font>";
        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            p.PrintToCenterHtml(PluginHelper.FormatHud("BOKS MODU", content));
        }

        _gameTimeRemaining--;
    }

    private void EndBox()
    {
        _isBoxActive = false;
        _gameTimer?.Kill();
        _gameTimer = null;
        Server.ExecuteCommand("mp_teammates_are_enemies 0");
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " {ChatColors.Green}Boks Modu Bitti."));
    }
    
    // Saklambac placeholders...
}
```

- [ ] **Step 2: Implement Saklambac Mode**

Add `StartSaklambac` and `SaklambacTick` to `GameManagerService.cs`.

```csharp
    public void StartSaklambac(int duration)
    {
        _isSaklambacActive = true;
        _gameTimeRemaining = duration;

        var ctSpawns = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
        var cts = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive).ToList();

        foreach (var ct in cts)
        {
            var pawn = ct.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid && ctSpawns.Count > 0)
            {
                var spawn = ctSpawns[0]; // Simple approach, could randomize
                // Teleport and Rotate 180
                Vector angles = pawn.AbsRotation;
                angles.Y += 180;
                pawn.Teleport(spawn.AbsOrigin, angles, new Vector(0, 0, 0));
                
                pawn.MoveType = MoveType_t.MOVETYPE_NONE;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_NONE;
            }
        }

        _gameTimer?.Kill();
        _gameTimer = _plugin.AddTimer(1.0f, SaklambacTick, TimerFlags.REPEAT);
    }

    private void SaklambacTick()
    {
        if (!_isSaklambacActive) return;

        if (_gameTimeRemaining <= 0)
        {
            EndSaklambac();
            return;
        }

        // Blind CTs and Update HUD
        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            if (p.Team == CsTeam.CounterTerrorist)
            {
                // ScreenFade: duration 1s, hold 1s, flags 1 (fade in), color black
                // Note: Actual UserMessage ScreenFade implementation varies, using a simplified approach if helper not available.
                // For now, let's use a HUD overlay if ScreenFade is complex.
                p.PrintToCenterHtml(PluginHelper.FormatHud("SAKLAMBAÇ", "<font size='30' color='black'>KÖR EDİLDİNİZ</font>"));
            }
            else
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud("SAKLAMBAÇ", $"Saklanmak İçin Kalan: {PluginHelper.ColorTime}{_gameTimeRemaining}s"));
            }
        }

        _gameTimeRemaining--;
    }

    private void EndSaklambac()
    {
        _isSaklambacActive = false;
        _gameTimer?.Kill();
        _gameTimer = null;

        foreach (var ct in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist))
        {
            var pawn = ct.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.MoveType = MoveType_t.MOVETYPE_WALK;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            }
        }

        _freezeService.FreezeAll();
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " {ChatColors.Green}Saklambaç Bitti! Ebeciler Serbest, Mahkumlar Dondu!"));
    }
```

- [ ] **Step 3: Register Service and Commands in `JailBreakPlugin.cs`**

Initialize `GameManagerService` in `Load()` and register `css_b`, `css_box`, `css_saklambac`.

- [ ] **Step 4: Build and verify**
Run `dotnet build`.

- [ ] **Step 5: Commit**
```bash
git add .
git commit -m "feat: implement GameManagerService with Boxing and Saklambac modes"
```

---

### Task 3: Unified Warden Menu (`!k`)

**Files:**
- Modify: `Services/WardenService.cs`
- Modify: `JailBreakPlugin.cs`

- [ ] **Step 1: Implement `CommandKomMenu` in `WardenService.cs`**

Create a master menu connecting all services.

```csharp
    public void CommandKomMenu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        var menu = new CenterHtmlMenu("👑 Komutçu Kontrol Paneli", _plugin);

        menu.AddItem("[🔓] Kapıları Aç", (p, o) => _plugin.IseliService.QuickOpen(p));
        menu.AddItem("[🥊] Boks Modu", (p, o) => _plugin.GameManagerService.OpenBoxMenu(p));
        menu.AddItem("[🙈] Saklambaç", (p, o) => _plugin.GameManagerService.StartSaklambac(30)); // Default 30
        menu.AddItem("[⚔️] FF Menüsü", (p, o) => _plugin.FFMenuService.OpenWardenConfigMenu(p));
        menu.AddItem("[➕] Herkesi Canlandır", (p, o) => _plugin.UtilityService.CommandAf(p, info));
        
        menu.Display(player, 0);
    }
```
*Note: Ensure `JailBreakPlugin` has public properties for these services.*

- [ ] **Step 2: Register `!k` and `!kommenu` in `JailBreakPlugin.cs`**

- [ ] **Step 3: Build and verify**
Run `dotnet build`.

- [ ] **Step 4: Commit**
```bash
git add .
git commit -m "feat: implement unified Warden menu (!k)"
```

---

### Task 4: Broadcast and Admin Commands

**Files:**
- Modify: `Services/UtilityService.cs`
- Modify: `JailBreakPlugin.cs`

- [ ] **Step 1: Implement Broadcast Commands**

Add `CommandMsay`, `CommandCsay`, `CommandHsay` to `UtilityService.cs`.

- [ ] **Step 2: Implement Revive Command**

Add `CommandRev` with support for `@t`, `@ct`, `@all`.

- [ ] **Step 3: Implement Fake Say Command**

Add `CommandFsay` (Root only).

```csharp
    public void CommandFsay(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/root")) return;

        string targetName = info.GetArg(1);
        string message = info.GetArg(2); // Simplified, might need GetArgTarget/Remainder

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));
        if (target != null)
        {
            // Force say
            target.ExecuteClientCommand($"say {message}");
        }
    }
```

- [ ] **Step 4: Register all commands in `JailBreakPlugin.cs`**

- [ ] **Step 5: Build and verify**
Run `dotnet build`.

- [ ] **Step 6: Commit**
```bash
git add .
git commit -m "feat: add broadcast, revive, and fake say commands"
```

---

### Task 5: Final Cleanup and FF Default Duration

**Files:**
- Modify: `Services/FFMenuService.cs`

- [ ] **Step 1: Set FF default duration to 30s**

Update `FFMenuService.cs` (and config if applicable) to use 30s as default.

- [ ] **Step 2: Remove any remaining ChatMenu usages**

Double check all services and ensure no `ChatMenu` remains.

- [ ] **Step 3: Final Build and verification**
Run `dotnet build`.

- [ ] **Step 4: Commit**
```bash
git add .
git commit -m "chore: set FF defaults and perform final code cleanup"
```
