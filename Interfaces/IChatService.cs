using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.UserMessages;

namespace JailBreak.Services;

public interface IChatService
{
    HookResult OnUserMessageChat(UserMessage @event, CCSPlayerController player, string message);
}
