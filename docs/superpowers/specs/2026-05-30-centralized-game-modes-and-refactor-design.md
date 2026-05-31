# Centralized Game Management and Refactoring Design

## 1. Overview
The goal is to centralize game mode logic (Boxing, Hide-and-Seek) and provide a unified administrative interface for the Warden and Root admins. This also involves refactoring redundant code patterns (permissions, message formatting) into shared utilities to improve maintainability.

## 2. Shared Utilities & Refactoring
To eliminate code duplication, the following patterns will be standardized:

*   **Centralized Permissions**: `WardenService.HasPermission(CCSPlayerController player)` will be the single source of truth for checking if a player is the Warden, a Warden Admin, or a high-level Administrator. All other services (`FFMenuService`, `GameManagerService`, etc.) will reference this method.
*   **Message Formatting**: A shared helper (e.g., in `ChatService` or a dedicated `PluginHelper`) will be used to handle `ReplaceColors` and prepending the `ChatPrefix`.
*   **Default Durations**: The default duration for FF and Boxing countdowns will be standardized to 30 seconds.

## 3. GameManagerService
A new service responsible for transient "Game Modes".

### 3.1 Boxing Mode (`!b`, `!box`)
*   **Logic**: A simplified version of FF with a mandatory 30s duration (or menu selection).
*   **UI**: A `CenterHtmlMenu` to confirm or select duration (10s increments).
*   **Execution**: Enables FF (`mp_teammates_are_enemies 1`) for the duration and disables it upon completion.

### 3.2 Hide-and-Seek (Saklambaç) Mode (`!saklambac <saniye>`)
*   **Initialization**: 
    1. Teleports all CT players back to their spawn points.
    2. Rotates CT players 180 degrees (facing away from the field).
    3. Freezes CT players (`MoveType.None`).
    4. Blinds CT players using a periodic `ScreenFade` user message.
*   **HUD**: Displays a countdown to all players.
*   **Completion**:
    1. Unfreezes and restores vision to CTs.
    2. Freezes all T players (`FreezeService.FreezeAll()`).

## 4. Warden Control Menu (`!k`, `!kommenu`)
A master `CenterHtmlMenu` available to the Warden/Admins.
*   **Options**:
    *   **Kapıları Aç**: Triggers `IseliService.QuickOpen`.
    *   **Boks Modu**: Triggers `GameManagerService.OpenBoxMenu`.
    *   **Saklambaç**: Triggers `GameManagerService.OpenSaklambacMenu`.
    *   **FF Menüsü**: Triggers `FFMenuService.OpenWardenConfigMenu`.
    *   **Pozisyonlar**: Sub-menu for `!daire`, `!diz`.
    *   **Canlandır (Herkes)**: Triggers `UtilityService.CommandAf`.
    *   **Bunny/Marker**: Quick toggles for plugin features.

## 5. Broadcast & Administrative Commands

### 5.1 Broadcast Commands
*   **`!msay <msg>`**: Displays a large message in the center using a Menu-style UI (Top of screen or large center).
*   **`!csay <msg>`**: Displays the message using the standard `PrintToCenterHtml` helper.
*   **`!hsay <msg>`**: Displays the message in the HUD (Hint/Top) area.

### 5.2 Fake Say (`!fsay <target> <message>`)
*   **Permission**: `@css/root` only.
*   **Logic**: Finds the target player and forces them to say the provided message in chat.

### 5.3 Revive (`!rev <target>`)
*   **Targets**: Supports `@t`, `@ct`, `@all`, or specific names.
*   **Logic**: Respawns the target player(s) if they are dead.

## 6. Implementation Scope
*   Update `WardenService.cs` with the centralized `HasPermission`.
*   Create `Services/GameManagerService.cs`.
*   Update `Services/UtilityService.cs` or a new `BroadcastService` for Say commands.
*   Refactor `FFMenuService.cs` to use the centralized permission and duration defaults.
*   Add `!k` command registration in `JailBreakPlugin.cs`.
