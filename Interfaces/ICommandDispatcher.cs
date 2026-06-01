using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface ICommandDispatcher
{
    void RegisterCommand(string command, string description, CommandInfo.CommandCallback callback);

    void ExecuteCommand(CCSPlayerController? player, CommandInfo info);
}
