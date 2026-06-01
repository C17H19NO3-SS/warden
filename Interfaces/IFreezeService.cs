using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace JailBreak.Services;

public interface IFreezeService
{
    void RegisterCommands();
    void OnRoundStart();
    void FreezeAll(bool silent = false);
    void UnfreezeAll(bool silent = false);
    void ApplyFreezeState();
    bool IsFrozen { get; }
}
