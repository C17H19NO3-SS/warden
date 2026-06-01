using JailBreak.Config;

namespace JailBreak.Services;

public interface IConfigService
{
    PluginConfig Config { get; set; }
    LangConfig Lang { get; set; }

    void OnConfigParsed(PluginConfig config);
    void ReloadConfig();
    void RegisterCommands();
}
