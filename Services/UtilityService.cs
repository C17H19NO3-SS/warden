using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Linq;

namespace JailBreak.Services;

public class UtilityService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    public UtilityService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void CommandHpAll(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgHpAllSet)}");
    }

    public void CommandHpT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgHpTSet)}");
    }

    public void CommandHpCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgHpCTSet)}");
    }

    public void CommandGetT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgGetTApplied)}");
    }

    public void CommandGit(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgGitUsage)}");
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));
        if (target == null || !target.IsValid || !target.PawnIsAlive)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgPlayerNotFound)}");
            return;
        }

        player.PlayerPawn.Value?.Teleport(target.PlayerPawn.Value?.AbsOrigin, target.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgGitApplied, target.PlayerName))}");
    }

    public void CommandHakSal(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.Team != CsTeam.CounterTerrorist) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgHakSalUsage)}");
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase) && p.Team == CsTeam.Terrorist);
        
        if (target == null || !target.IsValid)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgHakSalTargetNotFound)}");
            return;
        }

        // Hem oyuncu hem hedef valide edildi, şimdi değişim yapılıyor
        Server.NextFrame(() => 
        {
            if (player.IsValid && target.IsValid)
            {
                player.ChangeTeam(CsTeam.Terrorist);
                target.ChangeTeam(CsTeam.CounterTerrorist);
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgHakSalApplied, player.PlayerName, target.PlayerName))}");
            }
        });
    }

    private bool HasPermission(CCSPlayerController player)
    {
        return _wardenService.IsWarden(player) || 
               AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") || 
               AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
