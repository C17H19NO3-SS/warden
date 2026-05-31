using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace JailBreak.Services;

public class VoteService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    // Normal Vote State
    private bool _isCandidatePhase = false;
    private bool _isVotePhase = false;
    private int _phaseTimer = 0;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _tickTimer;
    private readonly List<CCSPlayerController> _candidates = new();
    private readonly Dictionary<ulong, ulong> _votes = new();

    // Kick Vote State
    private bool _isKickVotePhase = false;
    private readonly Dictionary<ulong, bool> _kickVotes = new();

    public VoteService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public void CommandStartVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        if (!AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") && !AdminManager.PlayerHasPermissions(player, "@css/vote") && !AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyAdminsCanVote)}");
            return;
        }

        if (_isCandidatePhase || _isVotePhase || _isKickVotePhase)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgVoteAlreadyActive)}");
            return;
        }

        StartCandidatePhase();
    }

    public void CommandStartKickVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        if (!AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") && !AdminManager.PlayerHasPermissions(player, "@css/vote") && !AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyAdminsCanVote)}");
            return;
        }

        if (_wardenService.CurrentWarden == null || !_wardenService.CurrentWarden.IsValid)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgNoActiveWarden)}");
            return;
        }

        if (_isCandidatePhase || _isVotePhase || _isKickVotePhase)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgVoteAlreadyActive)}");
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

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgCandidatePhaseStarted)}");

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);
    }

    public void StartKickVotePhase()
    {
        _isKickVotePhase = true;
        _phaseTimer = _plugin.Config.VotePhaseDuration;
        _kickVotes.Clear();

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgKickVotePhaseStarted)}");

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);
    }

    public void CommandJoinVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (!_plugin.IsJailbreakMap())
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgOnlyJailbreakMap)}");
            return;
        }

        if (!_isCandidatePhase)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgCandidatePhaseNotActive)}");
            return;
        }

        if (_candidates.Any(c => c.SteamID == player.SteamID))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgAlreadyCandidate)}");
            return;
        }

        if (_candidates.Count >= 5)
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgCandidateListFull)}");
            return;
        }

        _candidates.Add(player);
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgPlayerBecameCandidate), player.PlayerName, _candidates.Count))}");
    }

    private void Tick()
    {
        _phaseTimer--;

        if (_isCandidatePhase)
        {
            string html = string.Format(_plugin.Lang.HudContentVoteCandidate, _plugin.Lang.HudTitleVoteCandidate, _phaseTimer);
            foreach (var c in _candidates)
            {
                html += $"- {c.PlayerName}<br>";
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(html);
            }

            if (_phaseTimer <= 0)
            {
                if (_candidates.Count == 0)
                {
                    Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgNoCandidates)}");
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
            string html = string.Format(_plugin.Lang.HudContentVoteSelection, _plugin.Lang.HudTitleVoteSelection, _phaseTimer);
            for (int i = 0; i < _candidates.Count; i++)
            {
                int voteCount = _votes.Values.Count(v => v == _candidates[i].SteamID);
                html += $"[{i + 1}] {_candidates[i].PlayerName} - Oylar: {voteCount}<br>";
            }

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(html);
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
            string html = string.Format(_plugin.Lang.HudContentVoteKick, title, _phaseTimer);
            html += $"[1] Komutçu Kalsın - Oylar: {keepCount}<br>";
            html += $"[2] Komutçu Atılsın - Oylar: {kickCount}<br>";

            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
            {
                p.PrintToCenterHtml(html);
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
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgVotePhaseStarted)}");
    }

    private void EndVotePhase()
    {
        var winner = _candidates.OrderByDescending(c => _votes.Values.Count(v => v == c.SteamID)).FirstOrDefault();

        if (winner != null && winner.IsValid)
        {
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgVoteEndedNewWarden), winner.PlayerName))}");
            _wardenService.SetWarden(winner);
        }
        else
        {
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgVoteCancelled)}");
        }

        ResetPhase();
    }

    private void EndKickVotePhase()
    {
        int keepCount = _kickVotes.Values.Count(v => v == true);
        int kickCount = _kickVotes.Values.Count(v => v == false);

        if (kickCount > keepCount)
        {
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgKickVoteDecided), kickCount, keepCount))}");

            _plugin.AddTimer(_plugin.Config.KickVoteDelayedDuration, () =>
            {
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgKickVoteDelayedSuccess)}");
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
            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgKickVoteStayed), keepCount, kickCount))}");
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
                    player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(string.Format(ChatService.ReplaceColors(_plugin.Lang.MsgVoteCast), candidate.PlayerName))}");
                    return true;
                }
            }
            return false;
        }

        if (_isKickVotePhase)
        {
            // Komutçu kendi oylamasına katılamaz
            if (_wardenService.IsWarden(player))
            {
                player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgCannotVoteSelf)}");
                return true; // Stop them from typing 1 or 2 as normal chat
            }

            if (int.TryParse(message, out int choice))
            {
                if (choice == 1)
                {
                    _kickVotes[player.SteamID] = true;
                    player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgKickVoteKeepCast)}");
                    return true;
                }
                else if (choice == 2)
                {
                    _kickVotes[player.SteamID] = false;
                    player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Lang.MsgKickVoteKickCast)}");
                    return true;
                }
            }
            return false;
        }

        return false;
    }
}
