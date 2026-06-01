using CounterStrikeSharp.API.Core;
using JailBreak.Models;

namespace JailBreak.Services;

public interface IHudManager
{
    void SetHudText(string text, float duration);
    void ClearHud();
    void Update();
    void OnTick();
    // Supporting different ways services might want to show content
    void SetHudContent(CCSPlayerController player, string id, string content, HudPriority priority, float duration = 0);
    void ClearHud(CCSPlayerController player, string? id = null);
}
