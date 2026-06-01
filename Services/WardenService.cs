using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using JailBreak.Models;
using System.Drawing;
using System.Text.Json;
using CS2MenuManager.API.Menu;
using CenterHtmlMenu = CS2MenuManager.API.Menu.CenterHtmlMenu;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class WardenService
{
    private readonly JailBreakPlugin _plugin;

    public CCSPlayerController? CurrentWarden { get; private set; }
    private DateTime? _wardenStartTime;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _wardenTimer;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _rgbTimer;
    private readonly HashSet<ulong> _wardenAdmins = new();
    private readonly HashSet<ulong> _godModePlayers = new();
    private readonly Dictionary<ulong, uint> _originalImmunity = new();
    private float _hue = 0;
    private List<WardenStat> _wardenStats = new();
    private List<WardenStat> _wardenAdminStats = new();
    private readonly string _statsPath;
    private readonly string _adminStatsPath;
    private readonly Dictionary<ulong, DateTime> _wardenAdminStartTimes = new();

    public WardenService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
        _statsPath = Path.Combine(_plugin.ModuleDirectory, "../../configs/plugins/JailBreak/WardenStats.json");
        _adminStatsPath = Path.Combine(_plugin.ModuleDirectory, "../../configs/plugins/JailBreak/WardenAdminStats.json");
        LoadStats();
        LoadAdminStats();
    }

    public void RegisterCommands(ICommandDispatcher dispatcher)
    {
        dispatcher.RegisterCommand("css_w", "Komutçu ol", CommandBecomeWarden);
        dispatcher.RegisterCommand("css_uw", "Komutçuluktan çık", CommandUnwarden);
        dispatcher.RegisterCommand("css_komkalan", "Komutçunun kalan süresini gör", CommandKomKalan);
        dispatcher.RegisterCommand("css_topkomutcu", "En çok komutçu olanları gör", CommandTopKomutcu);
        dispatcher.RegisterCommand("css_q", "Komutçu koruması (God, MuteT)", CommandQ);
        dispatcher.RegisterCommand("css_qq", "Komutçu korumasını kaldır", CommandQQ);
        dispatcher.RegisterCommand("css_ka", "Komutçu admin menüsü", CommandWardenAdmin);
        dispatcher.RegisterCommand("css_kasil", "Komutçu adminini kaldır", CommandRemoveWardenAdmin);
        dispatcher.RegisterCommand("css_topka", "En çok komutçu admin olanları gör", CommandTopKa);
        dispatcher.RegisterCommand("css_k", "Komutçu ana menüsünü açar", CommandKomMenu);
        dispatcher.RegisterCommand("css_kommenu", "Komutçu ana menüsünü açar", CommandKomMenu);
    }

    private void LoadStats()
    {
        LogHelper.LogDebug("WardenService: Loading warden stats...");
        try
        {
            if (File.Exists(_statsPath))
            {
                string json = File.ReadAllText(_statsPath);
                _wardenStats = JsonSerializer.Deserialize<List<WardenStat>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error loading warden stats.", ex);
        }
    }

    private void SaveStats()
    {
        LogHelper.LogDebug("WardenService: Saving warden stats...");
        try
        {
            string dir = Path.GetDirectoryName(_statsPath) ?? "";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(_wardenStats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_statsPath, json);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error saving warden stats.", ex);
        }
    }

    private void UpdateStats(CCSPlayerController player, double seconds)
    {
        var stat = _wardenStats.FirstOrDefault(s => s.SteamID == player.SteamID);
        if (stat == null)
        {
            stat = new WardenStat
            {
                SteamID = player.SteamID,
                PlayerName = player.PlayerName,
                TotalTimeSeconds = 0
            };
            _wardenStats.Add(stat);
        }
        else
        {
            stat.PlayerName = player.PlayerName; 
        }

        stat.TotalTimeSeconds += seconds;
        SaveStats();
    }

    private void LoadAdminStats()
    {
        LogHelper.LogDebug("WardenService: Loading warden admin stats...");
        try
        {
            if (File.Exists(_adminStatsPath))
            {
                string json = File.ReadAllText(_adminStatsPath);
                _wardenAdminStats = JsonSerializer.Deserialize<List<WardenStat>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error loading warden admin stats.", ex);
        }
    }

    private void SaveAdminStats()
    {
        LogHelper.LogDebug("WardenService: Saving warden admin stats...");
        try
        {
            string dir = Path.GetDirectoryName(_adminStatsPath) ?? "";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(_wardenAdminStats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_adminStatsPath, json);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error saving warden admin stats.", ex);
        }
    }

    private void UpdateAdminStats(CCSPlayerController player, double seconds)
    {
        var stat = _wardenAdminStats.FirstOrDefault(s => s.SteamID == player.SteamID);
        if (stat == null)
        {
            stat = new WardenStat
            {
                SteamID = player.SteamID,
                PlayerName = player.PlayerName,
                TotalTimeSeconds = 0
            };
            _wardenAdminStats.Add(stat);
        }
        else
        {
            stat.PlayerName = player.PlayerName;
        }

        stat.TotalTimeSeconds += seconds;
        SaveAdminStats();
    }

    public bool IsWarden(CCSPlayerController player)
    {
        return CurrentWarden != null && CurrentWarden.IsValid && CurrentWarden.SteamID == player.SteamID;
    }

    public bool IsWardenAdmin(CCSPlayerController player)
    {
        return _wardenAdmins.Contains(player.SteamID);
    }

    public bool IsGodMode(ulong steamId)
    {
        return _godModePlayers.Contains(steamId);
    }

    public bool HasPermission(CCSPlayerController? player, string? requiredFlag = null)
    {
        if (player == null) return true; // Console command
        if (!player.IsValid) return false;

        if (AdminManager.PlayerHasPermissions(player, "@css/root")) return true;
        if (IsWarden(player) || IsWardenAdmin(player)) return true;
        
        if (requiredFlag != null && AdminManager.PlayerHasPermissions(player, requiredFlag)) return true;
        
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNoPermission));
        return false;
    }

    public void OnRoundStart()
    {
        _godModePlayers.Clear();
        if (CurrentWarden != null && !CurrentWarden.IsValid)
        {
            RemoveWarden();
        }
    }

    public void OnClientDisconnect(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player == null || !player.IsValid) return;

        if (IsWarden(player))
        {
            RemoveWarden();
        }

        if (_wardenAdmins.Contains(player.SteamID))
        {
            RemoveWardenAdmin(player);
        }
    }

    public void SetWarden(CCSPlayerController player)
    {
        if (CurrentWarden != null)
        {
            RemoveWarden();
        }

        CurrentWarden = player;
        _wardenStartTime = DateTime.Now;

        if (player.Team != CsTeam.CounterTerrorist)
        {
            player.ChangeTeam(CsTeam.CounterTerrorist);
        }

        AdminManager.AddPlayerPermissions(player, "@jailbreak/warden");

        _originalImmunity[player.SteamID] = (uint)player.Score; 
        AdminManager.SetPlayerImmunity(player, 100);

        _wardenTimer?.Kill();
        _wardenTimer = _plugin.AddTimer(_plugin.Config.WardenDurationMinutes * 60f, () =>
        {
            if (CurrentWarden != null && CurrentWarden.IsValid)
            {
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgWardenDurationExpired));
                _plugin.VoteService.StartKickVotePhase();
            }
        });

        _rgbTimer?.Kill();
        _hue = 0;
        _rgbTimer = _plugin.AddTimer(0.1f, UpdateWardenRgb, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgNewWarden, player.PlayerName)));
    }

    private void UpdateWardenRgb()
    {
        if (CurrentWarden == null || !CurrentWarden.IsValid || !CurrentWarden.PawnIsAlive)
        {
            _rgbTimer?.Kill();
            _rgbTimer = null;
            return;
        }

        _hue += 10.0f;
        if (_hue >= 360.0f) _hue -= 360.0f;

        Color currentColor = ColorFromHSV(_hue, 1.0f, 1.0f);

        var pawn = CurrentWarden.PlayerPawn.Value;
        if (pawn != null && pawn.IsValid)
        {
            pawn.Render = currentColor;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
        }
    }

    public void RemoveWarden()
    {
        if (CurrentWarden != null && CurrentWarden.IsValid)
        {
            if (_wardenStartTime != null)
            {
                var duration = DateTime.Now - _wardenStartTime.Value;
                UpdateStats(CurrentWarden, duration.TotalSeconds);
            }

            AdminManager.RemovePlayerPermissions(CurrentWarden, "@jailbreak/warden");
            
            if (_originalImmunity.TryGetValue(CurrentWarden.SteamID, out uint immunity))
            {
                AdminManager.SetPlayerImmunity(CurrentWarden, immunity);
                _originalImmunity.Remove(CurrentWarden.SteamID);
            }
            else
            {
                AdminManager.SetPlayerImmunity(CurrentWarden, 0);
            }

            var pawn = CurrentWarden.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.Render = Color.White;
                Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
            }
        }
        CurrentWarden = null;
        _wardenStartTime = null;
        _wardenTimer?.Kill();
        _wardenTimer = null;
        _rgbTimer?.Kill();
        _rgbTimer = null;

        var admins = _wardenAdmins.ToList();
        foreach (var adminId in admins)
        {
            var adminPlayer = Utilities.GetPlayers().FirstOrDefault(p => p.SteamID == adminId);
            if (adminPlayer != null)
            {
                RemoveWardenAdmin(adminPlayer);
            }
            else
            {
                _wardenAdmins.Remove(adminId);
            }
        }
    }

    public void CommandKomMenu(CCSPlayerController? player, CommandInfo info)
    {
        LogHelper.LogTrace($"WardenService: CommandKomMenu called for {player?.PlayerName}.");
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        var menu = new CenterHtmlMenu("👑 Komutçu Kontrol Paneli", _plugin);

        menu.AddItem("[🔓] Kapıları Aç", (p, o) => _plugin.IseliService.QuickOpen(p));
        menu.AddItem("[🥊] Boks Modu", (p, o) => _plugin.GameManagerService.OpenBoxMenu(p));
        menu.AddItem("[🙈] Saklambaç (30s)", (p, o) => _plugin.GameManagerService.StartSaklambac(30));
        menu.AddItem("[⚔️] FF Menüsü", (p, o) => _plugin.FFMenuService.OpenWardenConfigMenu(p));
        menu.AddItem("[🛡️] Koruma Modu (God Mode)", (p, o) => CommandQ(p, info));
        menu.AddItem("[❌] Korumayı Kapat (!qq)", (p, o) => CommandQQ(p, info));
        menu.AddItem("[➕] Herkesi Canlandır", (p, o) => _plugin.UtilityService.CommandAf(p, info));
        menu.AddItem("[📈] İşaretleyici Ayarları", (p, o) => _plugin.MarkerService.OpenMarkerMenu(p, info));
        
        LogHelper.LogTrace($"WardenService: CommandKomMenu displaying menu for {player.PlayerName}.");
        menu.Display(player, 0);
    }

    public void CommandBecomeWarden(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid)
        {
            Server.PrintToConsole("[JailBreak] Bu komut sadece oyuncular tarafından kullanılabilir.");
            return;
        }

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (player.Team != CsTeam.CounterTerrorist)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyCTCanBeWarden));
            return;
        }

        if (CurrentWarden != null && CurrentWarden.IsValid)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgWardenExists, CurrentWarden.PlayerName)));
            return;
        }

        SetWarden(player);
    }

    public void CommandUnwarden(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid)
        {
            Server.PrintToConsole("[JailBreak] Bu komut sadece oyuncular tarafından kullanılabilir.");
            return;
        }

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!IsWarden(player))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNotWarden));
            return;
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgWardenLeft, player.PlayerName)));
        RemoveWarden();
    }

    public void CommandKomKalan(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (CurrentWarden == null || !CurrentWarden.IsValid || _wardenStartTime == null)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNoActiveWarden));
            return;
        }

        var elapsed = DateTime.Now - _wardenStartTime.Value;
        var total = TimeSpan.FromMinutes(_plugin.Config.WardenDurationMinutes);
        var remaining = total - elapsed;

        if (remaining.Ticks < 0) remaining = TimeSpan.Zero;

        string timeStr = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}";
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgWardenTimeRemaining, timeStr)));
    }

    public void CommandTopKomutcu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_wardenStats.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopWardenEmpty));
            return;
        }

        var sortedStats = _wardenStats.OrderByDescending(s => s.TotalTimeSeconds).ToList();
        var menu = new CenterHtmlMenu(_plugin.Lang.HudTitleTopWarden, _plugin);

        for (int i = 0; i < sortedStats.Count; i++)
        {
            var s = sortedStats[i];
            int minutes = (int)(s.TotalTimeSeconds / 60);
            menu.AddItem($"{i + 1}. {s.PlayerName} - {minutes} dk", (p, o) => { });
        }

        menu.Display(player, 0);
    }

    public void CommandQ(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player, "@css/generic")) return;

        foreach (var ct in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            _godModePlayers.Add(ct.SteamID);
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Green}CT takımına koruma (God Mode) verildi."));
    }

    public void CommandQQ(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player, "@css/generic")) return;

        _godModePlayers.Clear();

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Red}Koruma (God Mode) kaldırıldı."));
    }






    public void CommandWardenAdmin(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!HasPermission(player, "@css/cvar"))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyWardenAdminOrRootCanUse));
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
                player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
            }
            return;
        }

        var players = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.SteamID != player.SteamID && AdminManager.PlayerHasPermissions(p, "@css/generic")).ToList();

        if (players.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNoOtherAdmins));
            return;
        }

        var menu = new CenterHtmlMenu("Komutçu Admin Seçimi", _plugin);

        foreach (var p in players)
        {
            var targetPlayer = p;
            menu.AddItem(targetPlayer.PlayerName, (caller, option) =>
            {
                if (targetPlayer.IsValid) AddWardenAdmin(caller, targetPlayer);
            });
        }

        menu.Display(player, 0);
    }

    private void AddWardenAdmin(CCSPlayerController caller, CCSPlayerController target)
    {
        if (!AdminManager.PlayerHasPermissions(target, "@css/generic"))
        {
            caller.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerHasNoAdminPerms));
            return;
        }

        _wardenAdmins.Add(target.SteamID);
        _wardenAdminStartTimes[target.SteamID] = DateTime.Now;
        AdminManager.AddPlayerPermissions(target, "@jailbreak/ka");

        AdminManager.SetPlayerImmunity(target, 100);

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgWardenAdminSelected, target.PlayerName)));
    }

    public void CommandRemoveWardenAdmin(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!HasPermission(player, "@css/ban")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgKasilUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid && !p.IsBot && p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target != null)
        {
            if (_wardenAdmins.Contains(target.SteamID))
            {
                RemoveWardenAdmin(target);
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgWardenAdminRemoved, target.PlayerName)));
            }
            else
            {
                player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotWardenAdmin));
            }
        }
    }

    private void RemoveWardenAdmin(CCSPlayerController player)
    {
        if (_wardenAdminStartTimes.TryGetValue(player.SteamID, out DateTime startTime))
        {
            double seconds = (DateTime.Now - startTime).TotalSeconds;
            UpdateAdminStats(player, seconds);
            _wardenAdminStartTimes.Remove(player.SteamID);
        }

        _wardenAdmins.Remove(player.SteamID);
        AdminManager.RemovePlayerPermissions(player, "@jailbreak/ka");

        if (_originalImmunity.TryGetValue(player.SteamID, out uint immunity))
        {
            AdminManager.SetPlayerImmunity(player, immunity);
            _originalImmunity.Remove(player.SteamID);
        }
        else
        {
            AdminManager.SetPlayerImmunity(player, 0);
        }
    }

    public void CommandTopKa(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_wardenAdminStats.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopWardenAdminEmpty));
            return;
        }

        var sortedStats = _wardenAdminStats.OrderByDescending(s => s.TotalTimeSeconds).ToList();
        var menu = new CenterHtmlMenu(_plugin.Lang.HudTitleTopWardenAdmin, _plugin);

        for (int i = 0; i < sortedStats.Count; i++)
        {
            var s = sortedStats[i];
            int minutes = (int)(s.TotalTimeSeconds / 60);
            menu.AddItem($"{i + 1}. {s.PlayerName} - {minutes} dk", (p, o) => { });
        }

        menu.Display(player, 0);
    }

    private static Color ColorFromHSV(float hue, float saturation, float value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        float f = hue / 60 - (float)Math.Floor(hue / 60);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return Color.FromArgb(255, v, t, p);
        else if (hi == 1)
            return Color.FromArgb(255, q, v, p);
        else if (hi == 2)
            return Color.FromArgb(255, p, v, t);
        else if (hi == 3)
            return Color.FromArgb(255, p, q, v);
        else if (hi == 4)
            return Color.FromArgb(255, t, p, v);
        else
            return Color.FromArgb(255, v, p, q);
    }
}
