using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.UserMessages;
using JailBreak.Helpers;

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

            tag = PluginHelper.ReplaceColors(tag);
            nameColor = PluginHelper.ReplaceColors(nameColor);
            chatColor = PluginHelper.ReplaceColors(chatColor);

            // Reconstruct the chat line similar to cs2-tags
            string formattedMessage = $" {tag} {nameColor}{player.PlayerName}\x01 : {chatColor}{message}";

            // Set the constructed string as the new message name
            @event.SetString("messagename", formattedMessage);

            return HookResult.Changed;
        }

        return HookResult.Continue;
    }
}
