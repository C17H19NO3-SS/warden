using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CS2MenuManager.API.Menu;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class GameManagerService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly FreezeService _freezeService;
    
    private int _gameTimeRemaining = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _gameTimer;
    private bool _isBoxActive = false;
    private bool _isSaklambacActive = false;

    public GameManagerService(JailBreakPlugin plugin, WardenService wardenService, FreezeService freezeService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _freezeService = freezeService;
    }

    public void OnRoundStart()
    {
        _gameTimer?.Kill();
        _gameTimer = null;
        _isBoxActive = false;
        _isSaklambacActive = false;
    }

    public void OpenBoxMenu(CCSPlayerController player)
    {
        if (!_wardenService.HasPermission(player, "@css/changemap")) return;
        
        var menu = new CenterHtmlMenu("🥊 Boks Modu Süresi", _plugin);
        for (int i = 10; i <= 60; i += 10)
        {
            int time = i;
            menu.AddItem($"{time} Saniye", (p, o) => StartBox(time));
        }
        menu.Display(player, 0);
    }

    public void StartBox(int duration)
    {
        _isBoxActive = true;
        _gameTimeRemaining = duration;
        Server.ExecuteCommand("mp_teammates_are_enemies 1");
        
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Red}Boks Modu Başladı! Süre: {duration}sn"));
        
        _gameTimer?.Kill();
        _gameTimer = _plugin.AddTimer(1.0f, BoxTick, TimerFlags.REPEAT);
    }

    private void BoxTick()
    {
        if (!_isBoxActive) return;

        if (_gameTimeRemaining <= 0)
        {
            EndBox();
            return;
        }

        string content = $"Boks Sürüyor: <font color='{PluginHelper.ColorTime}'><b>{_gameTimeRemaining}s</b></font>";
        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            p.PrintToCenterHtml(PluginHelper.FormatHud("BOKS MODU", content));
        }

        _gameTimeRemaining--;
    }

    private void EndBox()
    {
        _isBoxActive = false;
        _gameTimer?.Kill();
        _gameTimer = null;
        Server.ExecuteCommand("mp_teammates_are_enemies 0");
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " {ChatColors.Green}Boks Modu Bitti."));
    }

    public void StartSaklambac(int duration)
    {
        _isSaklambacActive = true;
        _gameTimeRemaining = duration;

        var ctSpawns = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("info_player_counterterrorist").ToList();
        var cts = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive).ToList();

        foreach (var ct in cts)
        {
            var pawn = ct.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid && ctSpawns.Count > 0)
            {
                var spawn = ctSpawns[new Random().Next(ctSpawns.Count)]; 
                // Teleport and Rotate 180
                QAngle angles = pawn.AbsRotation!;
                angles.Y += 180;
                pawn.Teleport(spawn.AbsOrigin, angles, new CounterStrikeSharp.API.Modules.Utils.Vector(0, 0, 0));
                
                pawn.MoveType = MoveType_t.MOVETYPE_NONE;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_NONE;
            }
        }

        _gameTimer?.Kill();
        _gameTimer = _plugin.AddTimer(1.0f, SaklambacTick, TimerFlags.REPEAT);
    }

    private void SaklambacTick()
    {
        if (!_isSaklambacActive) return;

        if (_gameTimeRemaining <= 0)
        {
            EndSaklambac();
            return;
        }

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            if (p.Team == CsTeam.CounterTerrorist)
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud("SAKLAMBAÇ", "<font size='30' color='black'>KÖR EDİLDİNİZ</font>", "Saklanma süresi bitene kadar bekleyin."));
            }
            else
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud("SAKLAMBAÇ", $"Saklanmak İçin Kalan: <font color='{PluginHelper.ColorTime}'><b>{_gameTimeRemaining}s</b></font>"));
            }
        }

        _gameTimeRemaining--;
    }

    private void EndSaklambac()
    {
        _isSaklambacActive = false;
        _gameTimer?.Kill();
        _gameTimer = null;

        foreach (var ct in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist))
        {
            var pawn = ct.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.MoveType = MoveType_t.MOVETYPE_WALK;
                pawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            }
        }

        _freezeService.FreezeAll();
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " {ChatColors.Green}Saklambaç Bitti! Ebeciler Serbest, Mahkumlar Dondu!"));
    }
}
