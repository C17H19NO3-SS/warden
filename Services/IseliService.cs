using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class IseliService : IIseliService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;
    private int _iseliTimeRemaining = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _iseliTimer;
    private bool _isIseliActive = false;

    public IseliService(JailBreakPlugin plugin, IWardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_iseli", "İseli kapı kontrolü", CommandIseli);
        _plugin.RegisterCommand("css_iq", "Kapıları anında aç", CommandQuickIseli);
    }

    public void OnRoundStart()
    {
        _iseliTimer?.Kill();
        _iseliTimer = null;
        _iseliTimeRemaining = 0;
        _isIseliActive = false;
    }

    public void CommandIseli(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        if (_isIseliActive)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " {ChatColors.Red}Zaten bir iseli süreci aktif!"));
            return;
        }

        string arg = info.GetArg(1);
        int time = int.TryParse(arg, out int t) ? t : 30;

        StartIseli(time);
    }

    public void CommandQuickIseli(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;
        QuickOpen(player);
    }

    public void QuickOpen(CCSPlayerController player)
    {
        _iseliTimer?.Kill();
        _iseliTimer = null;
        _isIseliActive = false;
        _iseliTimeRemaining = 0;

        OpenAllDoors();
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgIseliQuickOpened));
    }

    private void StartIseli(int time)
    {
        _isIseliActive = true;
        _iseliTimeRemaining = time;

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgIseliStarted, _iseliTimeRemaining)));

        _iseliTimer = _plugin.AddTimer(1.0f, () =>
        {
            if (_iseliTimeRemaining <= 0)
            {
                EndIseli();
                return;
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                string content = string.Format(_plugin.Lang.HudContentIseli, _iseliTimeRemaining);
                p.PrintToCenterHtml(PluginHelper.FormatHud(_plugin.Lang.HudTitleIseli, content));
            }

            _iseliTimeRemaining--;
        }, TimerFlags.REPEAT);
    }

    private void EndIseli()
    {
        _iseliTimer?.Kill();
        _iseliTimer = null;
        _isIseliActive = false;
        _iseliTimeRemaining = 0;

        OpenAllDoors();
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgIseliDoorsOpened));
    }

    private void OpenAllDoors()
    {
        var doors = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door").ToList();
        var rotatingDoors = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door_rotating").ToList();

        foreach (var door in doors.Concat(rotatingDoors))
        {
            door.AcceptInput("Open");
        }
    }
}
