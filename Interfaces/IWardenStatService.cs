using CounterStrikeSharp.API.Core;
using JailBreak.Models;
using System.Threading.Tasks;

namespace JailBreak.Services;

public interface IWardenStatService
{
    Task LoadStats();
    Task LoadAdminStats();
    Task UpdateWardenStats(ulong steamId, string playerName, double seconds);
    Task UpdateWardenAdminStats(ulong steamId, string playerName, double seconds);
    List<WardenStat> GetTopWardenStats();
    List<WardenStat> GetTopWardenAdminStats();
}
