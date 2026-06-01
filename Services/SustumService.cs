using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using JailBreak.Helpers;

namespace JailBreak.Services;

public enum SustumMode
{
    None,
    Dsustum,
    Tsustum,
    Olusustum
}

public class SustumService : ISustumService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;

    private SustumMode _activeMode = SustumMode.None;
    private string _targetWord = "";
    private int _remainingTime = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _sustumTimer;
    private readonly Random _random = new();

    public SustumService(JailBreakPlugin plugin, IWardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_dsustum", "Deagle ödüllü sustum başlat", CommandDsustum);
        _plugin.RegisterCommand("css_tsustum", "T'ye geçme ödüllü sustum başlat", CommandTsustum);
        _plugin.RegisterCommand("css_tsusdum", "T'ye geçme ödüllü sustum başlat", CommandTsustum);
        _plugin.RegisterCommand("css_olusustum", "Canlanma ödüllü sustum başlat", CommandOlusustum);
    }

    public void CommandDsustum(CCSPlayerController? player, CommandInfo info) => StartSustum(player, SustumMode.Dsustum);
    
    public void CommandTsustum(CCSPlayerController? player, CommandInfo info) => StartSustum(player, SustumMode.Tsustum);
    
    public void CommandOlusustum(CCSPlayerController? player, CommandInfo info) => StartSustum(player, SustumMode.Olusustum);

    private void StartSustum(CCSPlayerController? player, SustumMode mode)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!_wardenService.HasPermission(player, "@css/slay"))
        {
            return;
        }

        _sustumTimer?.Kill();

        _activeMode = mode;
        string baseWord = _plugin.Config.SustumWords[_random.Next(_plugin.Config.SustumWords.Count)];
        int number = _random.Next(100, 999);
        _targetWord = $"{baseWord}{number}";
        _remainingTime = _plugin.Config.SustumDuration;

        string modeName = mode.ToString().ToUpper();
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgSustumStarted, modeName, _targetWord));

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
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgSustumExpired, modeName));
                _activeMode = SustumMode.None;
                _targetWord = "";
                _sustumTimer?.Kill();
                _sustumTimer = null;
                return;
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                string content = string.Format(_plugin.Lang.HudContentSustum, _targetWord, _remainingTime);
                p.PrintToCenterHtml(PluginHelper.FormatHud(_plugin.Lang.HudTitleSustum, content));
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
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgSustumWinner, modeName, player.PlayerName, rewardName));
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
