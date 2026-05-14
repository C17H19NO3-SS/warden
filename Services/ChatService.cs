using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.UserMessages;

namespace JailBreak.Services;

public class ChatService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;

    public ChatService(JailBreakPlugin plugin, WardenService wardenService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
    }

    public HookResult OnUserMessageChat(UserMessage @event, CCSPlayerController player, string message)
    {
        bool isWarden = _wardenService.IsWarden(player);
        bool isWardenAdmin = _wardenService.IsWardenAdmin(player);

        if (isWarden || isWardenAdmin)
        {
            string tag = isWarden ? _plugin.Config.WardenTag : _plugin.Config.WardenAdminTag;
            string nameColor = isWarden ? _plugin.Config.WardenNameColor : _plugin.Config.WardenAdminNameColor;
            string chatColor = isWarden ? _plugin.Config.WardenChatColor : _plugin.Config.WardenAdminChatColor;

            tag = ReplaceColors(tag);
            nameColor = ReplaceColors(nameColor);
            chatColor = ReplaceColors(chatColor);

            // Reconstruct the chat line similar to cs2-tags
            string formattedMessage = $" {tag} {nameColor}{player.PlayerName}\x01 : {chatColor}{message}";

            // Set the constructed string as the new message name
            @event.SetString("messagename", formattedMessage);

            return HookResult.Changed;
        }

        return HookResult.Continue;
    }

    public static string ReplaceColors(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return input.Replace("{Default}", $"{ChatColors.Default}")
                    .Replace("{White}", $"{ChatColors.White}")
                    .Replace("{DarkRed}", $"{ChatColors.DarkRed}")
                    .Replace("{Purple}", $"{ChatColors.Purple}")
                    .Replace("{Green}", $"{ChatColors.Green}")
                    .Replace("{LightYellow}", $"{ChatColors.LightYellow}")
                    .Replace("{LightBlue}", $"{ChatColors.LightBlue}")
                    .Replace("{Olive}", $"{ChatColors.Olive}")
                    .Replace("{Lime}", $"{ChatColors.Lime}")
                    .Replace("{Red}", $"{ChatColors.Red}")
                    .Replace("{LightPurple}", $"{ChatColors.LightPurple}")
                    .Replace("{Gray}", $"{ChatColors.Grey}")
                    .Replace("{Grey}", $"{ChatColors.Grey}")
                    .Replace("{Yellow}", $"{ChatColors.Yellow}")
                    .Replace("{Gold}", $"{ChatColors.Gold}")
                    .Replace("{Silver}", $"{ChatColors.Silver}")
                    .Replace("{Blue}", $"{ChatColors.Blue}")
                    .Replace("{DarkBlue}", $"{ChatColors.DarkBlue}")
                    .Replace("{BlueGrey}", $"{ChatColors.BlueGrey}")
                    .Replace("{Magenta}", $"{ChatColors.Magenta}")
                    .Replace("{LightRed}", $"{ChatColors.LightRed}");
    }
}
