using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IVoteService
{
    void RegisterCommands();
    bool HandleVoteChat(CCSPlayerController player, string message);
    void StartKickVotePhase();
}
