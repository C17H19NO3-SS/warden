using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CS2MenuManager.API.Menu;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Enum;
using JailBreak.Helpers;

namespace JailBreak.Services;

public enum LRType
{
    None,
    Deagle,
    Knife
}

public class LastRequestService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    private LRType _selectedLRType = LRType.None;
    private CCSPlayerController? _lastT;
    private CCSPlayerController? _selectedCT;

    private bool _isLRActive = false;
    private bool _isDeagleTurnT = true;

    public LastRequestService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void OnRoundStart()
    {
        _isLRActive = false;
        _selectedLRType = LRType.None;
        _lastT = null;
        _selectedCT = null;
    }

    public void CommandSonaKalan(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.Team != CsTeam.Terrorist || !player.PawnIsAlive) return;
        if (!_plugin.IsJailbreakMap()) return;

        var aliveTs = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive).ToList();
        if (aliveTs.Count != 1)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgLROnlyLastT));
            return;
        }

        _lastT = player;
        OpenInitialMenu();
    }

    public void CommandSonSec(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;
        if (!_plugin.IsJailbreakMap()) return;

        string targetName = info.ArgString.Trim(); 
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgSonSecUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => 
            p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive &&
            (p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase) || p.SteamID.ToString() == targetName));

        if (target == null) 
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
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
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgSonSecApplied, target.PlayerName)));
        OpenInitialMenu();
    }

    private CenterHtmlMenu GetInitialMenu()
    {
        CenterHtmlMenu menu = new(_plugin.Lang.HudTitleLRMain, _plugin);

        menu.AddItem("LR", (p, o) =>
        {
            Server.NextFrame(() =>
            {
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgLRChoiceLR, _lastT?.PlayerName)));
            });
            OpenTypeSelectionMenu();
        });

        menu.AddItem("İsyan", (p, o) =>
        {
            StartIsyan();
        });

        string krediText = string.Format("Kredi Al ({0})", _plugin.Config.LRCreditReward);
        menu.AddItem(krediText, (p, o) =>
        {
            GiveCreditsAndEnd();
        });

        return menu;
    }

    private void OpenInitialMenu()
    {
        if (_lastT == null || !_lastT.IsValid) return;
        GetInitialMenu().Display(_lastT, 0);
    }

    private CenterHtmlMenu GetTypeSelectionMenu()
    {
        CenterHtmlMenu menu = new(_plugin.Lang.HudTitleLRType, _plugin);
        menu.AddItem("Deagle Düellosu", (p, o) =>
        {
            _selectedLRType = LRType.Deagle;
            OpenTargetSelectionMenu();
        });
        menu.AddItem("Bıçak Düellosu", (p, o) =>
        {
            _selectedLRType = LRType.Knife;
            OpenTargetSelectionMenu();
        });

        menu.PrevMenu = GetInitialMenu();
        return menu;
    }

    private void OpenTypeSelectionMenu()
    {
        if (_lastT == null || !_lastT.IsValid) return;
        GetTypeSelectionMenu().Display(_lastT, 0);
    }

    private void OpenTargetSelectionMenu()
    {
        if (_lastT == null || !_lastT.IsValid) return;

        CenterHtmlMenu menu = new(_plugin.Lang.HudTitleLRTarget, _plugin);
        var ctCandidates = Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive).ToList();

        if (ctCandidates.Count == 0)
        {
            _lastT.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgLRNoCTFound));
            return;
        }

        foreach (var ct in ctCandidates)
        {
            menu.AddItem(ct.PlayerName, (p, o) =>
            {
                SelectTarget(ct);
            });
        }

        menu.PrevMenu = GetTypeSelectionMenu();
        menu.Display(_lastT, 0);
    }

    private void StartIsyan()
    {
        Server.NextFrame(() =>
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgLRRebellion, _lastT?.PlayerName)));
        });
    }

    private void GiveCreditsAndEnd()
    {
        if (_lastT == null || !_lastT.IsValid) return;

        var userId = _lastT.UserId;
        StoreApi.StoreBridge.GiveCredits(_lastT, _plugin.Config.LRCreditReward);
        
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgLRCreditReceived, _lastT.PlayerName)));
        
        Server.ExecuteCommand($"css_slay #{userId}");
    }

    private void SelectTarget(CCSPlayerController target)
    {
        _selectedCT = target;
        StartLR();
    }

    private void StartLR()
    {
        if (_lastT == null || _selectedCT == null) return;

        _isLRActive = true;
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgLRStarted, _lastT.PlayerName, _selectedCT.PlayerName)));
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgLRTypeInfo, _selectedLRType)));

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
        _lastT.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgLRDeagleTurn));
        _selectedCT.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgLRDeagleWait));

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

        to.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgLRDeagleTurn));
    }

    public void OnPlayerDeath(EventPlayerDeath @event)
    {
        if (!_isLRActive) return;

        var victim = @event.Userid;
        if (victim == null) return;

        if (victim.SteamID == _lastT?.SteamID || victim.SteamID == _selectedCT?.SteamID)
        {
            _isLRActive = false;

            if (victim.SteamID == _selectedCT?.SteamID)
            {
                if (!_wardenService.IsWarden(_selectedCT!))
                {
                    Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgLRCTLost, _selectedCT!.PlayerName)));
                    _selectedCT.ChangeTeam(CsTeam.Terrorist);
                }
            }
        }
    }
}

