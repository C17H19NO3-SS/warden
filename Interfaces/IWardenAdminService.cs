using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IWardenAdminService
{
    bool IsWardenAdmin(CCSPlayerController player);
    void AddWardenAdmin(CCSPlayerController caller, CCSPlayerController target);
    void RemoveWardenAdmin(CCSPlayerController player);
    void RemoveAllWardenAdmins();
    void CommandWardenAdmin(CCSPlayerController? player, CommandInfo info);
    void CommandRemoveWardenAdmin(CCSPlayerController? player, CommandInfo info);
    void CommandTopKa(CCSPlayerController? player, CommandInfo info);
}
