using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class FreezeService
{
    private readonly JailBreakPlugin _plugin;
    private bool _isFrozen = false;
    private int _countdownTime = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _countdownTimer;

    public FreezeService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
    }

    public void OnRoundStart()
    {
        UnfreezeAll(true);
        _countdownTimer?.Kill();
        _countdownTimer = null;
        _countdownTime = 0;
    }

    public void CommandFreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_plugin.WardenService.HasPermission(player)) return;
        FreezeAll();
    }

    public void CommandUnfreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_plugin.WardenService.HasPermission(player)) return;
        UnfreezeAll();
    }

    public void CommandDelayedFreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_plugin.WardenService.HasPermission(player)) return;

        string arg = info.GetArg(1);
        if (int.TryParse(arg, out int time))
        {
            StartCountdown(time);
        }
    }

    public void CommandResetFreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_plugin.WardenService.HasPermission(player)) return;

        _countdownTimer?.Kill();
        _countdownTimer = null;
        _countdownTime = 0;
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTFreezeCountdownCancelled));
    }

    private void StartCountdown(int time)
    {
        _countdownTimer?.Kill();
        _countdownTime = time;

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgTFreezeCountdownStarted, _countdownTime)));

        _countdownTimer = _plugin.AddTimer(1.0f, () =>
        {
            if (_countdownTime <= 0)
            {
                FreezeAll();
                _countdownTimer?.Kill();
                _countdownTimer = null;
                return;
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                string content = string.Format(_plugin.Lang.HudContentFreeze, _countdownTime);
                p.PrintToCenterHtml(PluginHelper.FormatHud(_plugin.Lang.HudTitleFreeze, content));
            }

            _countdownTime--;
        }, TimerFlags.REPEAT);
    }

    public void FreezeAll(bool silent = false)
    {
        _isFrozen = true;
        if (!silent)
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTFreezeStarted));
        }

        ApplyFreezeState();
    }

    public void UnfreezeAll(bool silent = false)
    {
        if (!_isFrozen && !silent) return;

        _isFrozen = false;
        if (!silent)
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTFreezeEnded));
        }

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist))
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.MoveType = MoveType_t.MOVETYPE_WALK;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            }
        }
    }

    public void ApplyFreezeState()
    {
        if (!_isFrozen) return;

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.Velocity.X = 0;
                pawn.Velocity.Y = 0;
                pawn.Velocity.Z = 0;

                pawn.MoveType = MoveType_t.MOVETYPE_NONE;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_NONE;
            }
        }
    }
}
