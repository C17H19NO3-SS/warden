using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;

namespace JailBreak.Services;

public class IseliService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    private int _iseliTime = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _iseliTimer;
    private readonly Random _random = new();

    public IseliService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void OnRoundStart()
    {
        _iseliTimer?.Kill();
        _iseliTimer = null;
        _iseliTime = 0;
    }

    public void CommandIseli(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        string arg = info.GetArg(1);

        if (arg.ToLower() == "q")
        {
            QuickOpen(player);
            return;
        }

        if (int.TryParse(arg, out int time))
        {
            StartIseli(time);
        }
    }

    public void CommandQuickIseli(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        QuickOpen(player);
    }

    private void StartIseli(int time)
    {
        _iseliTimer?.Kill();
        _iseliTime = time;

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(_plugin.Config.MsgIseliStarted, _iseliTime))}");

        _iseliTimer = _plugin.AddTimer(1.0f, () =>
        {
            if (_iseliTime <= 0)
            {
                FinishIseli();
                _iseliTimer?.Kill();
                _iseliTimer = null;
                return;
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                string content = $"Kapıların açılmasına: <font color='{HudHelper.ColorTime}'><b>{_iseliTime}s</b></font>";
                p.PrintToCenterHtml(HudHelper.FormatHud("İSELİ GERİ SAYIM", content));
            }

            _iseliTime--;
        }, TimerFlags.REPEAT);
    }

    private void QuickOpen(CCSPlayerController player)
    {
        _iseliTimer?.Kill();
        _iseliTimer = null;
        FinishIseli(true);
    }

    private void FinishIseli(bool quick = false)
    {
        OpenAllDoors();
        TeleportTsToRandomSpawns();

        string msg = quick ? _plugin.Config.MsgIseliQuickOpened : _plugin.Config.MsgIseliDoorsOpened;
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(msg)}");
    }

    public static void OpenAllDoors()
    {
        var doors = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door");
        foreach (var door in doors) if (door.IsValid) door.AcceptInput("Open");

        var rotatingDoors = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door_rotating");
        foreach (var door in rotatingDoors) if (door.IsValid) door.AcceptInput("Open");

        var propDoors = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("prop_door_rotating");
        foreach (var door in propDoors) if (door.IsValid) door.AcceptInput("Open");

        var breakables = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_breakable");
        foreach (var breakable in breakables) if (breakable.IsValid) breakable.AcceptInput("Break");
    }

    private void TeleportTsToRandomSpawns()
    {
        var tSpawns = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_terrorist").ToList();
        if (tSpawns.Count == 0) return;

        var tPlayers = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();

        foreach (var player in tPlayers)
        {
            var pawn = player.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                var randomSpawn = tSpawns[_random.Next(tSpawns.Count)];
                pawn.Teleport(randomSpawn.AbsOrigin, randomSpawn.AbsRotation, new Vector(0, 0, 0));
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
