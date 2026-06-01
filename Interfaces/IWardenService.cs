using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IWardenService
{
    void RegisterCommands();
    Task LoadStats();
    bool HasPermission(CCSPlayerController? player, string? requiredFlag = null);
    void OnRoundStart();
    void OnClientDisconnect(int playerSlot);
    void SetWarden(CCSPlayerController player);
    void RemoveWarden();
    
    CCSPlayerController? CurrentWarden { get; }
    bool IsWarden(CCSPlayerController player);
    bool IsWardenAdmin(CCSPlayerController player);
    bool IsGodMode(ulong steamId);
    
    void CommandBecomeWarden(CCSPlayerController? player, CommandInfo info);
    void CommandUnwarden(CCSPlayerController? player, CommandInfo info);
    void CommandKomMenu(CCSPlayerController? player, CommandInfo info);
}
