using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class VoteService : IVoteService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;

    private bool _isCandidatePhase = false;

    private bool _isVotePhase = false;

    private int _phaseTimer = 0;

    private CounterStrikeSharp.API.Modules.Timers.Timer? _tickTimer;

    private readonly List<CCSPlayerController> _candidates = new();

    private readonly Dictionary<ulong, ulong> _votes = new();

    private bool _isKickVotePhase = false;

    private readonly Dictionary<ulong, bool> _kickVotes = new();

    public VoteService(JailBreakPlugin plugin, IWardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_komoyla", "Komutçu oylamasını başlat", CommandStartVote);
        _plugin.RegisterCommand("css_komdk", "Komutçuyu atma oylamasını başlat", CommandStartKickVote);
        _plugin.RegisterCommand("css_komaday", "Komutçu oylamasına katıl", CommandJoinVote);
    }

    public void CommandStartVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            PluginHelper.ReplyToCommand(player, _plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap);
            return;
        }

        if (player != null && !_wardenService.HasPermission(player, "@css/vote"))
        {
            PluginHelper.ReplyToCommand(player, _plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyAdminsCanVote);
            return;
        }

        if (_isCandidatePhase || _isVotePhase || _isKickVotePhase)
        {
            PluginHelper.ReplyToCommand(player, _plugin.Config.ChatPrefix, _plugin.Lang.MsgVoteAlreadyActive);
            return;
        }

        StartCandidatePhase();
    }

    public void CommandStartKickVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!_wardenService.HasPermission(player, "@css/vote"))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyAdminsCanVote));
            return;
        }

        if (_wardenService.CurrentWarden == null || !_wardenService.CurrentWarden.IsValid)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNoActiveWarden));
            return;
        }

        if (_isCandidatePhase || _isVotePhase || _isKickVotePhase)
        {
            PluginHelper.ReplyToCommand(player, _plugin.Config.ChatPrefix, _plugin.Lang.MsgVoteAlreadyActive);
            return;
        }

        StartKickVotePhase();
    }

    public void StartCandidatePhase()
    {
        _isCandidatePhase = true;
        _phaseTimer = _plugin.Config.VotePhaseDuration;
        _candidates.Clear();
        _votes.Clear();

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCandidatePhaseStarted));

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);
    }

    public void StartKickVotePhase()
    {
        _isKickVotePhase = true;
        _phaseTimer = _plugin.Config.VotePhaseDuration;
        _kickVotes.Clear();

        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgKickVotePhaseStarted));

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);
    }

    public void CommandJoinVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgOnlyJailbreakMap));
            return;
        }

        if (!_isCandidatePhase)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCandidatePhaseNotActive));
            return;
        }

        if (_candidates.Any(c => c.SteamID == player.SteamID))
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgAlreadyCandidate));
            return;
        }

        if (_candidates.Count >= 5)
        {
            player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCandidateListFull));
            return;
        }

        _candidates.Add(player);
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgPlayerBecameCandidate, player.PlayerName, _candidates.Count)));
    }

    private void Tick()
    {
        _phaseTimer--;

        if (_isCandidatePhase)
        {
            string content = string.Format(_plugin.Lang.HudContentVoteCandidate, "", _phaseTimer);
            foreach (var c in _candidates)
            {
                content += $"- {c.PlayerName}<br>";
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud(_plugin.Lang.HudTitleVoteCandidate, content));
            }

            if (_phaseTimer <= 0)
            {
                if (_candidates.Count == 0)
                {
                    Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgNoCandidates));
                    ResetPhase();
                }
                else
                {
                    StartVotePhase();
                }
            }
        }
        else if (_isVotePhase)
        {
            string content = string.Format(_plugin.Lang.HudContentVoteSelection, "", _phaseTimer);
            for (int i = 0; i < _candidates.Count; i++)
            {
                int voteCount = _votes.Values.Count(v => v == _candidates[i].SteamID);
                content += $"[{i + 1}] {_candidates[i].PlayerName} - Oylar: {voteCount}<br>";
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud(_plugin.Lang.HudTitleVoteSelection, content));
            }

            if (_phaseTimer <= 0)
            {
                EndVotePhase();
            }
        }
        else if (_isKickVotePhase)
        {
            int keepCount = _kickVotes.Values.Count(v => v == true);
            int kickCount = _kickVotes.Values.Count(v => v == false);

            string title = string.Format(_plugin.Lang.HudTitleVoteKick, _wardenService.CurrentWarden?.PlayerName);
            string content = string.Format(_plugin.Lang.HudContentVoteKick, "", _phaseTimer);
            content += $"[1] Komutçu Kalsın - Oylar: {keepCount}<br>";
            content += $"[2] Komutçu Atılsın - Oylar: {kickCount}<br>";

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(PluginHelper.FormatHud(title, content));
            }

            if (_phaseTimer <= 0)
            {
                EndKickVotePhase();
            }
        }
    }

    private void StartVotePhase()
    {
        _isCandidatePhase = false;
        _isVotePhase = true;
        _phaseTimer = _plugin.Config.VotePhaseDuration;
        Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgVotePhaseStarted));
    }

    private void EndVotePhase()
    {
        var winner = _candidates.OrderByDescending(c => _votes.Values.Count(v => v == c.SteamID)).FirstOrDefault();

        if (winner != null && winner.IsValid)
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgVoteEndedNewWarden, winner.PlayerName)));
            _wardenService.SetWarden(winner);
        }
        else
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgVoteCancelled));
        }

        ResetPhase();
    }

    private void EndKickVotePhase()
    {
        int keepCount = _kickVotes.Values.Count(v => v == true);
        int kickCount = _kickVotes.Values.Count(v => v == false);

        if (kickCount > keepCount)
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgKickVoteDecided, kickCount, keepCount)));

            _plugin.AddTimer(_plugin.Config.KickVoteDelayedDuration, () =>
            {
                Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgKickVoteDelayedSuccess));
                _wardenService.RemoveWarden();

                foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && p.Team == CsTeam.CounterTerrorist))
                {
                    player.ChangeTeam(CsTeam.Terrorist);
                }

                StartCandidatePhase();
            });
        }
        else
        {
            Server.PrintToChatAll(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgKickVoteStayed, keepCount, kickCount)));
        }

        ResetPhase();
    }

    private void ResetPhase()
    {
        _isCandidatePhase = false;
        _isVotePhase = false;
        _isKickVotePhase = false;
        _tickTimer?.Kill();
        _tickTimer = null;
    }

    public bool HandleVoteChat(CCSPlayerController player, string message)
    {
        if (_isVotePhase)
        {
            if (int.TryParse(message, out int choice))
            {
                if (choice > 0 && choice <= _candidates.Count)
                {
                    var candidate = _candidates[choice - 1];
                    _votes[player.SteamID] = candidate.SteamID;
                    player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, string.Format(_plugin.Lang.MsgVoteCast, candidate.PlayerName)));
                    return true;
                }
            }
            return false;
        }

        if (_isKickVotePhase)
        {
            if (_wardenService.IsWarden(player))
            {
                player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgCannotVoteSelf));
            }

            if (int.TryParse(message, out int choice))
            {
                if (choice == 1)
                {
                    _kickVotes[player.SteamID] = true;
                    player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgKickVoteKeepCast));
                    return true;
                }
                else if (choice == 2)
                {
                    _kickVotes[player.SteamID] = false;
                    player.PrintToChat(PluginHelper.FormatChat(_plugin.Config.ChatPrefix, _plugin.Lang.MsgKickVoteKickCast));
                    return true;
                }
            }
            return false;
        }

        return false;
    }
}
