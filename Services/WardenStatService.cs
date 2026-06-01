using CounterStrikeSharp.API.Core;
using JailBreak.Models;
using JailBreak.Helpers;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JailBreak.Services;

public class WardenStatService : IWardenStatService
{
    private List<WardenStat> _wardenStats = new();
    private List<WardenStat> _wardenAdminStats = new();
    private readonly string _statsPath;
    private readonly string _adminStatsPath;
    private readonly SemaphoreSlim _statsSemaphore = new(1, 1);
    private readonly SemaphoreSlim _adminStatsSemaphore = new(1, 1);

    public WardenStatService(string moduleDirectory)
    {
        _statsPath = Path.Combine(moduleDirectory, "config/WardenStats.json");
        _adminStatsPath = Path.Combine(moduleDirectory, "config/WardenAdminStats.json");
        _ = LoadStats();
        _ = LoadAdminStats();
    }

    public async Task LoadStats()
    {
        LogHelper.LogDebug("WardenStatService: Loading warden stats...");
        await _statsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (File.Exists(_statsPath))
            {
                string json = await File.ReadAllTextAsync(_statsPath).ConfigureAwait(false);
                _wardenStats = JsonSerializer.Deserialize<List<WardenStat>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error loading warden stats.", ex);
        }
        finally
        {
            _statsSemaphore.Release();
        }
    }

    public async Task LoadAdminStats()
    {
        LogHelper.LogDebug("WardenStatService: Loading warden admin stats...");
        await _adminStatsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (File.Exists(_adminStatsPath))
            {
                string json = await File.ReadAllTextAsync(_adminStatsPath).ConfigureAwait(false);
                _wardenAdminStats = JsonSerializer.Deserialize<List<WardenStat>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error loading warden admin stats.", ex);
        }
        finally
        {
            _adminStatsSemaphore.Release();
        }
    }

    private async Task SaveStatsAsync()
    {
        LogHelper.LogDebug("WardenStatService: Saving warden stats...");
        await _statsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await SaveStatsInternalAsync().ConfigureAwait(false);
        }
        finally
        {
            _statsSemaphore.Release();
        }
    }

    private async Task SaveStatsInternalAsync()
    {
        string dir = Path.GetDirectoryName(_statsPath) ?? "";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string json = JsonSerializer.Serialize(_wardenStats, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_statsPath, json).ConfigureAwait(false);
    }

    private async Task SaveAdminStatsAsync()
    {
        LogHelper.LogDebug("WardenStatService: Saving warden admin stats...");
        await _adminStatsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await SaveAdminStatsInternalAsync().ConfigureAwait(false);
        }
        finally
        {
            _adminStatsSemaphore.Release();
        }
    }

    private async Task SaveAdminStatsInternalAsync()
    {
        string dir = Path.GetDirectoryName(_adminStatsPath) ?? "";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string json = JsonSerializer.Serialize(_wardenAdminStats, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_adminStatsPath, json).ConfigureAwait(false);
    }

    public async Task UpdateWardenStats(ulong steamId, string playerName, double seconds)
    {
        await _statsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var stat = _wardenStats.FirstOrDefault(s => s.SteamID == steamId);
            if (stat == null)
            {
                stat = new WardenStat
                {
                    SteamID = steamId,
                    PlayerName = playerName,
                    TotalTimeSeconds = 0
                };
                _wardenStats.Add(stat);
            }
            else
            {
                stat.PlayerName = playerName;
            }

            stat.TotalTimeSeconds += seconds;
            await SaveStatsInternalAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error updating warden stats.", ex);
        }
        finally
        {
            _statsSemaphore.Release();
        }
    }

    public async Task UpdateWardenAdminStats(ulong steamId, string playerName, double seconds)
    {
        await _adminStatsSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var stat = _wardenAdminStats.FirstOrDefault(s => s.SteamID == steamId);
            if (stat == null)
            {
                stat = new WardenStat
                {
                    SteamID = steamId,
                    PlayerName = playerName,
                    TotalTimeSeconds = 0
                };
                _wardenAdminStats.Add(stat);
            }
            else
            {
                stat.PlayerName = playerName;
            }

            stat.TotalTimeSeconds += seconds;
            await SaveAdminStatsInternalAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error updating warden admin stats.", ex);
        }
        finally
        {
            _adminStatsSemaphore.Release();
        }
    }

    public List<WardenStat> GetTopWardenStats()
    {
        _statsSemaphore.Wait();
        try
        {
            return _wardenStats.OrderByDescending(s => s.TotalTimeSeconds).ToList();
        }
        finally
        {
            _statsSemaphore.Release();
        }
    }

    public List<WardenStat> GetTopWardenAdminStats()
    {
        _adminStatsSemaphore.Wait();
        try
        {
            return _wardenAdminStats.OrderByDescending(s => s.TotalTimeSeconds).ToList();
        }
        finally
        {
            _adminStatsSemaphore.Release();
        }
    }
}
