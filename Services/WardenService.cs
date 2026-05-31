using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Models;
using System.Drawing;
using System.Text.Json;
using CS2MenuManager.API.Menu;
using CenterHtmlMenu = CS2MenuManager.API.Menu.CenterHtmlMenu;

namespace JailBreak.Services;

public class WardenService
{
    private readonly JailBreakPlugin _plugin;

    public CCSPlayerController? CurrentWarden { get; private set; }
    private DateTime? _wardenStartTime;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _wardenTimer;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _rgbTimer;
    private readonly HashSet<ulong> _wardenAdmins = new();
    private float _hue = 0;
    private List<WardenStat> _wardenStats = new();
    private readonly string _statsPath;

    public WardenService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
        _statsPath = Path.Combine(_plugin.ModuleDirectory, "../../configs/plugins/JailBreak/WardenStats.json");
        LoadStats();
    }

    private void LoadStats()
    {
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
            Console.WriteLine($"[JailBreak] Error loading warden stats: {ex.Message}");
        }
    }

    private void SaveStats()
    {
        try
        {
            string dir = Path.GetDirectoryName(_statsPath) ?? "";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(_wardenStats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_statsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JailBreak] Error saving warden stats: {ex.Message}");
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
            stat.PlayerName = player.PlayerName; // Update name in case it changed
        }

        stat.TotalTimeSeconds += seconds;
        SaveStats();
    }

    public bool IsWarden(CCSPlayerController player)
    {
        return CurrentWarden != null && CurrentWarden.IsValid && CurrentWarden.SteamID == player.SteamID;
    }

    public bool IsWardenAdmin(CCSPlayerController player)
    {
        return _wardenAdmins.Contains(player.SteamID);
    }

    public void OnRoundStart()
    {
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

        _wardenTimer?.Kill();
        _wardenTimer = _plugin.AddTimer(_plugin.Config.WardenDurationMinutes * 60f, () =>
        {
            if (CurrentWarden != null && CurrentWarden.IsValid)
            {
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgWardenDurationExpired)}");
                _plugin.VoteService.StartKickVotePhase();
            }
        });

        // Warden RGB Timer
        _rgbTimer?.Kill();
        _hue = 0;
        _rgbTimer = _plugin.AddTimer(0.1f, UpdateWardenRgb, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgNewWarden), player.PlayerName))}");
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

        // Komutçu gidince ka yetkileri de gider
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

    public void CommandBecomeWarden(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        if (player.Team != CsTeam.CounterTerrorist)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyCTCanBeWarden)}");
            return;
        }

        if (CurrentWarden != null && CurrentWarden.IsValid)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgWardenExists), CurrentWarden.PlayerName))}");
            return;
        }

        SetWarden(player);
    }

    public void CommandUnwarden(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        if (!IsWarden(player))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgNotWarden)}");
            return;
        }

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgWardenLeft), player.PlayerName))}");
        RemoveWarden();
    }

    public void CommandKomKalan(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (CurrentWarden == null || !CurrentWarden.IsValid || _wardenStartTime == null)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgNoActiveWarden)}");
            return;
        }

        var elapsed = DateTime.Now - _wardenStartTime.Value;
        var total = TimeSpan.FromMinutes(_plugin.Config.WardenDurationMinutes);
        var remaining = total - elapsed;

        if (remaining.Ticks < 0) remaining = TimeSpan.Zero;

        string timeStr = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}";
        player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgWardenTimeRemaining), timeStr))}");
    }

    public void CommandTopKomutcu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_wardenStats.Count == 0)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgTopWardenEmpty)}");
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

    private void AddWardenAdmin(CCSPlayerController caller, CCSPlayerController target)
    {
        if (!AdminManager.PlayerHasPermissions(target, "@css/generic"))
        {
            caller.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgPlayerHasNoAdminPerms)}");
            return;
        }

        _wardenAdmins.Add(target.SteamID);
        AdminManager.AddPlayerPermissions(target, "@jailbreak/ka");

        // Immunity set to 99
        AdminManager.SetPlayerImmunity(target, 99);

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgWardenAdminSelected), target.PlayerName))}");
    }

    public void CommandRemoveWardenAdmin(CCSPlayerController? player, CommandInfo info)
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

        if (!isWarden && !isRoot && !isWardenAdmin) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgKasilUsage)}");
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid && !p.IsBot && p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target != null)
        {
            if (_wardenAdmins.Contains(target.SteamID))
            {
                RemoveWardenAdmin(target);
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgWardenAdminRemoved), target.PlayerName))}");
            }
            else
            {
                player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgPlayerNotWardenAdmin)}");
            }
        }
    }

    private void RemoveWardenAdmin(CCSPlayerController player)
    {
        _wardenAdmins.Remove(player.SteamID);
        AdminManager.RemovePlayerPermissions(player, "@jailbreak/ka");

        // Reset immunity if needed (standard usually 0 or per group)
        AdminManager.SetPlayerImmunity(player, 0);
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
