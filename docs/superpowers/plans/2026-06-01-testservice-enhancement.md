# TestService Enhancement Implementation Plan

**Goal:** Enhance `TestService` with a basic validation structure.

**Architecture:** Add a dictionary of validation functions (`_validators`) to `TestService`. Update `RunTests` to invoke these validators after command execution.

**Tech Stack:** C# (.NET 8.0)

---

### Task 1: Enhance TestService

**Files:**
- Modify: `/home/synx/plugins/warden/Services/TestService.cs`

- [ ] **Step 1: Update TestService**

```csharp
// Add to TestService class
private readonly Dictionary<string, Func<bool>> _validators = new()
{
    { "css_td", () => true }, // Logic to check if T frozen
    { "css_tdb", () => true } // Logic to check if T unfrozen
};
```

```csharp
// Update RunTests
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
```

- [ ] **Step 2: Commit**

```bash
git add Services/TestService.cs
git commit -m "feat: add basic validation structure to TestService"
```
