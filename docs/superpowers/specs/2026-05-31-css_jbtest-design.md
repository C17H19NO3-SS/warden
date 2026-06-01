# `css_jbtest` Command Implementation Design

## Overview
The `css_jbtest` command will be a centralized test manager within the `JailBreak` plugin to automatically verify the functionality of all registered commands by spawning bots, executing the commands, and validating their effects.

## Architecture
- **`TestService` (New)**: A dedicated service responsible for:
  - Spawning bots via `bot_add` (Terrorist and Counter-Terrorist teams).
  - Managing the execution queue of registered commands.
  - Implementing waiting periods between tests to allow for command processing.
  - Aggregating and reporting test results.
- **`DispatcherService` (Extended)**: 
  - Will be enhanced to allow metadata for commands (e.g., validation logic, required arguments).
  - Will expose registered commands to `TestService`.

## Test Execution Flow
1. **Preparation**: `TestService` clears existing bots and spawns new T/CT bots.
2. **Iteration**: Retrieves all registered commands from `DispatcherService`.
3. **Execution & Validation**:
   - Executes each command on the bot(s).
   - Waits (e.g., 500ms) for command effect.
   - Runs validation logic (e.g., checking player status, freeze state).
   - Logs result (Pass/Fail).
4. **Reporting**: Outputs the final test report to the console/chat.

## Assumptions & Risk Management
- **Command Parameters**: Default, safe arguments will be provided for commands requiring input.
- **Bot Behavior**: Acknowledged that bot behavior can be unpredictable; validation will focus on server-side state (e.g., `PawnIsAlive`, `FreezeStatus`) rather than client-side visuals.
