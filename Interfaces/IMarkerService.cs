using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IMarkerService
{
    void RegisterCommands();
    void OpenMarkerMenu(CCSPlayerController? player, CommandInfo? info = null);
    void OnRoundStart();
    void OnPlayerPing(EventPlayerPing @event, CCSPlayerController player);
}
