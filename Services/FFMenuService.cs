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

public enum FFMenuStep
{
    None,
    Primary,
    Secondary
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
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);

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
    }

    private void Tick()
    {
    }

    public void OnTick()
    {
    }

    private void NextPage()
    {
    }

    private void PrevPage()
    {
    }

    private void UpdateHUD()
    {
    }

    public bool HandleFFMenuChat(CCSPlayerController player, string message)
    {
        return false;
    }

    private void SelectWeapon(FFWeapon weapon)
    {
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
