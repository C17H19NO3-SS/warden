using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace JailBreak.Services;

public interface ILastRequestService
{
    void RegisterCommands();
    void OnRoundStart();
    void OnWeaponFire(EventWeaponFire @event);
    void OnPlayerDeath(EventPlayerDeath @event);
    void CommandSonaKalan(CCSPlayerController? player, CommandInfo info);
    void CommandSonSec(CCSPlayerController? player, CommandInfo info);
}
