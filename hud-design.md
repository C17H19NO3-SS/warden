# JailBreak HUD Design Standards

## Color Palette
- **Primary (Title):** Gold (`#FFD700`) - Used for headers and main titles.
- **Time/Countdown:** Red (`#FF0000`) - Used for all countdown timers and critical info.
- **Success/Word:** Green (`#00FF00`) - Used for target words, winners, and active states.
- **Instruction:** Silver (`#C0C0C0`) - Used for keybind instructions and secondary info.
- **System:** Cyan (`#00FFFF`) - Used for system status and formations.

## HUD Structure
All center-html messages should follow this template:
```html
<font color='gold' size='20'><b>--- {TITLE} ---</b></font><br>
{CONTENT}<br>
<font color='#C0C0C0' size='14'>{INSTRUCTION}</font>
```

## Freeze Indicator
Instead of blue glow (which was hard to see), frozen players will be indicated by:
1. A clear HUD message "T TAKIMI DONDURULDU".
2. Movement speed set to 0 and MoveType to None.
3. (Optional) If visual indication is needed, use a slight red tint or model scale change, but HUD is the primary source of truth.
