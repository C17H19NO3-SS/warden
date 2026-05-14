using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using JailBreak.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JailBreak.Services;

public enum LRType
{
    None,
    Deagle,
    Knife
}

public enum LRStep
{
    None,
    InitialChoice,
    TypeSelection,
    TargetSelection
}

public class LastRequestService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    private LRStep _currentStep = LRStep.None;
    private LRType _selectedLRType = LRType.None;
    private CCSPlayerController? _lastT;
    private CCSPlayerController? _selectedCT;
    
    // LR State
    private bool _isLRActive = false;
    private bool _isDeagleTurnT = true;
    private List<CCSPlayerController> _ctCandidates = new();
    private int _ctPage = 0;
    private const int CTPerPage = 5;

    public LastRequestService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void OnRoundStart()
    {
        _isLRActive = false;
        _currentStep = LRStep.None;
        _selectedLRType = LRType.None;
        _lastT = null;
        _selectedCT = null;
    }

    public void CommandSonaKalan(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.Team != CsTeam.Terrorist || !player.PawnIsAlive) return;

        var aliveTs = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();
        if (aliveTs.Count != 1)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Red}Bu komutu sadece sona kalan T kullanabilir.");
            return;
        }

        _lastT = player;
        OpenInitialMenu();
    }

    public void CommandSonSec(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !HasPermission(player)) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Red}Kullanım: !sonseç <isim>");
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase) && p.Team == CsTeam.Terrorist && p.PawnIsAlive);
        
        if (target == null || !target.IsValid)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgPlayerNotFound)}");
            return;
        }

        foreach (var t in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            if (t.SteamID != target.SteamID)
            {
                t.PlayerPawn.Value?.CommitSuicide(false, true);
            }
        }

        _lastT = target;
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Green}{target.PlayerName} {ChatColors.Default}sona bırakıldı ve LR menüsü açıldı!");
        OpenInitialMenu();
    }

    private void OpenInitialMenu()
    {
        _currentStep = LRStep.InitialChoice;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (_lastT == null || !_lastT.IsValid) return;

        string title = "SONA KALAN MENÜSÜ";
        string content = "";
        string instruction = "";

        switch (_currentStep)
        {
            case LRStep.InitialChoice:
                content = "!1 LR (Son İstek)<br>!2 İSİAN (Rebellion)";
                break;
            case LRStep.TypeSelection:
                title = "LR TÜRÜ SEÇİN";
                content = "!1 Deagle Düellosu<br>!2 Bıçak Düellosu";
                break;
            case LRStep.TargetSelection:
                title = "RAKİP SEÇİN";
                int start = _ctPage * CTPerPage;
                int end = Math.Min(start + CTPerPage, _ctCandidates.Count);
                for (int i = start; i < end; i++)
                {
                    content += $"!{i - start + 1} {_ctCandidates[i].PlayerName}<br>";
                }
                instruction = "Tab: Sonraki Sayfa";
                break;
        }

        _lastT.PrintToCenterHtml(HudHelper.FormatHud(title, content, instruction));
    }

    public bool HandleLRChat(CCSPlayerController player, string message)
    {
        if (_lastT == null || player.SteamID != _lastT.SteamID || _currentStep == LRStep.None) return false;

        if (message.StartsWith("!"))
        {
            if (int.TryParse(message.Substring(1), out int choice))
            {
                switch (_currentStep)
                {
                    case LRStep.InitialChoice:
                        if (choice == 1) { _currentStep = LRStep.TypeSelection; UpdateHUD(); }
                        else if (choice == 2) { StartIsyan(); }
                        return true;

                    case LRStep.TypeSelection:
                        if (choice == 1) { _selectedLRType = LRType.Deagle; StartTargetSelection(); }
                        else if (choice == 2) { _selectedLRType = LRType.Knife; StartTargetSelection(); }
                        return true;

                    case LRStep.TargetSelection:
                        if (choice > 0 && choice <= CTPerPage)
                        {
                            int index = (_ctPage * CTPerPage) + choice - 1;
                            if (index >= 0 && index < _ctCandidates.Count)
                            {
                                SelectTarget(_ctCandidates[index]);
                                return true;
                            }
                        }
                        return false;
                }
            }
        }
        return false;
    }

    public void OnTick()
    {
        if (_currentStep == LRStep.TargetSelection && _lastT != null && _lastT.IsValid)
        {
            if ((_lastT.Buttons & PlayerButtons.Scoreboard) != 0) // Tab to change page
            {
                _ctPage++;
                if (_ctPage * CTPerPage >= _ctCandidates.Count) _ctPage = 0;
                UpdateHUD();
            }
        }
    }

    private void StartIsyan()
    {
        _currentStep = LRStep.None;
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Red}{_lastT?.PlayerName} {ChatColors.Default}isyan etmeyi seçti!");
    }

    private void StartTargetSelection()
    {
        _ctCandidates = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive).ToList();
        if (_ctCandidates.Count == 0)
        {
            _lastT?.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Red}Düello yapacak CT bulunamadı.");
            _currentStep = LRStep.None;
            return;
        }
        _currentStep = LRStep.TargetSelection;
        _ctPage = 0;
        UpdateHUD();
    }

    private void SelectTarget(CCSPlayerController target)
    {
        _selectedCT = target;
        _currentStep = LRStep.None;
        StartLR();
    }

    private void StartLR()
    {
        if (_lastT == null || _selectedCT == null) return;

        _isLRActive = true;
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Green}LR Başladı! {ChatColors.Red}{_lastT.PlayerName} vs {ChatColors.Blue}{_selectedCT.PlayerName}");
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Yellow}Tür: {_selectedLRType}");

        if (_selectedLRType == LRType.Deagle)
        {
            _isDeagleTurnT = true;
            PrepareDeagleDuel();
        }
        else if (_selectedLRType == LRType.Knife)
        {
            PrepareKnifeDuel();
        }
    }

    private void PrepareDeagleDuel()
    {
        _lastT!.RemoveWeapons();
        _selectedCT!.RemoveWeapons();
        
        _lastT.GiveNamedItem("weapon_deagle");
        _lastT.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Green}Sıra sende! Ateş et.");
        _selectedCT.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Yellow}Rakibinin ateş etmesini bekle.");
        
        // Turn-based logic will be handled in EventWeaponFire
    }

    private void PrepareKnifeDuel()
    {
        _lastT!.RemoveWeapons();
        _selectedCT!.RemoveWeapons();
        _lastT.GiveNamedItem("weapon_knife");
        _selectedCT.GiveNamedItem("weapon_knife");
    }

    public void OnWeaponFire(EventWeaponFire @event)
    {
        if (!_isLRActive || _selectedLRType != LRType.Deagle) return;

        var player = @event.Userid;
        if (player == null || !player.IsValid) return;

        if (_isDeagleTurnT && player.SteamID == _lastT?.SteamID)
        {
            _isDeagleTurnT = false;
            TransferDeagle(_lastT, _selectedCT!);
        }
        else if (!_isDeagleTurnT && player.SteamID == _selectedCT?.SteamID)
        {
            _isDeagleTurnT = true;
            TransferDeagle(_selectedCT, _lastT!);
        }
    }

    private void TransferDeagle(CCSPlayerController from, CCSPlayerController to)
    {
        from.RemoveWeapons();
        from.GiveNamedItem("weapon_knife");
        
        to.RemoveWeapons();
        to.GiveNamedItem("weapon_deagle");
        
        to.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Green}Sıra sende! Ateş et.");
    }

    public void OnPlayerDeath(EventPlayerDeath @event)
    {
        if (!_isLRActive) return;

        var victim = @event.Userid;
        if (victim == null) return;

        if (victim.SteamID == _lastT?.SteamID || victim.SteamID == _selectedCT?.SteamID)
        {
            _isLRActive = false;
            
            // Eğer kaybeden CT ise ve koruma ise (Warden değilse), T takımına atılacak
            if (victim.SteamID == _selectedCT?.SteamID)
            {
                if (!_wardenService.IsWarden(_selectedCT!))
                {
                    Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Red}{_selectedCT!.PlayerName} {ChatColors.Default}LR kaybettiği için T takımına atıldı!");
                    _selectedCT.ChangeTeam(CsTeam.Terrorist);
                }
            }
        }
    }

    private bool HasPermission(CCSPlayerController player)
    {
        return _wardenService.IsWarden(player) || 
               AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") || 
               AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
