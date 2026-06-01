using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using JailBreak.Config;
using JailBreak.Services;
using System.Text.Json;
using System.Text.Encodings.Web;
using JailBreak.Helpers;

namespace JailBreak;

public class JailBreakPlugin : BasePlugin, IPluginConfig<PluginConfig>, ICommandDispatcher
{
    public override string ModuleName => "JailBreak Warden";
    public override string ModuleVersion => "1.0.2";
    public override string ModuleAuthor => "SoulSnatcher";

    public PluginConfig Config { get; set; } = new();
    public LangConfig Lang { get; set; } = new();

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
    private RebelService _rebelService = null!;
    private GameManagerService _gameManagerService = null!;
    private DispatcherService _dispatcherService = null!;
    private HudService _hudService = null!;
    private HudManager _hudManager = null!;

    public WardenService WardenService => _wardenService;
    public VoteService VoteService => _voteService;
    public GameManagerService GameManagerService => _gameManagerService;
    public IseliService IseliService => _iseliService;
    public FFMenuService FFMenuService => _ffMenuService;
    public UtilityService UtilityService => _utilityService;
    public MarkerService MarkerService => _markerService;
    public HudService HudService => _hudService;

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;

        string configPath = Path.Combine(ModuleDirectory, "../../configs/plugins/JailBreak/JailBreak.json");
        string langPath = Path.Combine(ModuleDirectory, "../../configs/plugins/JailBreak/lang.json");

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string configDir = Path.GetDirectoryName(configPath) ?? "";
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, json);

            if (File.Exists(langPath))
            {
                string langJson = File.ReadAllText(langPath);
                var loadedLang = JsonSerializer.Deserialize<LangConfig>(langJson);
                if (loadedLang != null)
                {
                    Lang = loadedLang;
                    string updatedLangJson = JsonSerializer.Serialize(Lang, options);
                    File.WriteAllText(langPath, updatedLangJson);
                }
            }
            else
            {
                string dir = Path.GetDirectoryName(langPath) ?? "";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string defaultLangJson = JsonSerializer.Serialize(Lang, options);
                File.WriteAllText(langPath, defaultLangJson);
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error loading configs during OnConfigParsed.", ex);
        }
    }

    public bool IsJailbreakMap()
    {
        string mapName = Server.MapName.ToLower();
        return mapName.Contains("jb_") || mapName.Contains("jail_");
    }

    public override void Load(bool hotReload)
    {
        LogHelper.Initialize(Config, ModuleDirectory);

        string storeConfigPath = Path.Combine(ModuleDirectory, "../../configs/plugins/cs2-store/config.toml");
        StoreApi.StoreBridge.SetConfigPath(storeConfigPath);

        _wardenService = new WardenService(this);
        _voteService = new VoteService(this, _wardenService);
        _chatService = new ChatService(this, _wardenService);
        _markerService = new MarkerService(this, _wardenService);
        _sustumService = new SustumService(this, _wardenService);
        _freezeService = new FreezeService(this);
        _iseliService = new IseliService(this, _wardenService);
        _positionService = new PositionService(this, _wardenService, _freezeService);
        _ffMenuService = new FFMenuService(this, _wardenService, _freezeService);
        _hudService = new HudService(this);
        _utilityService = new UtilityService(this, _wardenService, _hudService);
        _lrService = new LastRequestService(this, _wardenService);
        _rebelService = new RebelService(this);
        _gameManagerService = new GameManagerService(this, _wardenService, _freezeService);
        _dispatcherService = new DispatcherService();
        _hudService = new HudService(this);
        _hudManager = new HudManager(this);

        RegisterListener<Listeners.OnClientDisconnect>(_wardenService.OnClientDisconnect);
        RegisterListener<Listeners.OnMapStart>((mapName) => _utilityService.ResetKacCmRecords());
        RegisterListener<Listeners.OnTick>(_hudManager.OnTick);

        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerPing>(OnPlayerPing);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        HookUserMessage(118, OnUserMessageChat, HookMode.Pre);
        AddCommandListener("jointeam", OnJoinTeam);

        _wardenService.RegisterCommands(this);
        RegisterCommand("css_komoyla", "Komutçu oylamasını başlat", _voteService.CommandStartVote);
        RegisterCommand("css_komdk", "Komutçuyu atma oylamasını başlat", _voteService.CommandStartKickVote);
        RegisterCommand("css_komaday", "Komutçu oylamasına katıl", _voteService.CommandJoinVote);
        RegisterCommand("css_marker", "İşaretleyici menüsünü açar", _markerService.OpenMarkerMenu);
        RegisterCommand("css_reloadconfig", "Config dosyasını yeniden yükle", CommandReloadConfig);

        _sustumService.RegisterCommands(this);

        RegisterCommand("css_td", "T takımını dondur", _freezeService.CommandFreeze);
        RegisterCommand("css_tdb", "T takımının donmasını çöz", _freezeService.CommandUnfreeze);
        RegisterCommand("css_fz", "Gecikmeli dondurma başlat", _freezeService.CommandDelayedFreeze);
        RegisterCommand("css_fz0", "Dondurmayı sıfırla", _freezeService.CommandResetFreeze);

        RegisterCommand("css_iseli", "İseli kapı kontrolü", _iseliService.CommandIseli);
        RegisterCommand("css_iq", "Kapıları anında aç", _iseliService.CommandQuickIseli);

        RegisterCommand("css_daire", "T takımını daire diz", _positionService.CommandDaire);
        RegisterCommand("css_diz", "T takımını yan yana diz", _positionService.CommandDiz);

        _ffMenuService.RegisterCommands(this);

        _utilityService.RegisterCommands(this);

        RegisterCommand("css_sonakalan", "LR menüsünü aç", _lrService.CommandSonaKalan);
        RegisterCommand("css_sonsec", "Sona kalan hariç öldür ve LR aç", _lrService.CommandSonSec);
        RegisterCommand("css_sonseç", "Sona kalan hariç öldür ve LR aç", _lrService.CommandSonSec);
        RegisterCommand("css_isyancılar", "İsyancı listesini göster", _rebelService.CommandRebels);
        RegisterCommand("css_isyancilar", "İsyancı listesini göster", _rebelService.CommandRebels);

        RegisterCommand("css_box", "Boks modunu başlatır", (p, i) => { if (p != null) _gameManagerService.OpenBoxMenu(p); });
        RegisterCommand("css_b", "Boks modunu başlatır", (p, i) => { if (p != null) _gameManagerService.OpenBoxMenu(p); });
        RegisterCommand("css_saklambac", "Saklambaç modunu başlatır", (p, i) => { 
            if (p == null || !_wardenService.HasPermission(p, "@css/changemap")) return;
            string arg = i.GetArg(1);
            int time = int.TryParse(arg, out int t) ? t : 30;
            _gameManagerService.StartSaklambac(time);
        });
        
        LogHelper.LogInfo("JailBreak eklentisi başarıyla yüklendi.");
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (IsJailbreakMap())
        {
            _wardenService.OnRoundStart();
            _markerService.OnRoundStart();
            _iseliService.OnRoundStart();
            _ffMenuService.OnRoundStart();
            _lrService.OnRoundStart();
            _gameManagerService.OnRoundStart();
        }

        _freezeService.OnRoundStart();
        _utilityService.OnRoundStart();
        return HookResult.Continue;
    }

    public void RegisterCommand(string command, string description, CommandInfo.CommandCallback callback)
    {
        _dispatcherService.RegisterCommand(command, description, callback);
        AddCommand(command, description, _dispatcherService.ExecuteCommand);
    }

    public void ExecuteCommand(CCSPlayerController? player, CommandInfo info)
    {
        _dispatcherService.ExecuteCommand(player, info);
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
        _rebelService.OnPlayerDeath(@event);
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
            player.PrintToChat(PluginHelper.FormatChat(Config.ChatPrefix, Lang.MsgCannotJoinCT));
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
        var player = @event.Userid;
        if (player == null || !player.IsValid) return HookResult.Continue;

        if (IsJailbreakMap())
        {
            _markerService.OnPlayerPing(@event, player);
        }

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

        return _chatService.OnUserMessageChat(@event, player, message);
    }

    private void CommandReloadConfig(CCSPlayerController? player, CommandInfo info)
    {
        if (player != null && !_wardenService.HasPermission(player, "@css/root"))
        {
            return;
        }

        string configPath = Path.Combine(ModuleDirectory, "../../configs/plugins/JailBreak/JailBreak.json");
        string langPath = Path.Combine(ModuleDirectory, "../../configs/plugins/JailBreak/lang.json");

        try
        {
            if (File.Exists(configPath))
            {
                string jsonString = File.ReadAllText(configPath);
                var newConfig = JsonSerializer.Deserialize<PluginConfig>(jsonString);
                if (newConfig != null)
                {
                    Config = newConfig;
                    OnConfigParsed(Config);
                    LogHelper.Initialize(Config, ModuleDirectory);
                }
            }

            if (File.Exists(langPath))
            {
                string langJson = File.ReadAllText(langPath);
                var loadedLang = JsonSerializer.Deserialize<LangConfig>(langJson);
                if (loadedLang != null) Lang = loadedLang;
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error reloading config.", ex);
            player?.PrintToChat(PluginHelper.FormatChat(Config.ChatPrefix, $"{ChatColors.Red}Config yüklenirken hata oluştu: {ex.Message}"));
            return;
        }
        string reloadMsg = PluginHelper.FormatChat(Config.ChatPrefix, Lang.MsgConfigReloaded);
        if (player != null) player.PrintToChat(reloadMsg);
        else info.ReplyToCommand(reloadMsg);
    }
}
