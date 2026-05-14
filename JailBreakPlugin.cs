using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Config;
using JailBreak.Services;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace JailBreak;

public class JailBreakPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "JailBreak Warden";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "SoulSnatcher";

    public PluginConfig Config { get; set; } = new();

    private WardenService _wardenService = null!;
    private VoteService _voteService = null!;
    private ChatService _chatService = null!;
    private MarkerService _markerService = null!;
    private SustumService _sustumService = null!;
    private FreezeService _freezeService = null!;
    private IseliService _iseliService = null!;
    private PositionService _positionService = null!;
    private FFMenuService _ffMenuService = null!;
    private UtilityService _utilityService = null!;
    private LastRequestService _lrService = null!;

    public VoteService VoteService => _voteService;

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;

        string configPath = Path.Combine(ModuleDirectory, "../../configs/plugins/JailBreak/JailBreak.json");
        try
        {
            if (File.Exists(configPath))
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, json);
            }
        }
        catch (Exception) { }
    }

    public bool IsJailbreakMap()
    {
        string mapName = Server.MapName.ToLower();
        return mapName.Contains("jb_") || mapName.Contains("jail_");
    }

    public override void Load(bool hotReload)
    {
        _wardenService = new WardenService(this);
        _voteService = new VoteService(this, _wardenService);
        _chatService = new ChatService(this, _wardenService);
        _markerService = new MarkerService(this, _wardenService);
        _sustumService = new SustumService(this, _wardenService);
        _freezeService = new FreezeService(this);
        _iseliService = new IseliService(this, _wardenService);
        _positionService = new PositionService(this, _wardenService, _freezeService);
        _ffMenuService = new FFMenuService(this, _wardenService, _freezeService);
        _utilityService = new UtilityService(this, _wardenService);
        _lrService = new LastRequestService(this, _wardenService);

        RegisterListener<Listeners.OnClientDisconnect>(_wardenService.OnClientDisconnect);
        RegisterListener<Listeners.OnTick>(_ffMenuService.OnTick);
        RegisterListener<Listeners.OnTick>(_lrService.OnTick);

        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerPing>(OnPlayerPing);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        HookUserMessage(118, OnUserMessageChat, HookMode.Pre);
        AddCommandListener("jointeam", OnJoinTeam);

        AddCommand("css_w", "Become Warden", _wardenService.CommandBecomeWarden);
        AddCommand("css_uw", "Unwarden", _wardenService.CommandUnwarden);
        AddCommand("css_komoyla", "Start Warden Vote", _voteService.CommandStartVote);
        AddCommand("css_komdk", "Start Warden Kick Vote", _voteService.CommandStartKickVote);
        AddCommand("css_komaday", "Join Warden Vote", _voteService.CommandJoinVote);
        AddCommand("css_ka", "Warden Admin Menu", _wardenService.CommandWardenAdmin);
        AddCommand("css_kasil", "Remove Warden Admin", _wardenService.CommandRemoveWardenAdmin);
        AddCommand("css_marker", "Set marker size", _markerService.CommandMarker);
        AddCommand("css_reloadconfig", "Reload config", CommandReloadConfig);

        // Sustum Commands
        AddCommand("css_dsustum", "Start Dsustum", _sustumService.CommandDsustum);
        AddCommand("css_tsustum", "Start Tsustum", _sustumService.CommandTsustum);
        AddCommand("css_olusustum", "Start Olusustum", _sustumService.CommandOlusustum);

        // Freeze Commands
        AddCommand("css_td", "Freeze T", _freezeService.CommandFreeze);
        AddCommand("css_tdb", "Unfreeze T", _freezeService.CommandUnfreeze);
        AddCommand("css_fz", "Delayed Freeze T", _freezeService.CommandDelayedFreeze);
        AddCommand("css_fz0", "Reset Freeze", _freezeService.CommandResetFreeze);

        // Iseli Commands
        AddCommand("css_iseli", "Iseli door control", _iseliService.CommandIseli);
        AddCommand("css_iq", "Quick open doors", _iseliService.CommandQuickIseli);

        // Position Commands
        AddCommand("css_daire", "Arrange T in circle", _positionService.CommandDaire);
        AddCommand("css_diz", "Arrange T in line", _positionService.CommandDiz);

        // FF Menu Commands
        AddCommand("css_ffmenu", "Open FF weapon menu", _ffMenuService.CommandFFMenu);
        AddCommand("css_ffkapat", "Disable FF", _ffMenuService.CommandFFKapat);
        AddCommand("css_ffk", "Disable FF", _ffMenuService.CommandFFKapat);
        AddCommand("css_ff0", "Disable FF and strip weapons", _ffMenuService.CommandFF0);
        AddCommand("css_ffondur", "Enable FF with freeze end", _ffMenuService.CommandFFOndur);

        // Utility Commands
        AddCommand("css_hpa", "Set HP 100 for everyone", _utilityService.CommandHpAll);
        AddCommand("css_hpt", "Set HP 100 for T", _utilityService.CommandHpT);
        AddCommand("css_hpct", "Set HP 100 for CT", _utilityService.CommandHpCT);
        AddCommand("css_gelt", "Get all Terrorists", _utilityService.CommandGetT);
        AddCommand("css_git", "Go to player", _utilityService.CommandGit);
        AddCommand("css_haksal", "Swap CT with T", _utilityService.CommandHakSal);

        // LR Commands
        AddCommand("css_sonakalan", "Open LR menu", _lrService.CommandSonaKalan);
        AddCommand("css_sonsec", "Kill all but one T and open LR", _lrService.CommandSonSec);
        AddCommand("css_sonseç", "Kill all but one T and open LR", _lrService.CommandSonSec);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;

        _wardenService.OnRoundStart();
        _markerService.OnRoundStart();
        _freezeService.OnRoundStart();
        _iseliService.OnRoundStart();
        _ffMenuService.OnRoundStart();
        _lrService.OnRoundStart();
        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;

        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        Server.NextFrame(() =>
        {
            if (player.IsValid && player.PawnIsAlive)
            {
                player.GiveNamedItem("weapon_knife");
            }
        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;
        _lrService.OnPlayerDeath(@event);
        return HookResult.Continue;
    }

    private HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;
        _lrService.OnWeaponFire(@event);
        return HookResult.Continue;
    }

    private HookResult OnJoinTeam(CCSPlayerController? player, CommandInfo info)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;
        if (player == null || !player.IsValid) return HookResult.Continue;

        string teamArg = info.GetArg(1);
        if (teamArg == "3")
        {
            player.PrintToChat($" {ChatService.ReplaceColors(Config.ChatPrefix)} {ChatService.ReplaceColors(Config.MsgCannotJoinCT)}");
            Server.NextFrame(() =>
            {
                if (player.IsValid) player.ChangeTeam(CsTeam.Terrorist);
            });
            return HookResult.Continue;
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerPing(EventPlayerPing @event, GameEventInfo info)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;

        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        _markerService.OnPlayerPing(@event, player);
        _positionService.OnPlayerPing(@event, player);
        return HookResult.Continue;
    }

    private HookResult OnUserMessageChat(UserMessage @event)
    {
        if (!IsJailbreakMap()) return HookResult.Continue;

        int entityIndex = @event.ReadInt("entityindex");
        var player = Utilities.GetPlayerFromIndex(entityIndex);
        if (player == null || !player.IsValid) return HookResult.Continue;

        string message = @event.ReadString("param2");
        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;

        if (_sustumService.HandleSustumChat(player, message)) return HookResult.Stop;
        if (_voteService.HandleVoteChat(player, message)) return HookResult.Stop;
        if (_ffMenuService.HandleFFMenuChat(player, message)) return HookResult.Stop;
        if (_lrService.HandleLRChat(player, message)) return HookResult.Stop;

        return _chatService.OnUserMessageChat(@event, player, message);
    }

    private void CommandReloadConfig(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            player.PrintToChat($" {ChatService.ReplaceColors(Config.ChatPrefix)} {ChatService.ReplaceColors(Config.MsgNoPermission)}");
            return;
        }

        string configPath = Path.Combine(ModuleDirectory, "../../configs/plugins/JailBreak/JailBreak.json");
        if (File.Exists(configPath))
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);
                var newConfig = JsonSerializer.Deserialize<PluginConfig>(jsonString);
                if (newConfig != null)
                {
                    Config = newConfig;
                    OnConfigParsed(Config);
                }
            }
            catch (Exception ex)
            {
                player?.PrintToChat($" {ChatService.ReplaceColors(Config.ChatPrefix)} {ChatColors.Red}Config yüklenirken hata oluştu: {ex.Message}");
                return;
            }
        }

        string reloadMsg = $" {ChatService.ReplaceColors(Config.ChatPrefix)} {ChatService.ReplaceColors(Config.MsgConfigReloaded)}";
        if (player != null) player.PrintToChat(reloadMsg);
        else info.ReplyToCommand(reloadMsg);
    }
}
