using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text;
using JailBreak.Models;

namespace JailBreak.Services;

public enum HudPriority
{
    Default = 0,
    Announcement = 1,
    GameStatus = 2,
    Menu = 3
}

public class HudContent
{
    public string Content { get; set; } = "";
    public HudPriority Priority { get; set; }
    public DateTime? Expiry { get; set; }
}

public class HudManager : IHudManager
{
    private readonly JailBreakPlugin _plugin;
    private readonly Dictionary<ulong, Dictionary<string, HudContent>> _playerHudStates = new();
    private readonly Dictionary<ulong, string> _lastSentContent = new();
    private readonly Dictionary<ulong, DateTime> _lastUpdateTime = new();
    private const double UpdateIntervalMs = 250;

    public HudManager(JailBreakPlugin plugin)
    {
        _plugin = plugin;
    }

    public void Update() => OnTick();

    public void OnTick()
    {
        var now = DateTime.Now;
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            if (_lastUpdateTime.TryGetValue(player.SteamID, out var lastUpdate) && (now - lastUpdate).TotalMilliseconds < UpdateIntervalMs)
                continue;

            UpdatePlayerHud(player);
            _lastUpdateTime[player.SteamID] = now;
        }
    }

    public void SetHudText(string text, float duration)
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            SetHudContent(player, "LegacyText", text, HudPriority.Announcement, duration);
        }
    }

    public void ClearHud()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            ClearHud(player);
        }
    }

    public void SetHudContent(CCSPlayerController player, string id, string content, HudPriority priority, float duration = 0)
    {
        if (!_playerHudStates.ContainsKey(player.SteamID))
            _playerHudStates[player.SteamID] = new Dictionary<string, HudContent>();

        _playerHudStates[player.SteamID][id] = new HudContent
        {
            Content = content,
            Priority = priority,
            Expiry = duration > 0 ? DateTime.Now.AddSeconds(duration) : null
        };
    }

    public void ClearHud(CCSPlayerController player, string? id = null)
    {
        if (!_playerHudStates.ContainsKey(player.SteamID)) return;

        if (id == null)
            _playerHudStates[player.SteamID].Clear();
        else
            _playerHudStates[player.SteamID].Remove(id);
    }

    private void UpdatePlayerHud(CCSPlayerController player)
    {
        if (!_playerHudStates.ContainsKey(player.SteamID)) return;

        var now = DateTime.Now;
        var activeContents = _playerHudStates[player.SteamID]
            .Where(kvp => kvp.Value.Expiry == null || kvp.Value.Expiry > now)
            .OrderByDescending(kvp => kvp.Value.Priority)
            .ToList();

        if (activeContents.Count == 0)
        {
            if (_lastSentContent.ContainsKey(player.SteamID))
            {
                player.PrintToCenterHtml("");
                _lastSentContent.Remove(player.SteamID);
            }
            return;
        }

        var sb = new StringBuilder();
        foreach (var content in activeContents)
        {
            sb.Append(content.Value.Content);
            sb.Append("<br>");
        }

        string fullContent = sb.ToString();
        if (_lastSentContent.TryGetValue(player.SteamID, out var lastSent) && lastSent == fullContent)
            return;

        player.PrintToCenterHtml(fullContent);
        _lastSentContent[player.SteamID] = fullContent;
    }
}
