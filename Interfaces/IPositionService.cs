using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IPositionService
{
    void RegisterCommands();
    void CommandDaire(CCSPlayerController? player, CommandInfo info);
    void CommandDiz(CCSPlayerController? player, CommandInfo info);
    void OnPlayerPing(EventPlayerPing @event, CCSPlayerController player);
}
