# Refactor Command Handlers to PluginHelper.ReplyToCommand

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update all `Command*` methods in the specified files to use `PluginHelper.ReplyToCommand` for feedback, ensuring consistent player/console handling.

**Architecture:**
1.  Update `Helpers/PluginHelper.cs` to support `params object[] args` in `ReplyToCommand` to allow for clean formatting before output.
2.  Refactor all `Command*` methods to use this new signature.

**Tech Stack:** C#, .NET 8.0, CounterStrikeSharp API

---

### Task 1: Update PluginHelper.ReplyToCommand

**Files:**
- Modify: `Helpers/PluginHelper.cs`

- [ ] **Step 1: Update PluginHelper.cs**
Update `ReplyToCommand` to support `params object[] args`.

### Task 2: Refactor Services

**Files:**
- Modify: `Services/VoteService.cs`, `Services/FFMenuService.cs`, `Services/IseliService.cs`, `Services/LastRequestService.cs`, `Services/PositionService.cs`, `Services/FreezeService.cs`, `Services/RebelService.cs`, `Services/SustumService.cs`, `Services/UtilityService.cs`

- [ ] **Step 1: Update all `Command*` methods in the services to use `ReplyToCommand`.**

### Task 3: Build and Verify

- [ ] **Step 1: Build the project**
Run: `dotnet build -c Release`
Expected: Success

- [ ] **Step 2: Check for any regressions (if possible)**
(No tests requested, just verification of build success).
