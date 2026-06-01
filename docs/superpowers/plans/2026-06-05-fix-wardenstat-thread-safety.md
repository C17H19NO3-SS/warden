# Fix Thread Safety and Race Conditions in WardenStatService Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix thread safety issues and race conditions in `WardenStatService` by wrapping shared state access in semaphores, extracting player data on the main thread, and using `.ConfigureAwait(false)`.

**Architecture:** Use `SemaphoreSlim` to protect both read and write operations on the shared stats lists. Modify method signatures to accept primitive types instead of `CCSPlayerController` to avoid unsafe cross-thread access.

**Tech Stack:** C#, .NET 8, CounterStrikeSharp

---

### Task 1: Update IWardenStatService Interface

**Files:**
- Modify: `Services/IWardenStatService.cs`

- [ ] **Step 1: Update method signatures**

Change `UpdateWardenStats` and `UpdateWardenAdminStats` to accept `ulong steamId` and `string playerName`.

```csharp
public interface IWardenStatService
{
    Task LoadStats();
    Task LoadAdminStats();
    Task UpdateWardenStats(ulong steamId, string playerName, double seconds);
    Task UpdateWardenAdminStats(ulong steamId, string playerName, double seconds);
    List<WardenStat> GetTopWardenStats();
    List<WardenStat> GetTopWardenAdminStats();
}
```

- [ ] **Step 2: Commit**

```bash
git add Services/IWardenStatService.cs
git commit -m "refactor: update IWardenStatService signatures for thread safety"
```

### Task 2: Refactor WardenStatService for Thread Safety

**Files:**
- Modify: `Services/WardenStatService.cs`

- [ ] **Step 1: Update methods with semaphores and primitive parameters**

Wrap the entire logic of `UpdateWardenStats` and `UpdateWardenAdminStats` inside the semaphores. Ensure `SaveStats` calls don't double-acquire if we refactor them (or just call `SaveStatsInternal` without semaphore). Actually, `SaveStats` already acquires the semaphore. I should make internal versions of save that don't acquire.

Wait, if I call `SaveStats` from `UpdateWardenStats` while holding the semaphore, it will deadlock.

Correct approach:
- Create `SaveStatsAsync` and `SaveAdminStatsAsync` that assume semaphore is NOT held.
- Create private `SaveStatsInternalAsync` and `SaveAdminStatsInternalAsync` that assume semaphore IS held.

- [ ] **Step 2: Add .ConfigureAwait(false) to all await calls.**

- [ ] **Step 3: Update GetTopWardenStats methods to use semaphores and return copies.**

```csharp
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
```

- [ ] **Step 4: Commit**

```bash
git add Services/WardenStatService.cs
git commit -m "refactor: implement thread safety in WardenStatService"
```

### Task 3: Update Callers (WardenService and WardenAdminService)

**Files:**
- Modify: `Services/WardenService.cs`
- Modify: `Services/WardenAdminService.cs`

- [ ] **Step 1: Update WardenService.RemoveWarden**

Extract `SteamID` and `PlayerName` on main thread.

```csharp
    public void RemoveWarden()
    {
        if (CurrentWarden != null && CurrentWarden.IsValid)
        {
            if (_wardenStartTime != null)
            {
                var duration = DateTime.Now - _wardenStartTime.Value;
                ulong steamId = CurrentWarden.SteamID;
                string playerName = CurrentWarden.PlayerName;
                _ = Task.Run(async () => await _statService.UpdateWardenStats(steamId, playerName, duration.TotalSeconds));
            }
            // ... rest
```

- [ ] **Step 2: Update WardenAdminService.RemoveWardenAdmin**

```csharp
    public void RemoveWardenAdmin(CCSPlayerController player)
    {
        if (_wardenAdminStartTimes.TryGetValue(player.SteamID, out DateTime startTime))
        {
            double seconds = (DateTime.Now - startTime).TotalSeconds;
            ulong steamId = player.SteamID;
            string playerName = player.PlayerName;
            _ = Task.Run(async () => await _statService.UpdateWardenAdminStats(steamId, playerName, seconds));
            _wardenAdminStartTimes.Remove(player.SteamID);
        }
        // ... rest
```

- [ ] **Step 3: Commit**

```bash
git add Services/WardenService.cs Services/WardenAdminService.cs
git commit -m "refactor: update stat service calls to use primitive types"
```

### Task 4: Final Verification

- [ ] **Step 1: Build the project**

Run: `dotnet build -c Release`
Expected: SUCCESS

- [ ] **Step 2: Commit any final fixes if needed**

```bash
git commit -m "fix: final adjustments for thread safety refactor"
```
