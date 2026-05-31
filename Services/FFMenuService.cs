using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using JailBreak.Config;
using CS2MenuManager.API.Menu;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Enum;

namespace JailBreak.Services;

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

public class FFMenuService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly FreezeService _freezeService;

    private FFConfigState _currentFFConfig = new();
    private Dictionary<ulong, PlayerFFSelection> _playerSelections = new();
    private bool _isSelectionPhaseActive = false;
    private int _selectionTimeRemaining = 0;

    private bool _isFFActive = false;
    private bool _isCountingToStart = false;
    private bool _isCountingToEnd = false;
    private int _ffRemainingTime = 0;
    private bool _freezeOnEnd = false;

    private CounterStrikeSharp.API.Modules.Timers.Timer? _tickTimer;

    public FFMenuService(JailBreakPlugin plugin, WardenService wardenService, FreezeService freezeService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _freezeService = freezeService;
    }

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

    public void CommandFFMenu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_plugin.IsJailbreakMap()) return;
        if (!HasPermission(player)) return;

        string arg = info.GetArg(1);
        if (!int.TryParse(arg, out int time))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFFMenuUsage)}");
            return;
        }

        StartFFMenu(time, player);
    }

    public void CommandFFKapat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_plugin.IsJailbreakMap()) return;
        if (!HasPermission(player)) return;

        DisableFF();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFFDisabled)}");
    }

    public void CommandFF0(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_plugin.IsJailbreakMap()) return;
        if (!HasPermission(player)) return;

        DisableFF();
        StripTWeapons();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFF0Applied)}");
    }

    public void CommandFFOndur(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_plugin.IsJailbreakMap()) return;
        if (!HasPermission(player)) return;

        string arg = info.GetArg(1);
        if (!int.TryParse(arg, out int time))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFFOndurUsage)}");
            return;
        }

        _freezeOnEnd = true;
        _isCountingToEnd = true;
        _ffRemainingTime = time;
        _isCountingToStart = false;
        _isSelectionPhaseActive = false;

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, CountdownTick, TimerFlags.REPEAT);

        UpdateHUD(); // Show HUD immediately

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFFOndurApplied)}");
    }

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
        
        // To make "Back" work perfectly, create the config menu again:
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

    public bool HandleFFMenuChat(CCSPlayerController player, string message)
    {
        // Intercept !1, !2 etc. so it doesn't show in chat while menus are active anywhere in CS2MenuManager
        if (message.StartsWith("!") && int.TryParse(message.Substring(1), out _))
        {
            return true; // Stop chat processing for menu number inputs globally or selectively
        }
        return false;
    }

    private void EnableFF()
    {
        Server.ExecuteCommand("mp_teammates_are_enemies 1");
        _isFFActive = true;
        _isCountingToStart = false;
    }

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
        _isSelectionPhaseActive = false;
        _playerSelections.Clear();
        _isCountingToStart = false;
        _isCountingToEnd = false;
        _ffRemainingTime = 0;
        _freezeOnEnd = false;
    }

    private void EndFF()
    {
        DisableFF();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgFFEnded)}");

        if (_freezeOnEnd)
        {
            _freezeService.FreezeAll();
        }
    }

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

    private void StripTWeapons()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            player.RemoveWeapons();
            player.GiveNamedItem("weapon_knife");
        }
    }

    private bool HasPermission(CCSPlayerController player)
    {
        return _wardenService.IsWarden(player) ||
               AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") ||
               AdminManager.PlayerHasPermissions(player, "@css/changemap") ||
               AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
