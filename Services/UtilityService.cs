using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Linq;
using JailBreak.Helpers;
using CS2MenuManager.API.Menu;
using System.Drawing;

namespace JailBreak.Services;

/// <summary>
/// Provides a variety of utility commands and functionalities for the Warden.
/// </summary>
public class UtilityService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly HudService _hudService;
    private readonly Random _random = new();
    private readonly Dictionary<ulong, (string Name, int Value)> _kacCmRecords = new();
    private int _currentCTRevives;

    /// <summary>
    /// Initializes a new instance of the <see cref="UtilityService"/> class.
    /// </summary>
    /// <param name="plugin">The main plugin instance.</param>
    /// <param name="wardenService">The warden service instance.</param>
    /// <param name="hudService">The HUD service instance.</param>
    public UtilityService(JailBreakPlugin plugin, WardenService wardenService, HudService hudService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _hudService = hudService;
        _currentCTRevives = plugin.Config.MaxCTRevives;
    }

    /// <summary>
    /// Registers all utility commands with the command dispatcher.
    /// </summary>
    /// <param name="dispatcher">The command dispatcher.</param>
    public void RegisterCommands(ICommandDispatcher dispatcher)
    {
        dispatcher.RegisterCommand("css_hpa", "Herkesin canını 100 yap", CommandHpAll);
        dispatcher.RegisterCommand("css_hpall", "Herkesin canını 100 yap", CommandHpAll);
        dispatcher.RegisterCommand("css_hpt", "T takımının canını 100 yap", CommandHpT);
        dispatcher.RegisterCommand("css_hpct", "CT takımının canını 100 yap", CommandHpCT);
        dispatcher.RegisterCommand("css_gelt", "Tüm T'leri çek", CommandGetT);
        dispatcher.RegisterCommand("css_gelct", "Tüm CT'leri çek", CommandGetCT);
        dispatcher.RegisterCommand("css_gelall", "Tüm oyuncuları çek", CommandGetAll);
        dispatcher.RegisterCommand("css_af", "Herkesi canlandır ve canını 100 yap", CommandAf);
        dispatcher.RegisterCommand("css_git", "Oyuncuya git", CommandGit);
        dispatcher.RegisterCommand("css_haksal", "CT ile T takımını yer değiştir", CommandHakSal);
        dispatcher.RegisterCommand("css_ba", "Bunnyhop aç", CommandBunnyOpen);
        dispatcher.RegisterCommand("css_bk", "Bunnyhop kapat", CommandBunnyClose);
        dispatcher.RegisterCommand("css_otores", "Otomatik canlanmayı aç", CommandOtores);
        dispatcher.RegisterCommand("css_otores0", "Otomatik canlanmayı kapat", CommandOtores0);
        dispatcher.RegisterCommand("css_gom", "Oyuncuyu göm", CommandGom);
        dispatcher.RegisterCommand("css_gom0", "Oyuncuyu gömülmekten çıkar", CommandGom0);
        dispatcher.RegisterCommand("css_umct", "CT takımının mutesini aç", CommandUnmuteCT);
        dispatcher.RegisterCommand("css_uct", "CT takımının mutesini aç", CommandUnmuteCT);
        dispatcher.RegisterCommand("css_umt", "T takımının mutesini aç", CommandUnmuteT);
        dispatcher.RegisterCommand("css_ut", "T takımının mutesini aç", CommandUnmuteT);
        dispatcher.RegisterCommand("css_ss", "T takımının silahlarını al", CommandSs);
        dispatcher.RegisterCommand("css_strip", "T takımının silahlarını al", CommandSs);
        dispatcher.RegisterCommand("css_kaccm", "Kaç cm ölçer", CommandKacCm);
        dispatcher.RegisterCommand("css_mct", "CT takımını mutele", CommandMuteCT);
        dispatcher.RegisterCommand("css_mt", "T takımını mutele", CommandMuteT);
        dispatcher.RegisterCommand("css_topkaccm", "Kaç cm sıralamasını göster", CommandTopKacCm);
        dispatcher.RegisterCommand("css_delay", "3 saniyelik ses gecikmesini giderir", CommandDelay);
        dispatcher.RegisterCommand("css_msay", "Ekranda büyük duyuru yapar", CommandMsay);
        dispatcher.RegisterCommand("css_csay", "Ekranın ortasında duyuru yapar", CommandCsay);
        dispatcher.RegisterCommand("css_hsay", "HUD kısmında duyuru yapar", CommandHsay);
        dispatcher.RegisterCommand("css_rev", "Oyuncu canlandırır", CommandRev);
        dispatcher.RegisterCommand("css_fsay", "Oyuncuya zorla say yazdırır", CommandFsay);
        dispatcher.RegisterCommand("css_kill", "Belirtilen oyuncuyu öldürür", CommandKill);
    }

    /// <summary>
    /// Resets the utility state at the start of a round.
    /// </summary>
    public void OnRoundStart()
    {
        _currentCTRevives = _plugin.Config.MaxCTRevives;
    }

    /// <summary>
    /// Displays a message to the left-center of the HUD.
    /// </summary>
    /// <param name="player">The player to display the message to.</param>
    /// <param name="info">Command information containing the message.</param>
    public void CommandMsay(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !_wardenService.HasPermission(player, "@css/chat")) return;
        string message = info.ArgString.Trim();
        if (string.IsNullOrEmpty(message)) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            _hudService.SendLeftCenterHudMessage(p, message, Color.Red);
        }
    }

    /// <summary>
    /// Displays a message in the center of the HUD.
    /// </summary>
    /// <param name="player">The player to display the message to.</param>
    /// <param name="info">Command information containing the message.</param>
    public void CommandCsay(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !_wardenService.HasPermission(player, "@css/chat")) return;
        string message = info.ArgString.Trim();
        if (string.IsNullOrEmpty(message)) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            _hudService.SendLeftCenterHudMessage(p, message, Color.LightBlue);
        }
    }

    /// <summary>
    /// Displays a message in the HUD area.
    /// </summary>
    /// <param name="player">The player to display the message to.</param>
    /// <param name="info">Command information containing the message.</param>
    public void CommandHsay(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !_wardenService.HasPermission(player, "@css/chat")) return;
        string message = info.ArgString.Trim();
        if (string.IsNullOrEmpty(message)) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            _hudService.SendLeftCenterHudMessage(p, message, Color.Yellow);
        }
    }

    /// <summary>
    /// Respawns a player or group of players.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information containing the target.</param>
    public void CommandRev(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !_wardenService.HasPermission(player, "@css/slay")) return;

        string target = info.GetArg(1).ToLower();
        var players = Utilities.GetPlayers().Where(p => p.IsValid).ToList();

        if (target == "@all")
        {
            foreach (var p in players) if (!p.PawnIsAlive) p.Respawn();
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Green}Herkes canlandırıldı."));
        }
        else if (target == "@t")
        {
            foreach (var p in players.Where(p => p.Team == CsTeam.Terrorist)) if (!p.PawnIsAlive) p.Respawn();
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Green}T takımı canlandırıldı."));
        }
        else if (target == "@ct")
        {
            foreach (var p in players.Where(p => p.Team == CsTeam.CounterTerrorist)) if (!p.PawnIsAlive) p.Respawn();
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Green}CT takımı canlandırıldı."));
        }
        else
        {
            var p = players.FirstOrDefault(x => x.PlayerName.Contains(target, StringComparison.OrdinalIgnoreCase));
            if (p != null)
            {
                p.Respawn();
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Green}{p.PlayerName} canlandırıldı."));
            }
        }
    }

    /// <summary>
    /// Forces a player to say a message in chat.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information containing target and message.</param>
    public void CommandFsay(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/root")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName)) return;

        string message = info.ArgString.Substring(targetName.Length).Trim();

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));
        if (target != null && !string.IsNullOrEmpty(message))
        {
            target.ExecuteClientCommand($"say {message}");
        }
    }

    /// <summary>
    /// Sets all players' health to 100.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandHpAll(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid)
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
            {
                var pawn = p.PlayerPawn.Value;
                if (pawn != null && pawn.IsValid)
                {
                    pawn.Health = 100;
                }
            }
            Server.PrintToConsole("[JailBreak] Tüm oyuncuların canı 100 yapıldı.");
            return;
        }

        if (!_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            var pawn = p.PlayerPawn.Value;
            if (pawn != null && pawn.IsValid)
            {
                pawn.Health = 100;
            }
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpAllSet));
    }

    /// <summary>
    /// Sets Terrorist players' health to 100.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandHpT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpTSet));
    }

    /// <summary>
    /// Sets Counter-Terrorist players' health to 100.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandHpCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.Health = 100;
            Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHpCTSet));
    }

    /// <summary>
    /// Teleports all Terrorist players to the caller's position.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandGetT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/kick")) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetTApplied));
    }

    /// <summary>
    /// Teleports all Counter-Terrorist players to the caller's position.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandGetCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/kick")) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetCTApplied));
    }

    /// <summary>
    /// Teleports all players to the caller's position.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandGetAll(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/kick")) return;

        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            p.PlayerPawn.Value?.Teleport(origin, player.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGetAllApplied));
    }

    /// <summary>
    /// Revives all players and sets their health to 100.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandAf(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        _currentCTRevives = _plugin.Config.MaxCTRevives;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.Team != CsTeam.Spectator && p.Team != CsTeam.None))
        {
            if (!p.PawnIsAlive)
            {
                p.Respawn();
            }

            if (p.PawnIsAlive)
            {
                p.Health = 100;
                Utilities.SetStateChanged(p, "CBaseEntity", "m_iHealth");
            }
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgAfApplied));
    }

    /// <summary>
    /// Teleports the caller to the specified player.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information containing target name.</param>
    public void CommandGit(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/kick")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgGitUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase));
        if (target == null || !target.IsValid || !target.PawnIsAlive)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
            return;
        }

        player.PlayerPawn.Value?.Teleport(target.PlayerPawn.Value?.AbsOrigin, target.PlayerPawn.Value?.AbsRotation, new Vector(0, 0, 0));
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgGitApplied, target.PlayerName)));
    }

    /// <summary>
    /// Swaps the teams of the caller and the target Terrorist player.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information containing target name.</param>
    public void CommandHakSal(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.Team != CsTeam.CounterTerrorist || !_wardenService.HasPermission(player, "@css/generic")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHakSalUsage));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, System.StringComparison.OrdinalIgnoreCase) && p.Team == CsTeam.Terrorist);

        if (target == null || !target.IsValid)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgHakSalTargetNotFound));
            return;
        }

        Server.NextFrame(() =>
        {
            if (player.IsValid && target.IsValid)
            {
                player.ChangeTeam(CsTeam.Terrorist);
                target.ChangeTeam(CsTeam.CounterTerrorist);
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgHakSalApplied, player.PlayerName, target.PlayerName)));
            }
        });
    }

    /// <summary>
    /// Enables bunnyhopping.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandBunnyOpen(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        Server.ExecuteCommand("sv_autobunnyhopping 1");
        Server.ExecuteCommand("sv_enablebunnyhopping 1");
        Server.ExecuteCommand("sv_airaccelerate 2000");
        Server.ExecuteCommand("sv_legacy_jump 1");
        Server.ExecuteCommand("sv_staminajumpcost 0");
        Server.ExecuteCommand("sv_staminalandcost 0");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgBunnyEnabled));
    }

    /// <summary>
    /// Disables bunnyhopping.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandBunnyClose(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        Server.ExecuteCommand("sv_autobunnyhopping 0");
        Server.ExecuteCommand("sv_enablebunnyhopping 0");
        Server.ExecuteCommand("sv_airaccelerate 12");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgBunnyDisabled));
    }

    /// <summary>
    /// Unmutes all Counter-Terrorist players.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandUnmuteCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist))
        {
            p.VoiceFlags = VoiceFlags.Normal;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgUnmuteCTApplied));
    }

    /// <summary>
    /// Unmutes all Terrorist players.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandUnmuteT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist))
        {
            p.VoiceFlags = VoiceFlags.Normal;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgUnmuteTApplied));
    }

    /// <summary>
    /// Removes all weapons from Terrorists and gives them a knife.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandSs(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            p.RemoveWeapons();
            p.GiveNamedItem("weapon_knife");
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgSsApplied));
    }

    /// <summary>
    /// Measures "length" (funny command).
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandKacCm(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        int cm = _random.Next(1, 41);
        
        if (!_kacCmRecords.TryGetValue(player.SteamID, out var record) || cm > record.Value)
        {
            _kacCmRecords[player.SteamID] = (player.PlayerName, cm);
        }

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgKacCmApplied, player.PlayerName, cm)));
    }

    /// <summary>
    /// Shows top players' "length" records.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandTopKacCm(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_kacCmRecords.Count == 0)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopKacCmEmpty));
            return;
        }

        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgTopKacCmTitle));

        var sortedRecords = _kacCmRecords.Values
            .OrderByDescending(r => r.Value)
            .Take(5)
            .ToList();

        for (int i = 0; i < sortedRecords.Count; i++)
        {
            var r = sortedRecords[i];
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgTopKacCmEntry, i + 1, r.Name, r.Value)));
        }
    }

    /// <summary>
    /// Resets all "length" records.
    /// </summary>
    public void ResetKacCmRecords()
    {
        _kacCmRecords.Clear();
    }

    /// <summary>
    /// Mutes all Counter-Terrorists.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandMuteCT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.CounterTerrorist))
        {
            p.VoiceFlags = VoiceFlags.Muted;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgMuteCTApplied));
    }

    /// <summary>
    /// Mutes all Terrorists.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandMuteT(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist))
        {
            p.VoiceFlags = VoiceFlags.Muted;
        }
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgMuteTApplied));
    }

    /// <summary>
    /// Enables automatic respawning for players.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandOtores(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        Server.ExecuteCommand("mp_respawn_on_death_t 1");
        Server.ExecuteCommand("mp_respawn_on_death_ct 1");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOtoresEnabled));
    }

    /// <summary>
    /// Disables automatic respawning for players.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandOtores0(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/generic")) return;

        Server.ExecuteCommand("mp_respawn_on_death_t 0");
        Server.ExecuteCommand("mp_respawn_on_death_ct 0");

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOtoresDisabled));
    }

    /// <summary>
    /// Buries a player in the ground.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information containing target name.</param>
    public void CommandGom(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " Kullanım: !gom <isim>"));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target == null || !target.IsValid || !target.PawnIsAlive)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
            return;
        }

        var pawn = target.PlayerPawn.Value;
        if (pawn != null && pawn.IsValid)
        {
            Vector origin = pawn.AbsOrigin!;
            pawn.Teleport(new Vector(origin.X, origin.Y, origin.Z - 35.0f), pawn.AbsRotation, new Vector(0, 0, 0));
            pawn.MoveType = MoveType_t.MOVETYPE_NONE;
            pawn.ActualMoveType = MoveType_t.MOVETYPE_NONE;
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Red}{target.PlayerName} {ChatColors.Default}gömüldü."));
        }
    }

    /// <summary>
    /// Unburies a player.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information containing target name.</param>
    public void CommandGom0(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || !_wardenService.HasPermission(player, "@css/slay")) return;

        string targetName = info.GetArg(1);
        if (string.IsNullOrEmpty(targetName))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, " Kullanım: !gom0 <isim>"));
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(p => p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target == null || !target.IsValid || !target.PawnIsAlive)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgPlayerNotFound));
            return;
        }

        var pawn = target.PlayerPawn.Value;
        if (pawn != null && pawn.IsValid)
        {
            Vector origin = pawn.AbsOrigin!;
            pawn.Teleport(new Vector(origin.X, origin.Y, origin.Z + 35.0f), pawn.AbsRotation, new Vector(0, 0, 0));
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;
            pawn.ActualMoveType = MoveType_t.MOVETYPE_WALK;
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, $" {ChatColors.Green}{target.PlayerName} {ChatColors.Default}gömülmekten çıkarıldı."));
        }
    }

    /// <summary>
    /// Clears 3-second voice delay for the player.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandDelay(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (player.VoiceFlags == VoiceFlags.Muted) return;

        player.VoiceFlags = VoiceFlags.Muted;
        player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgDelayStarted));

        _plugin.AddTimer(3.0f, () =>
        {
            if (player.IsValid)
            {
                player.VoiceFlags = VoiceFlags.Normal;
                player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgDelayEnded));
            }
        });
    }

    /// <summary>
    /// Forces the player to kill themselves.
    /// </summary>
    /// <param name="player">The player initiating the command.</param>
    /// <param name="info">Command information.</param>
    public void CommandKill(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        player.PlayerPawn.Value?.CommitSuicide(false, true);
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgSelfKillApplied, player.PlayerName)));
    }
}
