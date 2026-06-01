using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using JailBreak.Models;
using System.Text.Json;
using System.Text.Encodings.Web;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class RebelService : IRebelService
{
    private readonly JailBreakPlugin _plugin;
    private List<RebelData> _rebels = new();
    private readonly string _filePath;

    public RebelService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
        _filePath = Path.Combine(_plugin.ModuleDirectory, "../../configs/plugins/JailBreak/rebels.json");
        LoadData();
    }

    public void OnPlayerDeath(EventPlayerDeath @event)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;

        if (victim == null || !victim.IsValid || attacker == null || !attacker.IsValid) return;

        if (victim.Team == CsTeam.CounterTerrorist && attacker.Team == CsTeam.Terrorist)
        {
            UpdateRebelScore(attacker);
        }
    }

    private void UpdateRebelScore(CCSPlayerController player)
    {
        var rebel = _rebels.FirstOrDefault(r => r.SteamID == player.SteamID);
        if (rebel == null)
        {
            rebel = new RebelData
            {
                SteamID = player.SteamID,
                PlayerName = player.PlayerName,
                KillCount = 0
            };
            _rebels.Add(rebel);
        }

        rebel.KillCount++;
        SaveData();
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_isyancılar", "İsyancı listesini göster", CommandRebels);
        _plugin.RegisterCommand("css_isyancilar", "İsyancı listesini göster", CommandRebels);
    }

    public void CommandRebels(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!_plugin.IsJailbreakMap()) return;

        int page = 1;
        string arg = info.GetArg(1);
        if (!string.IsNullOrEmpty(arg) && int.TryParse(arg, out int p))
        {
            page = p;
        }

        if (page < 1) page = 1;

        ShowRebelHud(player, page);
    }

    private void ShowRebelHud(CCSPlayerController player, int page)
    {
        var sortedRebels = _rebels.Where(r => r.KillCount > 0).OrderByDescending(r => r.KillCount).ToList();
        
        int itemsPerPage = 5;
        int totalPages = (int)Math.Ceiling(sortedRebels.Count / (double)itemsPerPage);
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        int start = (page - 1) * itemsPerPage;
        int end = Math.Min(start + itemsPerPage, sortedRebels.Count);

        string title = "İSYANCI LİSTESİ";
        string content = $"Sayfa: <font color='{PluginHelper.ColorTitle}'>{page}/{totalPages}</font><br><br>";

        if (sortedRebels.Count == 0)
        {
            content += "Henüz isyancı bulunmuyor.";
        }
        else
        {
            for (int i = start; i < end; i++)
            {
                content += $"{i + 1}. <font color='{PluginHelper.ColorTime}'>{sortedRebels[i].PlayerName}</font> - <font color='{PluginHelper.ColorSuccess}'>{sortedRebels[i].KillCount} İsyan</font><br>";
            }
        }

        string instruction = totalPages > 1 ? $"!isyancılar <sayfa> (1-{totalPages})" : "";
        player.PrintToCenterHtml(PluginHelper.FormatHud(title, content, instruction));
    }

    private void LoadData()
    {
        LogHelper.LogDebug("RebelService: Loading rebel data...");
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _rebels = JsonSerializer.Deserialize<List<RebelData>>(json) ?? new List<RebelData>();
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Rebel data load error.", ex);
            _rebels = new List<RebelData>();
        }
    }

    private void SaveData()
    {
        LogHelper.LogDebug("RebelService: Saving rebel data...");
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(_rebels, options);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Rebel data save error.", ex);
        }
    }
}
