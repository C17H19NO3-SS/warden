using System;
using System.Collections.Generic;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Helpers;

namespace JailBreak.Services;

public class DispatcherService : IDispatcherService
{
    private readonly Dictionary<string, (string Description, CommandInfo.CommandCallback Callback)> _commands = new();


    public void RegisterCommand(string command, string description, CommandInfo.CommandCallback Callback)
    {
        _commands[command] = (description, Callback);
    }

    public IEnumerable<string> GetRegisteredCommands()
    {
        return _commands.Keys;
    }

    public void ExecuteCommand(CCSPlayerController? player, CommandInfo info)
    {
        string commandName = info.GetArg(0);
        string args = info.ArgString;
        
        LogHelper.LogDebug($"DispatcherService: Attempting to execute command '{commandName}' with args '{args}' for player '{player?.PlayerName ?? "Console"}'");
        
        if (_commands.TryGetValue(commandName, out var cmd))
        {
            try
            {
                cmd.Callback(player, info);
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Error executing command '{commandName}' for player '{player?.PlayerName ?? "N/A"}'", ex);
                player?.PrintToChat($" {ChatColors.Red}Hata: Bu komutu çalıştırırken bir sorun oluştu. Konsolunuzu kontrol edin.");
            }
        }
    }
}
