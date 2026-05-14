using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;

namespace JailBreak.Services;

public class PositionService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly FreezeService _freezeService;
    private readonly Dictionary<ulong, Vector> _lastPingLocations = new();

    public PositionService(JailBreakPlugin plugin, WardenService wardenService, FreezeService freezeService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _freezeService = freezeService;
    }

    public void OnPlayerPing(EventPlayerPing @event, CCSPlayerController player)
    {
        _lastPingLocations[player.SteamID] = new Vector(@event.X, @event.Y, @event.Z);
    }

    private Vector GetCenterPoint(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return new Vector(0, 0, 0);

        Vector eyePos = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 64.0f);
        QAngle eyeAngles = pawn.EyeAngles!;

        float pitch = (float)(eyeAngles.X * Math.PI / 180.0);
        float yaw = (float)(eyeAngles.Y * Math.PI / 180.0);

        if (eyeAngles.X > 5.0f)
        {
            float height = 64.0f;
            float distance = height / (float)Math.Tan(pitch);
            if (distance > 1000.0f) distance = 1000.0f;

            return new Vector(
                eyePos.X + (float)Math.Cos(yaw) * distance,
                eyePos.Y + (float)Math.Sin(yaw) * distance,
                pawn.AbsOrigin.Z
            );
        }

        return new Vector(
            eyePos.X + (float)Math.Cos(yaw) * 200.0f,
            eyePos.Y + (float)Math.Sin(yaw) * 200.0f,
            pawn.AbsOrigin.Z
        );
    }

    public void CommandDaire(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        Vector center = GetCenterPoint(player);
        if (center.X == 0 && center.Y == 0) return;

        string arg = info.GetArg(1);
        if (!float.TryParse(arg, out float radius))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgDaireUsage)}");
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
            }
        }

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgDaireApplied, radius))}");
        ApplyFormationFreeze();
    }

    public void CommandDiz(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        Vector center = GetCenterPoint(player);
        if (center.X == 0 && center.Y == 0) return;

        string arg = info.GetArg(1);
        if (!float.TryParse(arg, out float spacing))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgDizUsage)}");
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
            }
        }

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgDizApplied, spacing))}");
        ApplyFormationFreeze();
    }

    private void ApplyFormationFreeze()
    {
        _freezeService.FreezeAll(true);

        var timer = _plugin.AddTimer(0.1f, () =>
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(HudHelper.FormatHud("T TAKIMI DONDURULDU", "Formasyon tamamlandı, T'ler donduruldu!"));
            }
        }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

        _plugin.AddTimer(3.0f, () =>
        {
            timer.Kill();
        });
    }

    private bool HasPermission(CCSPlayerController player)
    {
        return AdminManager.PlayerHasPermissions(player, "@jailbreak/warden") ||
               AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") ||
               AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
