# FF Menu Overhaul Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Overhaul the Friendly Fire (FF) weapon selection system to use interactive `CenterHtmlMenu`s, resolving overlapping HUD issues, and migrate all remaining `ChatMenu` uses to `CenterHtmlMenu`.

**Architecture:** We will introduce an internal state tracking class `FFMenuState` within `FFMenuService` to hold the Warden's configuration. The service will utilize `CS2MenuManager.API.Menu.CenterHtmlMenu` to present configurations to the Warden, and then subsequently present selection menus to the T team. We will strip the old chat-based command interception logic. Finally, we will refactor `WardenService`'s admin menu to match this unified UI approach.

**Tech Stack:** C#, CounterStrikeSharp, CS2MenuManager

---

### Task 1: Refactor `WardenService` Admin Menu

**Files:**
- Modify: `Services/WardenService.cs`

- [ ] **Step 1: Replace ChatMenu with CenterHtmlMenu in `CommandWardenAdmin`**

In `Services/WardenService.cs`, locate the `CommandWardenAdmin` method. Update it to use `CenterHtmlMenu` instead of `ChatMenu`.

```csharp
    public void CommandWardenAdmin(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        bool isWarden = IsWarden(player);
        bool isRoot = AdminManager.PlayerHasPermissions(player, "@css/root") || AdminManager.PlayerHasPermissions(player, "@css/cvar");
        bool isWardenAdmin = IsWardenAdmin(player);

        if (!isWarden && !isRoot && !isWardenAdmin)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyWardenAdminOrRootCanUse)}");
            return;
        }

        string targetName = info.GetArg(1);
        if (!string.IsNullOrEmpty(targetName))
        {
            var target = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid && !p.IsBot && p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                AddWardenAdmin(player, target);
            }
            else
            {
                player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgPlayerNotFound)}");
            }
            return;
        }

        var players = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.SteamID != player.SteamID && AdminManager.PlayerHasPermissions(p, "@css/generic")).ToList();

        if (players.Count == 0)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgNoOtherAdmins)}");
            return;
        }

        var menu = new CenterHtmlMenu("Komutçu Admin Seçimi", _plugin);

        foreach (var p in players)
        {
            // Capture the target player reference for the closure
            var targetPlayer = p;
            menu.AddItem(targetPlayer.PlayerName, (caller, option) =>
            {
                if (targetPlayer.IsValid) AddWardenAdmin(caller, targetPlayer);
            });
        }

        menu.Display(player, 0);
    }
```

- [ ] **Step 2: Clean up unused imports**

Ensure `using ChatMenu = CounterStrikeSharp.API.Modules.Menu.ChatMenu;` is removed from `WardenService.cs` as it is no longer needed.

- [ ] **Step 3: Build and verify**
Run `dotnet build` to ensure the project still compiles correctly.

- [ ] **Step 4: Commit**
```bash
git add Services/WardenService.cs
git commit -m "refactor: migrate warden admin menu to CenterHtmlMenu"
```

---

### Task 2: Define FF Configuration State and Menus

**Files:**
- Modify: `Services/FFMenuService.cs`

- [ ] **Step 1: Add imports and state class**

Add necessary using directives at the top of `FFMenuService.cs`:
```csharp
using CS2MenuManager.API.Menu;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Enum;
```

Inside the `JailBreak.Services` namespace, create a new class `FFConfigState` to hold the Warden's configurations, and an object `PlayerFFSelection` for player choices. Also modify the main fields of `FFMenuService`.

```csharp
public class FFConfigState
{
    public HashSet<string> ActivePrimaries { get; set; } = new();
    public HashSet<string> ActiveSecondaries { get; set; } = new();
    public bool BunnyEnabled { get; set; } = false;
    public int CountdownTime { get; set; } = 10;
}

public class PlayerFFSelection
{
    public string PrimaryItem { get; set; } = "";
    public string SecondaryItem { get; set; } = "";
    public bool PrimarySelected { get; set; } = false;
    public bool SecondarySelected { get; set; } = false;
}
```

- [ ] **Step 2: Update `FFMenuService` class fields**

Remove the old chat/pagination related fields (`_currentStep`, `_currentPage`, `ItemsPerPage`, `_lastButtons`, `_selectedPrimaryName`, etc.).
Add fields for the new workflow.

```csharp
    private FFConfigState _currentFFConfig = new();
    private Dictionary<ulong, PlayerFFSelection> _playerSelections = new();
    private bool _isSelectionPhaseActive = false;
    private int _selectionTimeRemaining = 0;
```

- [ ] **Step 3: Update `OnRoundStart` and command resets**

Modify `OnRoundStart` to clear the new state fields, and update the command executions (`CommandFFMenu`, `CommandFFKapat`, `CommandFF0`, `CommandFFOndur`) to reset these.

```csharp
    public void OnRoundStart()
    {
        DisableFF(true);
        _isSelectionPhaseActive = false;
        _isCountingToStart = false;
        _isCountingToEnd = false;
        _tickTimer?.Kill();
        _tickTimer = null;
        _playerSelections.Clear();
    }
```

- [ ] **Step 4: Build to ensure no syntax errors**
Run `dotnet build`. There will be errors due to removed variables (like `_currentStep`). Comment out or delete the bodies of `Tick()`, `UpdateHUD()`, `HandleFFMenuChat()`, `NextPage()`, `PrevPage()`, `OnTick()`, `StartFFMenu()`, and `SelectWeapon()` for now. We will rewrite these completely in the next tasks.

- [ ] **Step 5: Commit**
```bash
git add Services/FFMenuService.cs
git commit -m "refactor: setup state structures for new FF menu workflow"
```

---

### Task 3: Build the Warden FF Settings Menu

**Files:**
- Modify: `Services/FFMenuService.cs`

- [ ] **Step 1: Create `StartFFMenu` logic**

Rewrite `StartFFMenu` to initialize the `FFConfigState` and open the Warden config menu.

```csharp
    private void StartFFMenu(int initialTime, CCSPlayerController player)
    {
        _currentFFConfig = new FFConfigState
        {
            CountdownTime = initialTime,
            BunnyEnabled = false
        };

        foreach (var wp in _plugin.Config.FFPrimaryWeaponList) _currentFFConfig.ActivePrimaries.Add(wp.ItemName);
        foreach (var wp in _plugin.Config.FFSecondaryWeaponList) _currentFFConfig.ActiveSecondaries.Add(wp.ItemName);

        OpenWardenConfigMenu(player);
    }
```
Update `CommandFFMenu` to pass `player` to `StartFFMenu(time, player)`.

- [ ] **Step 2: Implement `OpenWardenConfigMenu`**

```csharp
    private void OpenWardenConfigMenu(CCSPlayerController warden)
    {
        var menu = new CenterHtmlMenu("⚙️ FF Ayar Menüsü", _plugin);

        menu.AddItem("➡ Birincil Silahlar", (p, o) => OpenWardenPrimaryConfigMenu(p));
        menu.AddItem("➡ İkincil Silahlar", (p, o) => OpenWardenSecondaryConfigMenu(p));
        
        string bunnyStatus = _currentFFConfig.BunnyEnabled ? "AÇIK" : "KAPALI";
        menu.AddItem($"🔄 Bunny Durumu: {bunnyStatus}", (p, o) => 
        {
            _currentFFConfig.BunnyEnabled = !_currentFFConfig.BunnyEnabled;
            OpenWardenConfigMenu(p);
        });

        menu.AddItem($"⏳ FF Açılma Süresi: {_currentFFConfig.CountdownTime} Saniye", (p, o) => 
        {
            _currentFFConfig.CountdownTime += 10;
            if (_currentFFConfig.CountdownTime > 50) _currentFFConfig.CountdownTime = 10;
            OpenWardenConfigMenu(p);
        });

        menu.AddItem("▶ FF Başlat!", (p, o) => 
        {
            StartTWeaponSelectionPhase();
        });

        menu.Display(warden, 0);
    }
```

- [ ] **Step 3: Implement sub-menus for weapon toggling**

```csharp
    private void OpenWardenPrimaryConfigMenu(CCSPlayerController warden)
    {
        var menu = new CenterHtmlMenu("Birincil Silah Ayarları", _plugin);
        foreach (var wp in _plugin.Config.FFPrimaryWeaponList)
        {
            bool isActive = _currentFFConfig.ActivePrimaries.Contains(wp.ItemName);
            string status = isActive ? "[AÇIK]" : "[KAPALI]";
            menu.AddItem($"{status} {wp.Name}", (p, o) => 
            {
                if (isActive) _currentFFConfig.ActivePrimaries.Remove(wp.ItemName);
                else _currentFFConfig.ActivePrimaries.Add(wp.ItemName);
                OpenWardenPrimaryConfigMenu(p);
            });
        }
        menu.PrevMenu = new CenterHtmlMenu("⚙️ FF Ayar Menüsü", _plugin); // Dummy back, we intercept back action if needed, or simply let the user use standard back.
        // To make "Back" work perfectly, create the config menu again:
        var backMenu = new CenterHtmlMenu("⚙️ FF Ayar Menüsü", _plugin);
        backMenu.AddItem("Geri Dön", (p, o) => OpenWardenConfigMenu(p));
        menu.AddItem("⬅ Geri", (p, o) => OpenWardenConfigMenu(p));

        menu.Display(warden, 0);
    }

    private void OpenWardenSecondaryConfigMenu(CCSPlayerController warden)
    {
        var menu = new CenterHtmlMenu("İkincil Silah Ayarları", _plugin);
        foreach (var wp in _plugin.Config.FFSecondaryWeaponList)
        {
            bool isActive = _currentFFConfig.ActiveSecondaries.Contains(wp.ItemName);
            string status = isActive ? "[AÇIK]" : "[KAPALI]";
            menu.AddItem($"{status} {wp.Name}", (p, o) => 
            {
                if (isActive) _currentFFConfig.ActiveSecondaries.Remove(wp.ItemName);
                else _currentFFConfig.ActiveSecondaries.Add(wp.ItemName);
                OpenWardenSecondaryConfigMenu(p);
            });
        }
        menu.AddItem("⬅ Geri", (p, o) => OpenWardenConfigMenu(p));

        menu.Display(warden, 0);
    }
```

- [ ] **Step 4: Commit**
```bash
git add Services/FFMenuService.cs
git commit -m "feat: implement warden FF config CenterHtmlMenus"
```

---

### Task 4: Terrorist Weapon Selection Workflow

**Files:**
- Modify: `Services/FFMenuService.cs`

- [ ] **Step 1: Implement `StartTWeaponSelectionPhase`**

```csharp
    private void StartTWeaponSelectionPhase()
    {
        _isSelectionPhaseActive = true;
        _selectionTimeRemaining = 15; // 15 seconds to choose
        _playerSelections.Clear();

        var ts = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();
        foreach (var t in ts)
        {
            _playerSelections[t.SteamID] = new PlayerFFSelection();
            OpenTPrimarySelectionMenu(t);
        }

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, SelectionTick, TimerFlags.REPEAT);
    }
```

- [ ] **Step 2: Implement T player selection menus**

```csharp
    private void OpenTPrimarySelectionMenu(CCSPlayerController player)
    {
        if (!_isSelectionPhaseActive) return;
        var menu = new CenterHtmlMenu("Birincil Silah Seç", _plugin);
        
        foreach (var wp in _plugin.Config.FFPrimaryWeaponList)
        {
            if (_currentFFConfig.ActivePrimaries.Contains(wp.ItemName))
            {
                menu.AddItem(wp.Name, (p, o) => 
                {
                    if (_playerSelections.TryGetValue(p.SteamID, out var sel))
                    {
                        sel.PrimaryItem = wp.ItemName;
                        sel.PrimarySelected = true;
                    }
                    OpenTSecondarySelectionMenu(p);
                });
            }
        }
        menu.Display(player, 0);
    }

    private void OpenTSecondarySelectionMenu(CCSPlayerController player)
    {
        if (!_isSelectionPhaseActive) return;
        var menu = new CenterHtmlMenu("İkincil Silah Seç", _plugin);
        
        foreach (var wp in _plugin.Config.FFSecondaryWeaponList)
        {
            if (_currentFFConfig.ActiveSecondaries.Contains(wp.ItemName))
            {
                menu.AddItem(wp.Name, (p, o) => 
                {
                    if (_playerSelections.TryGetValue(p.SteamID, out var sel))
                    {
                        sel.SecondaryItem = wp.ItemName;
                        sel.SecondarySelected = true;
                    }
                    // Close menu by displaying an empty/dummy menu or letting it expire
                    var emptyMenu = new CenterHtmlMenu("Seçim Bekleniyor...", _plugin);
                    emptyMenu.Display(player, 0);
                    CheckAllSelectionsCompleted();
                });
            }
        }
        menu.Display(player, 0);
    }
```

- [ ] **Step 3: Implement Timer tracking and finalize phase**

```csharp
    private void CheckAllSelectionsCompleted()
    {
        var ts = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();
        bool allDone = true;
        foreach (var t in ts)
        {
            if (_playerSelections.TryGetValue(t.SteamID, out var sel))
            {
                if (!sel.PrimarySelected || !sel.SecondarySelected) allDone = false;
            }
            else { allDone = false; }
        }

        if (allDone && _isSelectionPhaseActive)
        {
            FinalizeSelectionPhase();
        }
    }

    private void SelectionTick()
    {
        if (!_isSelectionPhaseActive) return;

        _selectionTimeRemaining--;
        if (_selectionTimeRemaining <= 0)
        {
            FinalizeSelectionPhase();
        }
    }

    private void FinalizeSelectionPhase()
    {
        _isSelectionPhaseActive = false;
        _tickTimer?.Kill();
        
        GiveWeaponsToTs();

        if (_currentFFConfig.BunnyEnabled)
        {
            Server.ExecuteCommand("sv_autobunnyhopping 1");
            Server.ExecuteCommand("sv_enablebunnyhopping 1");
        }

        // Start final HUD countdown
        _isCountingToStart = true;
        _ffRemainingTime = _currentFFConfig.CountdownTime;
        _tickTimer = _plugin.AddTimer(1.0f, CountdownTick, TimerFlags.REPEAT);
    }
```

- [ ] **Step 4: Update `GiveWeaponsToTs`**

```csharp
    private void GiveWeaponsToTs()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            player.RemoveWeapons();
            player.GiveNamedItem("weapon_knife");

            if (_playerSelections.TryGetValue(player.SteamID, out var sel))
            {
                if (sel.PrimarySelected && sel.PrimaryItem != "none" && !string.IsNullOrEmpty(sel.PrimaryItem))
                    player.GiveNamedItem(sel.PrimaryItem);

                if (sel.SecondarySelected && sel.SecondaryItem != "none" && !string.IsNullOrEmpty(sel.SecondaryItem))
                    player.GiveNamedItem(sel.SecondaryItem);
            }
        }
    }
```

- [ ] **Step 5: Commit**
```bash
git add Services/FFMenuService.cs
git commit -m "feat: implement T team weapon selection and phase finalize"
```

---

### Task 5: Implement Countdown and Execution

**Files:**
- Modify: `Services/FFMenuService.cs`

- [ ] **Step 1: Write `CountdownTick` and `UpdateHUD`**

```csharp
    private void CountdownTick()
    {
        if (!_isCountingToStart && !_isCountingToEnd && !_isFFActive)
        {
            _tickTimer?.Kill();
            _tickTimer = null;
            return;
        }

        if (_ffRemainingTime > 0)
        {
            _ffRemainingTime--;
        }
        else
        {
            if (_isCountingToStart)
            {
                _isCountingToStart = false;
                _isFFActive = true;
                EnableFF();
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFFActiveNow)}");
            }
            else if (_isCountingToEnd)
            {
                EndFF();
            }
            else if (!_isFFActive)
            {
                _tickTimer?.Kill();
                _tickTimer = null;
            }
            return;
        }

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        string title = _plugin.Lang.HudTitleFFSystem;
        string content = "";
        string instruction = "";

        if (_isCountingToStart)
        {
            title = _plugin.Lang.HudTitleFFStartDelay;
            content = string.Format(_plugin.Lang.HudContentFFStartDelay, HudHelper.ColorSuccess, "Özel Silahlar", HudHelper.ColorTime, _ffRemainingTime);
        }
        else if (_isCountingToEnd)
        {
            title = _plugin.Lang.HudTitleFFEndDelay;
            content = string.Format(_plugin.Lang.HudContentFFEndDelay, HudHelper.ColorTime, _ffRemainingTime);
            if (_freezeOnEnd) content += string.Format(_plugin.Lang.HudContentFFFreezeWarning, HudHelper.ColorSystem);
        }
        else if (_isFFActive)
        {
            title = _plugin.Lang.HudTitleFFActive;
            content = string.Format(_plugin.Lang.HudContentFFActiveWeapons, HudHelper.ColorSuccess, "Özel Silahlar");
            instruction = _plugin.Lang.HudInstructionFFOndurInfo;
        }

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            p.PrintToCenterHtml(HudHelper.FormatHud(title, content, instruction));
        }
    }
```

- [ ] **Step 2: Update `DisableFF` to disable Bunny Hop if needed**
Add `sv_autobunnyhopping 0` and `sv_enablebunnyhopping 0` to `DisableFF` if bunny was enabled during the round.

```csharp
    private void DisableFF(bool silent = false)
    {
        Server.ExecuteCommand("mp_teammates_are_enemies 0");
        if (_currentFFConfig.BunnyEnabled)
        {
            Server.ExecuteCommand("sv_autobunnyhopping 0");
            Server.ExecuteCommand("sv_enablebunnyhopping 0");
            _currentFFConfig.BunnyEnabled = false;
        }
        _isFFActive = false;
        _isCountingToStart = false;
        _isCountingToEnd = false;
        _ffRemainingTime = 0;
        _freezeOnEnd = false;
    }
```

- [ ] **Step 3: Handle Chat Hook Cleanup**

Locate `HandleFFMenuChat` in `FFMenuService.cs` and replace its body with:
```csharp
    public bool HandleFFMenuChat(CCSPlayerController player, string message)
    {
        // Intercept !1, !2 etc. so it doesn't show in chat while menus are active anywhere in CS2MenuManager
        if (message.StartsWith("!") && int.TryParse(message.Substring(1), out _))
        {
            return true; // Stop chat processing for menu number inputs globally or selectively
        }
        return false;
    }
```

- [ ] **Step 4: Clean up obsolete methods**
Ensure `NextPage()`, `PrevPage()`, `OnTick()`, `SelectWeapon()`, and `Tick()` are completely deleted from `FFMenuService.cs`.

- [ ] **Step 5: Ensure JailBreakPlugin.cs builds successfully**
Run `dotnet build`. Remove `RegisterListener<Listeners.OnTick>(_ffMenuService.OnTick);` from `JailBreakPlugin.cs` line 116 since `OnTick` is deleted.

- [ ] **Step 6: Commit**
```bash
git add Services/FFMenuService.cs JailBreakPlugin.cs
git commit -m "feat: complete execution phase and clean up legacy menu code"
```