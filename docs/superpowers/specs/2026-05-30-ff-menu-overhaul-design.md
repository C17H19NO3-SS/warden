# FF Menu Overhaul and CenterHtmlMenu Migration Design

## 1. Overview
The goal of this project is to modernize the Friendly Fire (FF) weapon selection system in the JailBreak plugin. The previous chat-based sequential weapon selection will be replaced by an interactive `CenterHtmlMenu` for the Warden (Komutçu) to configure the FF event, and a subsequent menu-driven selection process for the Terrorist (T) team. Additionally, all existing `ChatMenu` implementations will be migrated to `CenterHtmlMenu` to standardize the UI.

## 2. Warden FF Configuration Menu (`!ffmenu`)
When the Warden types `!ffmenu`, a `CenterHtmlMenu` is presented to them ONLY.
The menu contains the following options:

1. **[➡] Birincil Silahlar**: Opens a sub-menu listing all primary weapons from the config. The Warden can toggle each weapon ON/OFF for this specific FF round. (Default: All ON).
2. **[➡] İkincil Silahlar**: Opens a sub-menu listing all secondary weapons from the config. The Warden can toggle each ON/OFF. (Default: All ON).
3. **[🔄] Bunny Durumu**: Toggles bunny hop ON or OFF for the duration of the FF event.
4. **[⏳] FF Açılma Süresi**: Cycles through duration options for the final countdown before FF opens: 10, 20, 30, 40, 50 seconds.
5. **[▶] FF Başlat!**: Saves the current configuration, closes the Warden's menu, and starts the weapon selection process for the T team.

## 3. Terrorist Weapon Selection Workflow
To prevent UI bugs caused by overlapping HUD timers and `CenterHtmlMenu`s, the process is strictly phased:

*   **Phase 1: Weapon Selection (No HUD Timer)**
    *   Immediately after the Warden clicks "FF Başlat!", every T player receives a `CenterHtmlMenu` to select their **Primary Weapon** (only from the list of weapons the Warden left ON).
    *   Upon selecting a primary weapon, the player is immediately presented with the **Secondary Weapon** selection menu.
    *   *Constraint*: To prevent players from stalling indefinitely, a hidden internal timer (e.g., 15 seconds) will force-close the selection menus if a player doesn't choose in time, assigning them default or random weapons from the active list.

*   **Phase 2: HUD Countdown & Execution**
    *   Once ALL alive T players have made their selections (or the max selection time elapses), their old weapons are stripped, and the selected new weapons are granted.
    *   The visual HUD countdown timer (using the duration selected by the Warden, e.g., 20 seconds) begins showing on screen.
    *   When the countdown reaches 0, FF is enabled.

## 4. Chat Cleanliness
The `CS2MenuManager` relies on chat commands (like `!1`, `!2`, etc.) to register menu selections. These inputs will be intercepted and suppressed (using `HookResult.Stop` or `HookResult.Handled` in the chat hook) so they do not clutter the global chat box while players navigate the menus.

## 5. Global Migration to CenterHtmlMenu
To ensure a consistent user experience, any remaining usages of `ChatMenu` will be refactored to use `CenterHtmlMenu`.
*   Specifically, the Warden Admin selection menu (`!ka`) currently uses `ChatMenu` and will be updated.

## 6. Data Structures
*   A temporary state object/class will be needed per-round to hold the Warden's current FF configuration (active primary weapons, active secondary weapons, bunny state, countdown time) before "FF Başlat!" is clicked.

## 7. Scope Check
This design is focused entirely on the FF menu workflow and UI standardization. It does not introduce new game modes or unrelated features, keeping it well-scoped for a single implementation plan.