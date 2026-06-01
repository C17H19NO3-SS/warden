using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class PositionService : IPositionService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;
    private readonly IFreezeService _freezeService;
    private readonly Dictionary<ulong, Vector> _lastPingLocations = new();

    public PositionService(JailBreakPlugin plugin, IWardenService wardenService, IFreezeService freezeService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _freezeService = freezeService;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_daire", "T takımını daire diz", CommandDaire);
        _plugin.RegisterCommand("css_diz", "T takımını yan yana diz", CommandDiz);
    }

    public void OnPlayerPing(EventPlayerPing @event, CCSPlayerController player)
    {
        _lastPingLocations[player.SteamID] = new Vector(@event.X, @event.Y, @event.Z);
    }

    private Vector GetCenterPoint(CCSPlayerController player)
    {
        if (_lastPingLocations.TryGetValue(player.SteamID, out var loc))
        {
            return loc;
        }
        return new Vector(0, 0, 0);
    }

    public void CommandDaire(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_wardenService.HasPermission(player, "@css/kick")) return;

        Vector center = GetCenterPoint(player);
        if (center.X == 0 && center.Y == 0 && center.Z == 0) 
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPingLocationFirst));
            return;
        }

        string arg = info.GetArg(1);
        if (!float.TryParse(arg, out float radius))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgDaireUsage));
            return;
        }

        var tPlayers = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();
        if (tPlayers.Count == 0) return;

        float angleStep = (float)(2 * Math.PI / tPlayers.Count);
        for (int i = 0; i < tPlayers.Count; i++)
        {
            float angle = i * angleStep;
            float x = center.X + (radius * (float)Math.Cos(angle));
            float y = center.Y + (radius * (float)Math.Sin(angle));

            var pawn = tPlayers[i].PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.Teleport(new Vector(x, y, center.Z + 5.0f), pawn.AbsRotation, new Vector(0, 0, 0));
                pawn.MoveType = MoveType_t.MOVETYPE_NONE;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_NONE;
            }
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgDaireApplied, radius)));
        ApplyFormationFreeze();
    }

    public void CommandDiz(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_wardenService.HasPermission(player, "@css/kick")) return;

        Vector center = GetCenterPoint(player);
        if (center.X == 0 && center.Y == 0 && center.Z == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPingLocationFirst));
            return;
        }

        string arg = info.GetArg(1);
        if (!float.TryParse(arg, out float spacing))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgDizUsage));
            return;
        }

        var tPlayers = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();
        if (tPlayers.Count == 0) return;

        var pawnCaller = player.PlayerPawn.Value;
        if (pawnCaller == null || !pawnCaller.IsValid) return;

        QAngle? rotation = pawnCaller.AbsRotation;
        if (rotation == null) return;

        float yaw = rotation.Y;
        float dirX = (float)Math.Cos((yaw + 90) * Math.PI / 180);
        float dirY = (float)Math.Sin((yaw + 90) * Math.PI / 180);

        int count = tPlayers.Count;
        float startOffset = -((count - 1) * spacing) / 2;

        for (int i = 0; i < count; i++)
        {
            float offset = startOffset + (i * spacing);
            float x = center.X + (offset * dirX);
            float y = center.Y + (offset * dirY);

            var pawn = tPlayers[i].PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.Teleport(new Vector(x, y, center.Z + 5.0f), pawn.AbsRotation, new Vector(0, 0, 0));
                pawn.MoveType = MoveType_t.MOVETYPE_NONE;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_NONE;
            }
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgDizApplied, spacing)));
        ApplyFormationFreeze();
    }

    private void ApplyFormationFreeze()
    {
        _freezeService.FreezeAll(true);

        var timer = _plugin.AddTimer(0.1f, () =>
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud(_plugin.Lang.HudTitleFormation, _plugin.Lang.HudContentFormation));
            }
        }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

        _plugin.AddTimer(3.0f, () =>
        {
            timer.Kill();
        });
    }
}
