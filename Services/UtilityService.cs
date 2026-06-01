using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using System.Linq;
using JailBreak.Helpers;
using CS2MenuManager.API.Menu;
using System.Drawing;

namespace JailBreak.Services;

/// <summary>
/// Provides a variety of utility commands and functionalities for the Warden.
/// </summary>
public class UtilityService : IUtilityService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;
    private readonly IHudService _hudService;
    private readonly Dictionary<ulong, int> _kacCmRecords = new();
    private bool _otoResEnabled = false;

    public UtilityService(JailBreakPlugin plugin, IWardenService wardenService, IHudService hudService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _hudService = hudService;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_hp", "Herkesin canını 100 yap", CommandHp);
        _plugin.RegisterCommand("css_hpt", "T takımının canını 100 yap", CommandHpT);
        _plugin.RegisterCommand("css_hpct", "CT takımının canını 100 yap", CommandHpCT);
        _plugin.RegisterCommand("css_gett", "T takımını yanına çek", CommandGetT);
        _plugin.RegisterCommand("css_getct", "CT takımını yanına çek", CommandGetCT);
        _plugin.RegisterCommand("css_getall", "Tüm oyuncuları yanına çek", CommandGetAll);
        _plugin.RegisterCommand("css_git", "Bir oyuncunun yanına ışınlan", CommandGit);
        _plugin.RegisterCommand("css_haksal", "LR hakkını başka birine sal", CommandHakSal);
        _plugin.RegisterCommand("css_af", "Herkesi canlandır ve canını 100 yap", CommandAf);
        _plugin.RegisterCommand("css_ctrev", "Ölü bir CT'yi canlandır", CommandCTRev);
        _plugin.RegisterCommand("css_bunny", "Bunnyhop'u aç/kapat", CommandBunny);
        _plugin.RegisterCommand("css_unmute", "Takımın mutesini aç", CommandUnmute);
        _plugin.RegisterCommand("css_ss", "T takımının silahlarını al", CommandSs);
        _plugin.RegisterCommand("css_kaccm", "Kaç cm ölç", CommandKacCm);
        _plugin.RegisterCommand("css_topkaccm", "En yüksek kaç cm listesi", CommandTopKacCm);
        _plugin.RegisterCommand("css_mute", "Takımı muteler", CommandMute);
        _plugin.RegisterCommand("css_otores", "Otomatik canlanmayı aç/kapat", CommandOtores);
        _plugin.RegisterCommand("css_göm", "Kendini öldür", CommandSelfKill);
        _plugin.RegisterCommand("css_gom", "Kendini öldür", CommandSelfKill);
    }

    // OnRoundStart removed: no per-round initialization required for UtilityService.

    public void ResetKacCmRecords()
    {
        _kacCmRecords.Clear();
    }

    public void OnRoundStart()
    {
        // No per-round initialization required at the moment.
        ResetKacCmRecords();
    }

    public void CommandHp(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            p.PlayerPawn.Value!.Health = 100;
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpAllSet));
    }

    public void CommandHpT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value!.Health = 100;
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpTSet));
    }

    public void CommandHpCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value!.Health = 100;
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpCTSet));
    }

    public void CommandGetT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        var pawnCaller = player.PlayerPawn.Value;
        if (pawnCaller == null || !pawnCaller.IsValid) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value!.Teleport(pawnCaller.AbsOrigin, pawnCaller.AbsRotation, new Vector(0, 0, 0));
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetTApplied));
    }

    public void CommandGetCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        var pawnCaller = player.PlayerPawn.Value;
        if (pawnCaller == null || !pawnCaller.IsValid) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value!.Teleport(pawnCaller.AbsOrigin, pawnCaller.AbsRotation, new Vector(0, 0, 0));
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetCTApplied));
    }

    public void CommandGetAll(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        var pawnCaller = player.PlayerPawn.Value;
        if (pawnCaller == null || !pawnCaller.IsValid) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive && p.SteamID != player.SteamID))
        {
            p.PlayerPawn.Value!.Teleport(pawnCaller.AbsOrigin, pawnCaller.AbsRotation, new Vector(0, 0, 0));
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetAllApplied));
    }

    public void CommandGit(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGitUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.IsValid && p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target == null || target.PlayerPawn.Value == null)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
            return;
        }

        player.PlayerPawn.Value!.Teleport(target.PlayerPawn.Value.AbsOrigin, target.PlayerPawn.Value.AbsRotation, new Vector(0, 0, 0));
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgGitApplied, target.PlayerName)));
    }

    public void CommandHakSal(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.Team != CsTeam.Terrorist || !player.PawnIsAlive) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHakSalUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => 
            p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive && p.SteamID != player.SteamID &&
            p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHakSalTargetNotFound));
            return;
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgHakSalApplied, player.PlayerName, target.PlayerName)));
        player.PlayerPawn.Value?.CommitSuicide(false, true);
    }

    public void CommandAf(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid))
        {
            if (!p.PawnIsAlive) p.Respawn();
            Server.NextFrame(() => {
                if (p.IsValid && p.PlayerPawn.Value != null) p.PlayerPawn.Value.Health = 100;
            });
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgAfApplied));
    }

    private int _ctReviveCount = 0;
    public void CommandCTRev(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        if (_ctReviveCount >= _plugin.Config.MaxCTRevives)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCTRevNoRevivesLeft));
            return;
        }

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCTRevUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => 
            p.IsValid && p.Team == CsTeam.CounterTerrorist && !p.PawnIsAlive &&
            p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCTRevTargetMustBeDeadCT));
            return;
        }

        target.Respawn();
        _ctReviveCount++;
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgCTRevApplied, target.PlayerName, _plugin.Config.MaxCTRevives - _ctReviveCount)));
    }

    public void CommandBunny(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/cvar")) return;

        var bunnyCvar = ConVar.Find("sv_autobunnyhopping");
        if (bunnyCvar == null) return;

        bool isEnabled = bunnyCvar.GetPrimitiveValue<bool>();
        if (isEnabled)
        {
            Server.ExecuteCommand("sv_autobunnyhopping 0");
            Server.ExecuteCommand("sv_enablebunnyhopping 0");
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgBunnyDisabled));
        }
        else
        {
            Server.ExecuteCommand("sv_autobunnyhopping 1");
            Server.ExecuteCommand("sv_enablebunnyhopping 1");
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgBunnyEnabled));
        }
    }

    public void CommandUnmute(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/mod")) return;

        string target = info.GetArg(1).ToLower();
        if (target == "t")
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist)) p.VoiceFlags = VoiceFlags.Normal;
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgUnmuteTApplied));
        }
        else if (target == "ct")
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist)) p.VoiceFlags = VoiceFlags.Normal;
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgUnmuteCTApplied));
        }
    }

    public void CommandSs(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

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

        if (_kacCmRecords.TryGetValue(player.SteamID, out int existingValue))
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgKacCmApplied, player.PlayerName, existingValue)));
            return;
        }

        int value = new Random().Next(1, 45);
        _kacCmRecords[player.SteamID] = value;
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgKacCmApplied, player.PlayerName, value)));
    }

    public void CommandTopKacCm(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_kacCmRecords.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopKacCmEmpty));
            return;
        }

        var sorted = _kacCmRecords.OrderByDescending(x => x.Value).Take(10).ToList();
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopKacCmTitle));

        int i = 1;
        foreach (var record in sorted)
        {
            var p = Utilities.GetPlayerFromSteamId(record.Key);
            string name = p?.PlayerName ?? "Unknown";
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgTopKacCmEntry, i++, name, record.Value)));
        }
    }

    public void CommandMute(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/mod")) return;

        string target = info.GetArg(1).ToLower();
        if (target == "t")
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist)) p.VoiceFlags = VoiceFlags.Muted;
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgMuteTApplied));
        }
        else if (target == "ct")
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist)) p.VoiceFlags = VoiceFlags.Muted;
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgMuteCTApplied));
        }
    }

    public void CommandOtores(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/cvar")) return;

        _otoResEnabled = !_otoResEnabled;
        string msg = _otoResEnabled ? _plugin.Lang.MsgOtoresEnabled : _plugin.Lang.MsgOtoresDisabled;
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, msg));
    }

    public void CommandSelfKill(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive) return;

        player.PlayerPawn.Value?.CommitSuicide(false, true);
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgSelfKillApplied, player.PlayerName)));
    }
}
