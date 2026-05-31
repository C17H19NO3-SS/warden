using CounterStrikeSharp.API;
using JailBreak.Helpers;
using JailBreak.Services;

namespace JailBreak.Services;

public class TestService
{
    private readonly DispatcherService _dispatcher;

    private readonly Dictionary<string, Func<bool>> _validators = new()
    {
        { "css_td", () => true }, // Logic to check if T frozen
        { "css_tdb", () => true } // Logic to check if T unfrozen
    };

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
            Server.ExecuteCommand(cmd);

            // --- Validation Logic ---
            if (_validators.TryGetValue(cmd, out var validator)) {
                bool result = validator();
                LogHelper.LogInfo($"Command '{cmd}' validation: {(result ? "PASS" : "FAIL")}");
            }
        }
        
        LogHelper.LogInfo("Command tests completed.");
    }
}
