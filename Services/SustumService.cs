using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;

namespace JailBreak.Services;

public enum SustumMode
{
    None,
    Dsustum,
    Tsustum,
    Olusustum
}

public class SustumService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    private SustumMode _activeMode = SustumMode.None;
    private string _targetWord = "";
    private int _remainingTime = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _sustumTimer;
    private readonly Random _random = new();

    public SustumService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void CommandDsustum(CCSPlayerController? player, CommandInfo info) => StartSustum(player, SustumMode.Dsustum);
    public void CommandTsustum(CCSPlayerController? player, CommandInfo info) => StartSustum(player, SustumMode.Tsustum);
    public void CommandOlusustum(CCSPlayerController? player, CommandInfo info) => StartSustum(player, SustumMode.Olusustum);

    private void StartSustum(CCSPlayerController? player, SustumMode mode)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        bool isWarden = AdminManager.PlayerHasPermissions(player, "@jailbreak/warden");
        bool isWardenAdmin = AdminManager.PlayerHasPermissions(player, "@jailbreak/ka");
        bool hasChat = AdminManager.PlayerHasPermissions(player, "@css/chat");
        bool isRoot = AdminManager.PlayerHasPermissions(player, "@css/root");

        if (!isWarden && !isWardenAdmin && !hasChat && !isRoot)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgNoPermission)}");
            return;
        }

        _sustumTimer?.Kill();

        _activeMode = mode;
        string baseWord = _plugin.Config.SustumWords[_random.Next(_plugin.Config.SustumWords.Count)];
        int number = _random.Next(100, 999);
        _targetWord = $"{baseWord}{number}";
        _remainingTime = _plugin.Config.SustumDuration;

        string modeName = mode.ToString().ToUpper();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgSustumStarted), modeName, _targetWord))}");

        _sustumTimer = _plugin.AddTimer(1.0f, () =>
        {
            if (_activeMode == SustumMode.None)
            {
                _sustumTimer?.Kill();
                _sustumTimer = null;
                return;
            }

            if (_remainingTime <= 0)
            {
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgSustumExpired), modeName))}");
                _activeMode = SustumMode.None;
                _targetWord = "";
                _sustumTimer?.Kill();
                _sustumTimer = null;
                return;
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                string content = string.Format(_plugin.Lang.HudContentSustum, _targetWord, _remainingTime);
                p.PrintToCenterHtml(HudHelper.FormatHud(_plugin.Lang.HudTitleSustum, content));
            }

            _remainingTime--;
        }, TimerFlags.REPEAT);
    }

    public bool HandleSustumChat(CCSPlayerController player, string message)
    {
        if (_activeMode == SustumMode.None) return false;
        if (message.Trim() != _targetWord) return false;

        bool canWin = false;
        string rewardName = "";
        string modeName = _activeMode.ToString().ToUpper();

        switch (_activeMode)
        {
            case SustumMode.Dsustum:
                if (player.Team == CsTeam.Terrorist && player.PawnIsAlive)
                {
                    canWin = true;
                    rewardName = _plugin.Lang.MsgSustumRewardDeagle;
                    GiveDeagle(player);
                }
                break;
            case SustumMode.Tsustum:
                if (player.Team == CsTeam.Terrorist && player.PawnIsAlive)
                {
                    canWin = true;
                    rewardName = _plugin.Lang.MsgSustumRewardCT;
                    player.ChangeTeam(CsTeam.CounterTerrorist);
                }
                break;
            case SustumMode.Olusustum:
                if (player.Team == CsTeam.Terrorist && !player.PawnIsAlive)
                {
                    canWin = true;
                    rewardName = _plugin.Lang.MsgSustumRewardRespawn;
                    player.Respawn();
                }
                break;
        }

        if (canWin)
        {
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgSustumWinner), modeName, player.PlayerName, rewardName))}");
            _activeMode = SustumMode.None;
            _targetWord = "";
            _sustumTimer?.Kill();
            _sustumTimer = null;
            return true;
        }

        return false;
    }

    private void GiveDeagle(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        player.GiveNamedItem(_plugin.Config.SustumRewardDeagleWeapon);
    }
}
