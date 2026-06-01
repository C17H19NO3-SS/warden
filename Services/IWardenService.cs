using CounterStrikeSharp.API.Core;

namespace JailBreak.Services;

public interface IWardenService
{
    void LoadStats();
    bool HasPermission(CCSPlayerController? player, string? requiredFlag = null);
    void OnRoundStart();
    void OnClientDisconnect(int playerSlot);
    void SetWarden(CCSPlayerController player);
    void RemoveWarden();
}
