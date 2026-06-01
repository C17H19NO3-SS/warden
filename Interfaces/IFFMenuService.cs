using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IFFMenuService
{
    void RegisterCommands();
    void OnRoundStart();
    bool HandleFFMenuChat(CCSPlayerController player, string message);
    void OpenWardenConfigMenu(CCSPlayerController warden);
}
