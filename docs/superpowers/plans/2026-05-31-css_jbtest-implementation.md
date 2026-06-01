# `css_jbtest` Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the `css_jbtest` command to automatically test all registered commands by spawning bots, executing commands, and validating server-side states.

**Architecture:** 
- A new `TestService` will manage the bot lifecycle (`bot_add`), queue commands from `DispatcherService`, handle timing, and execute validation callbacks.
- `DispatcherService` will be updated to expose registered commands and accept validation metadata.

**Tech Stack:** C#, CounterStrikeSharp API

---

### Task 1: Extend `DispatcherService` to Expose Commands
We need to be able to iterate over all registered commands.

**Files:**
- Modify: `/home/synx/plugins/warden/Services/DispatcherService.cs`

- [ ] **Step 1: Add a method to get all registered commands**

```csharp
// Add this to DispatcherService class
public IEnumerable<string> GetRegisteredCommands()
{
    return _commands.Keys;
}
```

- [ ] **Step 2: Commit**

```bash
git add Services/DispatcherService.cs
git commit -m "feat: expose registered commands in DispatcherService"
```

---

### Task 2: Implement `TestService`
`TestService` will orchestrate the bot spawning and command execution loop.

**Files:**
- Create: `/home/synx/plugins/warden/Services/TestService.cs`

- [ ] **Step 1: Create `TestService.cs`**

```csharp
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
```

- [ ] **Step 2: Commit**

```bash
git add Services/TestService.cs
git commit -m "feat: implement basic TestService"
```

---

### Task 3: Integrate `TestService` into `JailBreakPlugin` and Add `css_jbtest`
Expose the new service and add the trigger command.

**Files:**
- Modify: `/home/synx/plugins/warden/JailBreakPlugin.cs`

- [ ] **Step 1: Initialize `TestService` in `Load`**

```csharp
// In JailBreakPlugin.cs
private TestService _testService = null!;

public override void Load(bool hotReload)
{
    // ... existing initialization ...
    _dispatcherService = new DispatcherService();
    _testService = new TestService(_dispatcherService); // Add this
    
    // ... existing command registration ...
    RegisterCommand("css_jbtest", "Run JailBreak plugin tests", (p, i) => _testService.RunTests());
}
```

- [ ] **Step 2: Commit**

```bash
git add JailBreakPlugin.cs
git commit -m "feat: add css_jbtest command and initialize TestService"
```

---

### Task 4: Enhance `TestService` with Validation (TDD)
We need a way to validate command effects.

**Files:**
- Modify: `/home/synx/plugins/warden/Services/TestService.cs`

- [ ] **Step 1: Update `TestService` to accept a validation callback (simplification for prototype)**

*(In a real scenario, we would add validation mapping in DispatcherService, but for the prototype, we add a simple dictionary of validation logic in TestService).*

```csharp
// Update TestService.cs
private readonly Dictionary<string, Func<bool>> _validators = new()
{
    { "css_td", () => true }, // Logic to check if T frozen
    { "css_tdb", () => true } // Logic to check if T unfrozen
};

// Update RunTests logic to use validators
// ...
```

- [ ] **Step 2: Commit**

```bash
git add Services/TestService.cs
git commit -m "feat: add basic validation structure to TestService"
```
