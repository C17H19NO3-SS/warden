using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using JailBreak.Config;

namespace JailBreak.Services;

public enum FFMenuStep
{
    None,
    Primary,
    Secondary
}

public class FFMenuService
{
    private readonly JailBreakPlugin _plugin;
    private readonly WardenService _wardenService;
    private readonly FreezeService _freezeService;

    private bool _isMenuOpen = false;
    private FFMenuStep _currentStep = FFMenuStep.None;
    private int _currentPage = 0;
    private const int ItemsPerPage = 5;

    private bool _isFFActive = false;
    private bool _isCountingToStart = false;
    private bool _isCountingToEnd = false;
    private int _ffRemainingTime = 0;
    private bool _freezeOnEnd = false;

    private string _selectedPrimaryName = "";
    private string _selectedPrimaryItem = "";
    private string _selectedSecondaryName = "";
    private string _selectedSecondaryItem = "";

    private CounterStrikeSharp.API.Modules.Timers.Timer? _tickTimer;
    private readonly Dictionary<ulong, PlayerButtons> _lastButtons = new();

    public FFMenuService(JailBreakPlugin plugin, WardenService wardenService, FreezeService freezeService)
    {
        _plugin = plugin;
        _wardenService = wardenService;
        _freezeService = freezeService;
    }

    public void OnRoundStart()
    {
        DisableFF(true);
        _isMenuOpen = false;
        _currentStep = FFMenuStep.None;
        _isCountingToStart = false;
        _isCountingToEnd = false;
        _tickTimer?.Kill();
        _tickTimer = null;
    }

    public void CommandFFMenu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        string arg = info.GetArg(1);
        if (!int.TryParse(arg, out int time))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgFFMenuUsage)}");
            return;
        }

        StartFFMenu(time);
    }

    public void CommandFFKapat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        DisableFF();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgFFDisabled)}");
    }

    public void CommandFF0(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        DisableFF();
        StripTWeapons();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgFF0Applied)}");
    }

    public void CommandFFOndur(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!HasPermission(player)) return;

        string arg = info.GetArg(1);
        if (!int.TryParse(arg, out int time))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgFFOndurUsage)}");
            return;
        }

        _freezeOnEnd = true;
        _isCountingToEnd = true;
        _ffRemainingTime = time;
        _isCountingToStart = false;
        _isMenuOpen = false;

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);

        UpdateHUD(); // Show HUD immediately

        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgFFOndurApplied)}");
    }

    private void StartFFMenu(int time)
    {
        _isMenuOpen = true;
        _currentStep = FFMenuStep.Primary;
        _currentPage = 0;
        _ffRemainingTime = time;
        _isFFActive = false;
        _isCountingToStart = false;
        _isCountingToEnd = false;

        _selectedPrimaryName = "";
        _selectedPrimaryItem = "";
        _selectedSecondaryName = "";
        _selectedSecondaryItem = "";

        _tickTimer?.Kill();
        _tickTimer = _plugin.AddTimer(1.0f, Tick, TimerFlags.REPEAT);
    }

    private void Tick()
    {
        if (!_isMenuOpen && !_isCountingToStart && !_isCountingToEnd && !_isFFActive)
        {
            _tickTimer?.Kill();
            _tickTimer = null;
            return;
        }

        if (_ffRemainingTime > 0)
        {
            _ffRemainingTime--;
        }
        else
        {
            if (_isCountingToStart)
            {
                _isCountingToStart = false;
                _isFFActive = true;
                EnableFF();
                Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Green}FF Aktif Edildi!");
            }
            else if (_isCountingToEnd)
            {
                EndFF();
            }
            else if (!_isFFActive && !_isMenuOpen)
            {
                _tickTimer?.Kill();
                _tickTimer = null;
            }
            return;
        }

        UpdateHUD();
    }

    public void OnTick()
    {
        if (!_isMenuOpen) return;

        var warden = _wardenService.CurrentWarden;
        if (warden == null || !warden.IsValid) return;

        var buttons = warden.Buttons;
        _lastButtons.TryGetValue(warden.SteamID, out var lastButtons);

        // Tab: Scoreboard
        if ((buttons & PlayerButtons.Scoreboard) != 0 && (lastButtons & PlayerButtons.Scoreboard) == 0)
        {
            NextPage();
        }
        // Shift: Speed
        if ((buttons & PlayerButtons.Speed) != 0 && (lastButtons & PlayerButtons.Speed) == 0)
        {
            PrevPage();
        }

        _lastButtons[warden.SteamID] = buttons;
    }

    private void NextPage()
    {
        var list = _currentStep == FFMenuStep.Primary ? _plugin.Config.FFPrimaryWeaponList : _plugin.Config.FFSecondaryWeaponList;
        int maxPage = (list.Count - 1) / ItemsPerPage;
        _currentPage++;
        if (_currentPage > maxPage) _currentPage = 0;
        UpdateHUD();
    }

    private void PrevPage()
    {
        var list = _currentStep == FFMenuStep.Primary ? _plugin.Config.FFPrimaryWeaponList : _plugin.Config.FFSecondaryWeaponList;
        int maxPage = (list.Count - 1) / ItemsPerPage;
        _currentPage--;
        if (_currentPage < 0) _currentPage = maxPage;
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        string title = "FF SİSTEMİ";
        string content = "";
        string instruction = "";

        if (_isMenuOpen)
        {
            title = _currentStep == FFMenuStep.Primary ? "FF BİRİNCİL SİLAH" : "FF TABANCA SEÇİMİ";
            content = $"Seçim İçin Kalan: <font color='{HudHelper.ColorTime}'><b>{_ffRemainingTime}s</b></font><br><br>";

            var list = _currentStep == FFMenuStep.Primary ? _plugin.Config.FFPrimaryWeaponList : _plugin.Config.FFSecondaryWeaponList;
            int start = _currentPage * ItemsPerPage;
            int end = Math.Min(start + ItemsPerPage, list.Count);

            for (int i = start; i < end; i++)
            {
                content += $"!{i - start + 1} {list[i].Name}<br>";
            }

            instruction = "Tab: Sonraki, Shift: Önceki";
        }
        else if (_isCountingToStart)
        {
            title = "FF BAŞLAMASINA";
            string weapons = _selectedPrimaryName;
            if (!string.IsNullOrEmpty(_selectedSecondaryName)) weapons += $" & {_selectedSecondaryName}";

            content = $"Silahlar: <font color='{HudHelper.ColorSuccess}'><b>{weapons}</b></font><br>FF Açılmasına: <font color='{HudHelper.ColorTime}'><b>{_ffRemainingTime}s</b></font>";
        }
        else if (_isCountingToEnd)
        {
            title = "FF KAPANMASINA";
            content = $"FF Kapanmasına: <font color='{HudHelper.ColorTime}'><b>{_ffRemainingTime}s</b></font>";
            if (_freezeOnEnd) content += $"<br><font color='{HudHelper.ColorSystem}'>Sonunda T'ler dondurulacak!</font>";
        }
        else if (_isFFActive)
        {
            title = "FF AKTİF";
            string weapons = _selectedPrimaryName;
            if (!string.IsNullOrEmpty(_selectedSecondaryName)) weapons += $" & {_selectedSecondaryName}";
            content = $"Silahlar: <font color='{HudHelper.ColorSuccess}'><b>{weapons}</b></font>";
            instruction = "!ffondur <süre> ile kapatma sayacı başlatılabilir.";
        }

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot))
        {
            p.PrintToCenterHtml(HudHelper.FormatHud(title, content, instruction));
        }
    }

    public bool HandleFFMenuChat(CCSPlayerController player, string message)
    {
        if (!_isMenuOpen) return false;
        if (!HasPermission(player)) return false;

        if (message.StartsWith("!"))
        {
            if (int.TryParse(message.Substring(1), out int choice))
            {
                if (choice > 0 && choice <= ItemsPerPage)
                {
                    var list = _currentStep == FFMenuStep.Primary ? _plugin.Config.FFPrimaryWeaponList : _plugin.Config.FFSecondaryWeaponList;
                    int index = (_currentPage * ItemsPerPage) + choice - 1;
                    if (index >= 0 && index < list.Count)
                    {
                        SelectWeapon(list[index]);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void SelectWeapon(FFWeapon weapon)
    {
        if (_currentStep == FFMenuStep.Primary)
        {
            _selectedPrimaryName = weapon.Name == "Yok" ? "" : weapon.Name;
            _selectedPrimaryItem = weapon.ItemName == "none" ? "" : weapon.ItemName;

            _currentStep = FFMenuStep.Secondary;
            _currentPage = 0;
            UpdateHUD();
        }
        else if (_currentStep == FFMenuStep.Secondary)
        {
            _selectedSecondaryName = weapon.Name == "Yok" ? "" : weapon.Name;
            _selectedSecondaryItem = weapon.ItemName == "none" ? "" : weapon.ItemName;

            _isMenuOpen = false;
            _isCountingToStart = true;

            GiveWeaponsToTs();

            string weaponsMsg = _selectedPrimaryName;
            if (!string.IsNullOrEmpty(_selectedSecondaryName)) weaponsMsg += $" ve {_selectedSecondaryName}";
            if (string.IsNullOrEmpty(weaponsMsg)) weaponsMsg = "Sadece Bıçak";

            Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatColors.Green}{weaponsMsg} seçildi! FF {_ffRemainingTime} saniye sonra başlayacak.");
        }
    }

    private void EnableFF()
    {
        Server.ExecuteCommand("mp_teammates_are_enemies 1");
        _isFFActive = true;
        _isCountingToStart = false;
    }

    private void DisableFF(bool silent = false)
    {
        Server.ExecuteCommand("mp_teammates_are_enemies 0");
        _isFFActive = false;
        _isMenuOpen = false;
        _currentStep = FFMenuStep.None;
        _isCountingToStart = false;
        _isCountingToEnd = false;
        _ffRemainingTime = 0;
        _freezeOnEnd = false;
    }

    private void EndFF()
    {
        DisableFF();
        Server.PrintToChatAll($" {ChatService.ReplaceColors(_plugin.Config.ChatPrefix)} {ChatService.ReplaceColors(_plugin.Config.MsgFFEnded)}");

        if (_freezeOnEnd)
        {
            _freezeService.FreezeAll();
        }
    }

    private void GiveWeaponsToTs()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            player.RemoveWeapons();
            player.GiveNamedItem("weapon_knife");

            if (!string.IsNullOrEmpty(_selectedPrimaryItem))
                player.GiveNamedItem(_selectedPrimaryItem);

            if (!string.IsNullOrEmpty(_selectedSecondaryItem))
                player.GiveNamedItem(_selectedSecondaryItem);
        }
    }

    private void StripTWeapons()
    {
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid && p.Team == CsTeam.Terrorist && p.PawnIsAlive))
        {
            player.RemoveWeapons();
            player.GiveNamedItem("weapon_knife");
        }
    }

    private bool HasPermission(CCSPlayerController player)
    {
        return _wardenService.IsWarden(player) ||
               AdminManager.PlayerHasPermissions(player, "@jailbreak/ka") ||
               AdminManager.PlayerHasPermissions(player, "@css/root");
    }
}
