using CounterStrikeSharp.API;
using JailBreak.Helpers;
using JailBreak.Services;

namespace JailBreak.Services;

public class TestService
{
    private readonly DispatcherService _dispatcher;

    public TestService(DispatcherService dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void RunTests()
    {
        LogHelper.LogInfo("Starting JailBreak command tests...");
        // 1. Spawn bots
        Server.ExecuteCommand("bot_kick");
        Server.ExecuteCommand("bot_add_t");
        Server.ExecuteCommand("bot_add_ct");

        // 2. Iterate commands
        var commands = _dispatcher.GetRegisteredCommands();
        foreach (var cmd in commands)
        {
            LogHelper.LogInfo($"Testing command: {cmd}");
            // Simplified execution for now
            Server.ExecuteCommand(cmd);
        }
        
        LogHelper.LogInfo("Command tests completed.");
    }
}
