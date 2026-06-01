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

public class WardenService : IWardenService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenStatService _statService;
    private readonly IWardenAdminService _adminService;
    private readonly IHudManager _hudManager;
    private readonly ILangProvider _langProvider;

    public CCSPlayerController? CurrentWarden { get; private set; }
    private DateTime? _wardenStartTime;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _wardenTimer;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _rgbTimer;
    private readonly HashSet<ulong> _godModePlayers = new();
    private readonly Dictionary<ulong, uint> _originalImmunity = new();
    private float _hue = 0;

    public WardenService(JailBreakPlugin plugin, IWardenStatService statService, IWardenAdminService adminService, IHudManager hudManager, ILangProvider langProvider)
    {
        _plugin = plugin;
        _statService = statService;
        _adminService = adminService;
        _hudManager = hudManager;
        _langProvider = langProvider;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_w", "Komutçu ol", CommandBecomeWarden);
        _plugin.RegisterCommand("css_uw", "Komutçuluktan çık", CommandUnwarden);
        _plugin.RegisterCommand("css_komkalan", "Komutçunun kalan süresini gör", CommandKomKalan);
        _plugin.RegisterCommand("css_topkomutcu", "En çok komutçu olanları gör", CommandTopKomutcu);
        _plugin.RegisterCommand("css_q", "Komutçu koruması (God, MuteT)", CommandQ);
        _plugin.RegisterCommand("css_qq", "Komutçu korumasını kaldır", CommandQQ);
        _plugin.RegisterCommand("css_ka", "Komutçu admin menüsü", _adminService.CommandWardenAdmin);
        _plugin.RegisterCommand("css_kasil", "Komutçu adminini kaldır", _adminService.CommandRemoveWardenAdmin);
        _plugin.RegisterCommand("css_topka", "En çok komutçu admin olanları gör", _adminService.CommandTopKa);
        _plugin.RegisterCommand("css_k", "Komutçu ana menüsünü açar", CommandKomMenu);
        _plugin.RegisterCommand("css_kommenu", "Komutçu ana menüsünü açar", CommandKomMenu);
    }

    public async Task LoadStats() => await _statService.LoadStats();

    public bool IsWarden(CCSPlayerController player)
    {
        return CurrentWarden != null && CurrentWarden.IsValid && CurrentWarden.SteamID == player.SteamID;
    }

    public bool IsWardenAdmin(CCSPlayerController player)
    {
        return _adminService.IsWardenAdmin(player);
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
        
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgNoPermission")));
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

        if (_adminService.IsWardenAdmin(player))
        {
            _adminService.RemoveWardenAdmin(player);
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
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgWardenDurationExpired")));
                _plugin.VoteService.StartKickVotePhase();
            }
        });

        _rgbTimer?.Kill();
        _hue = 0;
        _rgbTimer = _plugin.AddTimer(0.1f, UpdateWardenRgb, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgNewWarden", player.PlayerName)));
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
                ulong steamId = CurrentWarden.SteamID;
                string playerName = CurrentWarden.PlayerName;
                _ = Task.Run(async () => await _statService.UpdateWardenStats(steamId, playerName, duration.TotalSeconds));
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

        _adminService.RemoveAllWardenAdmins();
    }

    public void CommandKomMenu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;
        OpenMainMenu(player);
    }

    public void OpenMainMenu(CCSPlayerController player)
    {
        LogHelper.LogTrace($"WardenService: OpenMainMenu called for {player.PlayerName}.");
        var menu = new CenterHtmlMenu("👑 Komutçu Kontrol Paneli", _plugin);

        menu.AddItem("[🔓] Kapıları Aç", (p, o) => _plugin.IseliService.QuickOpen(p));
        menu.AddItem("[🥊] Boks Modu", (p, o) => _plugin.GameManagerService.OpenBoxMenu(p));
        menu.AddItem("[🙈] Saklambaç (30s)", (p, o) => _plugin.GameManagerService.StartSaklambac(30));
        menu.AddItem("[⚔️] FF Menüsü", (p, o) => _plugin.FFMenuService.OpenWardenConfigMenu(p));
        menu.AddItem("[🛡️] Koruma Modu (God Mode)", (p, o) => CommandQ(p, null));
        menu.AddItem("[❌] Korumayı Kapat (!qq)", (p, o) => CommandQQ(p, null));
        menu.AddItem("[➕] Herkesi Canlandır", (p, o) => _plugin.UtilityService.CommandAf(p, null));
        menu.AddItem("[📈] İşaretleyici Ayarları", (p, o) => _plugin.MarkerService.OpenMarkerMenu(p));

        LogHelper.LogTrace($"WardenService: OpenMainMenu displaying menu for {player.PlayerName}.");
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
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgOnlyJailbreakMap")));
            return;
        }

        if (player.Team != CsTeam.CounterTerrorist)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgOnlyCTCanBeWarden")));
            return;
        }

        if (CurrentWarden != null && CurrentWarden.IsValid)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgWardenExists", CurrentWarden.PlayerName)));
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
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgOnlyJailbreakMap")));
            return;
        }

        if (!IsWarden(player))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgNotWarden")));
            return;
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgWardenLeft", player.PlayerName)));
        RemoveWarden();
    }

    public void CommandKomKalan(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (CurrentWarden == null || !CurrentWarden.IsValid || _wardenStartTime == null)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgNoActiveWarden")));
            return;
        }

        var elapsed = DateTime.Now - _wardenStartTime.Value;
        var total = TimeSpan.FromMinutes(_plugin.Config.WardenDurationMinutes);
        var remaining = total - elapsed;

        if (remaining.Ticks < 0) remaining = TimeSpan.Zero;

        string timeStr = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}";
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgWardenTimeRemaining", timeStr)));
    }

    public void CommandTopKomutcu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        var stats = _statService.GetTopWardenStats();
        if (stats.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _langProvider.GetMessage("MsgTopWardenEmpty")));
            return;
        }

        var sortedStats = stats.OrderByDescending(s => s.TotalTimeSeconds).ToList();
        var menu = new CenterHtmlMenu(_langProvider.GetMessage("HudTitleTopWarden"), _plugin);

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
