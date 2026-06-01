using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.UserMessages;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class ChatService : IChatService
{
    private readonly JailBreakPlugin _plugin;
    private readonly IWardenService _wardenService;

    public ChatService(JailBreakPlugin plugin, IWardenService wardenService)
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

            string formattedMessage = $" {tag} {nameColor}{player.PlayerName}\x01 : {chatColor}{message}";

            @event.SetString("messagename", formattedMessage);

            return HookResult.Changed;
        }

        return HookResult.Continue;
    }
}
