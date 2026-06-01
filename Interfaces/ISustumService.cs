using CounterStrikeSharp.API.Core;

namespace JailBreak.Services;

public interface ISustumService
{
    void RegisterCommands();
    bool HandleSustumChat(CCSPlayerController player, string message);
}
