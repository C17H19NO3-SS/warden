using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IIseliService
{
    void RegisterCommands();
    void OnRoundStart();
    void CommandIseli(CCSPlayerController? player, CommandInfo info);
    void CommandQuickIseli(CCSPlayerController? player, CommandInfo info);
    void QuickOpen(CCSPlayerController player);
}
