using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IRebelService
{
    void RegisterCommands();
    void OnPlayerDeath(EventPlayerDeath @event);
    void CommandRebels(CCSPlayerController? player, CommandInfo info);
}
