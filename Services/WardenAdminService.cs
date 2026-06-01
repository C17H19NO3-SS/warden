using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils; // Added for Server access
using JailBreak.Helpers;
using System.Collections.Generic;
using System.Linq;
using CenterHtmlMenu = CS2MenuManager.API.Menu.CenterHtmlMenu;

namespace JailBreak.Services;

public class WardenAdminService : IWardenAdminService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenStatService _statService;
    private readonly HashSet<ulong> _wardenAdmins = new();
    private readonly Dictionary<ulong, DateTime> _wardenAdminStartTimes = new();
    private readonly Dictionary<ulong, uint> _originalImmunity = new();

    public WardenAdminService(JailBreakPlugin plugin, IWardenStatService statService)
    {
        _plugin = plugin;
        _statService = statService;
    }

    public bool IsWardenAdmin(CCSPlayerController player)
    {
        return _wardenAdmins.Contains(player.SteamID);
    }

    public void AddWardenAdmin(CCSPlayerController caller, CCSPlayerController target)
    {
        if (!AdminManager.PlayerHasPermissions(target, "@css/generic"))
        {
            caller.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerHasNoAdminPerms));
            return;
        }

        _wardenAdmins.Add(target.SteamID);
        _wardenAdminStartTimes[target.SteamID] = DateTime.Now;
        _originalImmunity[target.SteamID] = (uint)target.Score;
        AdminManager.AddPlayerPermissions(target, "@jailbreak/ka");
        AdminManager.SetPlayerImmunity(target, 100);

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgWardenAdminSelected, target.PlayerName)));
    }

    public void RemoveWardenAdmin(CCSPlayerController player)
    {
        if (_wardenAdminStartTimes.TryGetValue(player.SteamID, out DateTime startTime))
        {
            double seconds = (DateTime.Now - startTime).TotalSeconds;
            ulong steamId = player.SteamID;
            string playerName = player.PlayerName;
            _ = Task.Run(async () => await _statService.UpdateWardenAdminStats(steamId, playerName, seconds));
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

    public void RemoveAllWardenAdmins()
    {
        var adminIds = _wardenAdmins.ToList();
        foreach (var adminId in adminIds)
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

    private bool HasPermission(CCSPlayerController player, string requiredFlag)
    {
        if (AdminManager.PlayerHasPermissions(player, "@css/root")) return true;
        if (AdminManager.PlayerHasPermissions(player, requiredFlag)) return true;
        
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNoPermission));
        return false;
    }

    public void CommandWardenAdmin(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!HasPermission(player, "@css/cvar")) return;

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

    public void CommandTopKa(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        var stats = _statService.GetTopWardenAdminStats();
        if (stats.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopWardenAdminEmpty));
            return;
        }

        var menu = new CenterHtmlMenu(_plugin.Lang.HudTitleTopWardenAdmin, _plugin);

        for (int i = 0; i < stats.Count; i++)
        {
            var s = stats[i];
            int minutes = (int)(s.TotalTimeSeconds / 60);
            menu.AddItem($"{i + 1}. {s.PlayerName} - {minutes} dk", (p, o) => { });
        }

        menu.Display(player, 0);
    }
}
