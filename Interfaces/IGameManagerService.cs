using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IGameManagerService
{
    void RegisterCommands();
    void OnRoundStart();
    void OpenBoxMenu(CCSPlayerController player);
    void CommandOpenBoxMenu(CCSPlayerController? player, CommandInfo info);
    void CommandSaklambac(CCSPlayerController? player, CommandInfo info);
    void StartBox(int duration);
    void StartSaklambac(int duration);
}
