using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace JailBreak.Services;

public class HudService
{
    private readonly JailBreakPlugin _plugin;

    public HudService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
    }

    public void SendHudMessage(CCSPlayerController player, string message, Color color, float duration = 15.0f)
    {
        string formattedMessage = $"<font color='{ColorTranslator.ToHtml(color)}'>{message}</font>";
        player.PrintToCenterHtml(formattedMessage);

        _plugin.AddTimer(duration, () =>
        {
            if (player.IsValid)
            {
                player.PrintToCenterHtml(""); 
            }
        });
    }

    public void SendLeftCenterHudMessage(CCSPlayerController player, string message, Color color, float duration = 15.0f)
    {
        string formattedMessage = $"<font color='{ColorTranslator.ToHtml(color)}'>{message}</font>";

        _plugin.AddTimer(duration, () =>
        {
            if (player.IsValid)
            {
                player.PrintToCenterHtml(""); 
            }
        });
    }
}
