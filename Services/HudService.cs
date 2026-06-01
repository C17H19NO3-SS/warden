using CounterStrikeSharp.API.Core;
using System.Drawing;

namespace JailBreak.Services;

public class HudService : IHudService
{
    private readonly JailBreakPlugin _plugin;

    public HudService(JailBreakPlugin plugin)
    {
        _plugin = plugin;
    }

    public void SendHudMessage(CCSPlayerController player, string message, Color color, float duration = 15.0f)
    {
        if (player == null || !player.IsValid) return;
        
        string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        string htmlMessage = $"<font color='{hexColor}'>{message}</font>";
        
        player.PrintToCenterHtml(htmlMessage);
    }

    public void SendLeftCenterHudMessage(CCSPlayerController player, string message, Color color, float duration = 15.0f)
    {
        if (player == null || !player.IsValid) return;

        string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        string htmlMessage = $"<font color='{hexColor}'>{message}</font>";

        player.PrintToCenterHtml(htmlMessage);
    }

    public void OnTick()
    {
        // No periodic HUD work needed currently; method exists to satisfy IHudService contract.
    }
}
