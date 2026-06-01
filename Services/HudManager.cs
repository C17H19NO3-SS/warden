using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text;

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
        // TODO: Implement centralized HUD text setting
    }

    public void ClearHud()
    {
        // TODO: Implement centralized HUD clearing
    }
    
    private void UpdatePlayerHud(CCSPlayerController player)
    {
        if (!_playerHudStates.TryGetValue(player.SteamID, out var states)) return;

        var now = DateTime.Now;
        var expiredKeys = states.Where(kvp => kvp.Value.Expiry.HasValue && kvp.Value.Expiry.Value < now).Select(kvp => kvp.Key).ToList();
        foreach (var key in expiredKeys) states.Remove(key);

        if (states.Count == 0)
        {
            if (_lastSentContent.TryGetValue(player.SteamID, out var last) && !string.IsNullOrEmpty(last))
            {
                player.PrintToCenterHtml("");
                _lastSentContent[player.SteamID] = "";
            }
            return;
        }

        var maxPriority = states.Values.Max(s => s.Priority);
        var activeItems = states.Values.Where(s => s.Priority == maxPriority).ToList();

        StringBuilder sb = new();
        foreach (var item in activeItems)
        {
            if (string.IsNullOrEmpty(item.Content)) continue;
            sb.Append(item.Content).Append("<br>");
        }

        string finalHud = sb.ToString();

        if (!_lastSentContent.TryGetValue(player.SteamID, out var lastContent) || lastContent != finalHud)
        {
            player.PrintToCenterHtml(finalHud);
            _lastSentContent[player.SteamID] = finalHud;
        }
    }

    public void SetHudContent(CCSPlayerController player, string id, string content, HudPriority priority, float duration = 0)
    {
        if (player == null || !player.IsValid) return;

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
        if (player == null || !player.IsValid) return;
        if (!_playerHudStates.TryGetValue(player.SteamID, out var states)) return;
        
        if (id == null) states.Clear();
        else states.Remove(id);
    }
}
