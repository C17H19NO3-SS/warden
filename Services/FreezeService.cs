using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;

namespace JailBreak.Services;

public static class HudHelper
{
    public const string ColorTitle = "gold";
    public const string ColorTime = "red";
    public const string ColorSuccess = "green";
    public const string ColorInstruction = "#C0C0C0";
    public const string ColorSystem = "cyan";

    public static string FormatHud(string title, string content, string instruction = "")
    {
        string hud = $"<font color='{ColorTitle}' size='20'><b>--- {title.ToUpper()} ---</b></font><br>{content}";
        if (!string.IsNullOrEmpty(instruction))
        {
            hud += $"<br><font color='{ColorInstruction}' size='14'>{instruction}</font>";
        }
        return hud;
    }
}

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
        if (player == null || !player.IsValid || !HasPermission(player)) return;
        FreezeAll();
    }

    public void CommandUnfreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;
        UnfreezeAll();
    }

    public void CommandDelayedFreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        string arg = info.GetArg(1);
        if (int.TryParse(arg, out int time))
        {
            StartCountdown(time);
        }
    }

    public void CommandResetFreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        _countdownTimer?.Kill();
        _countdownTimer = null;
        _countdownTime = 0;
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgTFreezeCountdownCancelled)}");
    }

    private void StartCountdown(int time)
    {
        _countdownTimer?.Kill();
        _countdownTime = time;

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgTFreezeCountdownStarted, _countdownTime))}");

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
                string content = $"Donmaya kalan süre: <font color='{HudHelper.ColorTime}'><b>{_countdownTime}s</b></font>";
                p.PrintToCenterHtml(HudHelper.FormatHud("T DONDURULUYOR", content));
            }

            _countdownTime--;
        }, TimerFlags.REPEAT);
    }

    public void FreezeAll(bool silent = false)
    {
        _isFrozen = true;
        if (!silent)
        {
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgTFreezeStarted)}");
        }

        ApplyFreezeState();
    }

    public void UnfreezeAll(bool silent = false)
    {
        if (!_isFrozen && !silent) return;

        _isFrozen = false;
        if (!silent)
        {
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgTFreezeEnded)}");
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

    private bool HasPermission(CCSPlayerController player)
    {
        return AdminManager.PlayerHasPermissions(player, "@jailbreak/warden") ||
               AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") ||
               AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
