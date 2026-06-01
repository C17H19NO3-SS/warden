using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Config;
using JailBreak.Helpers;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace JailBreak.Services;

public class ConfigService : IConfigService
{
    private readonly JailBreakPlugin _plugin;

    public PluginConfig Config { get; set; } = new();
    public LangConfig Lang { get; set; } = new();

    public ConfigService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
    }

    private string ConfigPath => Path.Combine(_plugin.ModuleDirectory, "../../configs/plugins/JailBreak/JailBreak.json");
    private string LangPath => Path.Combine(_plugin.ModuleDirectory, "../../configs/plugins/JailBreak/lang.json");

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
        SaveConfig();
        LoadOrCreateLang();
    }

    public void ReloadConfig()
    {
        if (!File.Exists(ConfigPath)) return;

        string jsonString = File.ReadAllText(ConfigPath);
        var newConfig = JsonSerializer.Deserialize<PluginConfig>(jsonString);
        if (newConfig == null) return;

        Config = newConfig;
        LoadOrCreateLang();
        _plugin.Config = Config;
        _plugin.Lang = Lang;
    }

    public void RegisterCommands()
    {
        _plugin.RegisterCommand("css_reloadconfig", "Config dosyasını yeniden yükle", CommandReloadConfig);
    }

    private void CommandReloadConfig(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !_plugin.WardenService.HasPermission(player, "@css/root"))
        {
            return;
        }

        try
        {
            ReloadConfig();
            LogHelper.Initialize(Config, _plugin.ModuleDirectory);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error reloading config.", ex);
            player?.PrintToChat(PluginHelper.FormatChat(Config.ChatPrefix, $"{ChatColors.Red}Config yüklenirken hata oluştu: {ex.Message}"));
            return;
        }

        string reloadMsg = PluginHelper.FormatChat(Config.ChatPrefix, Lang.MsgConfigReloaded);
        if (player != null) player.PrintToChat(reloadMsg);
        else info.ReplyToCommand(reloadMsg);
    }

    private void SaveConfig()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        EnsureDirectoryExists(ConfigPath);
        string json = JsonSerializer.Serialize(Config, options);
        File.WriteAllText(ConfigPath, json);
    }

    private void LoadOrCreateLang()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (File.Exists(LangPath))
        {
            string langJson = File.ReadAllText(LangPath);
            var loadedLang = JsonSerializer.Deserialize<LangConfig>(langJson);
            if (loadedLang != null)
            {
                Lang = loadedLang;
                string updatedLangJson = JsonSerializer.Serialize(Lang, options);
                File.WriteAllText(LangPath, updatedLangJson);
                return;
            }
        }

        EnsureDirectoryExists(LangPath);
        string defaultLangJson = JsonSerializer.Serialize(Lang, options);
        File.WriteAllText(LangPath, defaultLangJson);
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory)) return;
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
    }
}
