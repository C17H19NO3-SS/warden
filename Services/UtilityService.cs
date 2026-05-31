using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Linq;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class UtilityService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly Random _random = new();
    private readonly Dictionary<ulong, (string Name, int Value)> _kacCmRecords = new();
    private int _currentCTRevives;

    public UtilityService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _currentCTRevives = plugin.Config.MaxCTRevives;
    }

    public void OnRoundStart()
    {
        _currentCTRevives = _plugin.Config.MaxCTRevives;
    }

    public void CommandHpAll(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpAllSet));
    }

    public void CommandHpT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpTSet));
    }

    public void CommandHpCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpCTSet));
    }

    public void CommandGetT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetTApplied));
    }

    public void CommandGetCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetCTApplied));
    }

    public void CommandGetAll(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetAllApplied));
    }

    public void CommandAf(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        _currentCTRevives = _plugin.Config.MaxCTRevives;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.Team != CsTeam.Spectator && p.Team != CsTeam.None))
        {
            if (!p.PawnIsAlive)
            {
                p.Respawn();
            }

            if (p.PawnIsAlive)
            {
                p.Health = 100;
                Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
            }
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgAfApplied));
    }

    public void CommandGit(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/kick")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGitUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));
        if (target == null || !target.IsValid || !target.PawnIsAlive)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
            return;
        }

        player.PlayerPawn.Value?.Teleport(target.PlayerPawn.Value?.AbsOrigin, target.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgGitApplied, target.PlayerName)));
    }

    public void CommandHakSal(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.Team != CsTeam.CounterTerrorist || !_wardenService.HasPermission(player, "@css/ban")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHakSalUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase) && p.Team == CsTeam.Terrorist);

        if (target == null || !target.IsValid)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHakSalTargetNotFound));
            return;
        }

        // Hem oyuncu hem hedef valide edildi, şimdi değişim yapılıyor
        Server.NextFrame(() =>
        {
            if (player.IsValid && target.IsValid)
            {
                player.ChangeTeam(CsTeam.Terrorist);
                target.ChangeTeam(CsTeam.CounterTerrorist);
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgHakSalApplied, player.PlayerName, target.PlayerName)));
            }
        });
    }

    public void CommandBunnyOpen(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        Server.ExecuteCommand("sv_autobunnyhopping 1");
        Server.ExecuteCommand("sv_enablebunnyhopping 1");
        Server.ExecuteCommand("sv_airaccelerate 2000");
        Server.ExecuteCommand("sv_legacy_jump 1");
        Server.ExecuteCommand("sv_staminajumpcost 0");
        Server.ExecuteCommand("sv_staminalandcost 0");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgBunnyEnabled));
    }

    public void CommandBunnyClose(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        Server.ExecuteCommand("sv_autobunnyhopping 0");
        Server.ExecuteCommand("sv_enablebunnyhopping 0");
        Server.ExecuteCommand("sv_airaccelerate 12");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgBunnyDisabled));
    }

    public void CommandUnmuteCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist))
        {
            p.VoiceFlags = VoiceFlags.Normal;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgUnmuteCTApplied));
    }

    public void CommandUnmuteT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist))
        {
            p.VoiceFlags = VoiceFlags.Normal;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgUnmuteTApplied));
    }

    public void CommandSs(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.RemoveWeapons();
            p.GiveNamedItem("weapon_knife");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgSsApplied));
    }

    public void CommandKacCm(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        int cm = _random.Next(1, 41); // 1-40
        
        // Save highest value
        if (!_kacCmRecords.TryGetValue(player.SteamID, out var record) || cm > record.Value)
        {
            _kacCmRecords[player.SteamID] = (player.PlayerName, cm);
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgKacCmApplied, player.PlayerName, cm)));
    }

    public void CommandTopKacCm(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_kacCmRecords.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopKacCmEmpty));
            return;
        }

        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopKacCmTitle));

        var sortedRecords = _kacCmRecords.Values
            .OrderByDescending(r => r.Value)
            .Take(5)
            .ToList();

        for (int i = 0; i < sortedRecords.Count; i++)
        {
            var r = sortedRecords[i];
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgTopKacCmEntry, i + 1, r.Name, r.Value)));
        }
    }

    public void ResetKacCmRecords()
    {
        _kacCmRecords.Clear();
    }

    public void CommandMuteCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist))
        {
            p.VoiceFlags = VoiceFlags.Muted;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgMuteCTApplied));
    }

    public void CommandMuteT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist))
        {
            p.VoiceFlags = VoiceFlags.Muted;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgMuteTApplied));
    }

    public void CommandOtores(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        Server.ExecuteCommand("mp_respawn_on_death_t 1");
        Server.ExecuteCommand("mp_respawn_on_death_ct 1");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOtoresEnabled));
    }

    public void CommandOtores0(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/ban")) return;

        Server.ExecuteCommand("mp_respawn_on_death_t 0");
        Server.ExecuteCommand("mp_respawn_on_death_ct 0");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOtoresDisabled));
    }
}
