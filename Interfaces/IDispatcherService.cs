using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IDispatcherService : ICommandDispatcher
{
    IEnumerable<string> GetRegisteredCommands();
}
