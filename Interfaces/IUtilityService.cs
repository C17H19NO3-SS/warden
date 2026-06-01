using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IUtilityService
{
    void RegisterCommands();
    void OnRoundStart();
    void ResetKacCmRecords();
    void CommandAf(CCSPlayerController? player, CommandInfo info);
}
