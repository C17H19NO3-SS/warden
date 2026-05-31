using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using JailBreak.Config;
using JailBreak.Services;
using JailBreak.Helpers;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace JailBreak;

public class JailBreakPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "JailBreak Warden";
    public override string ModuleVersion => "1.0.1";
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

    public WardenService WardenService => _wardenService;
    public VoteService VoteService => _voteService;
    public GameManagerService GameManagerService => _gameManagerService;
    public IseliService IseliService => _iseliService;
    public FFMenuService FFMenuService => _ffMenuService;
    public UtilityService UtilityService => _utilityService;

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

            // Config File Logic
            // CS# already merged the defaults into 'config', we just write it back to disk to fill missing keys.
            string configDir = Path.GetDirectoryName(configPath) ?? "";
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, json);

            // Lang File Logic
            if (File.Exists(langPath))
            {
                string langJson = File.ReadAllText(langPath);
                var loadedLang = JsonSerializer.Deserialize<LangConfig>(langJson);
                if (loadedLang != null)
                {
                    Lang = loadedLang;
                    // Save it back to disk to add any missing keys
                    string updatedLangJson = JsonSerializer.Serialize(Lang, options);
                    File.WriteAllText(langPath, updatedLangJson);
                }
            }
            else
            {
                // Create default lang file
                string dir = Path.GetDirectoryName(langPath) ?? "";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string defaultLangJson = JsonSerializer.Serialize(Lang, options);
                File.WriteAllText(langPath, defaultLangJson);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JailBreak] Error loading configs: {ex.Message}");
        }
    }

    public bool IsJailbreakMap()
    {
        string mapName = Server.MapName.ToLower();
        return mapName.Contains("jb_") || mapName.Contains("jail_");
    }

    public override void Load(bool hotReload)
    {
        // StoreBridge config yolunu dinamik ayarla
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
        _utilityService = new UtilityService(this, _wardenService);
        _lrService = new LastRequestService(this, _wardenService);
        _rebelService = new RebelService(this);
        _gameManagerService = new GameManagerService(this, _wardenService, _freezeService);

        RegisterListener<Listeners.OnClientDisconnect>(_wardenService.OnClientDisconnect);
        RegisterListener<Listeners.OnMapStart>((mapName) => _utilityService.ResetKacCmRecords());

        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerPing>(OnPlayerPing);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        HookUserMessage(118, OnUserMessageChat, HookMode.Pre);
        AddCommandListener("jointeam", OnJoinTeam);

        AddCommand("css_w", "Komutçu ol", _wardenService.CommandBecomeWarden);
        AddCommand("css_uw", "Komutçuluktan çık", _wardenService.CommandUnwarden);
        AddCommand("css_komkalan", "Komutçunun kalan süresini gör", _wardenService.CommandKomKalan);
        AddCommand("css_topkomutcu", "En çok komutçu olanları gör", _wardenService.CommandTopKomutcu);
        AddCommand("css_komoyla", "Komutçu oylamasını başlat", _voteService.CommandStartVote);
        AddCommand("css_komdk", "Komutçuyu atma oylamasını başlat", _voteService.CommandStartKickVote);
        AddCommand("css_komaday", "Komutçu oylamasına katıl", _voteService.CommandJoinVote);
        AddCommand("css_ka", "Komutçu admin menüsü", _wardenService.CommandWardenAdmin);
        AddCommand("css_kasil", "Komutçu adminini kaldır", _wardenService.CommandRemoveWardenAdmin);
        AddCommand("css_k", "Komutçu ana menüsünü açar", _wardenService.CommandKomMenu);
        AddCommand("css_kommenu", "Komutçu ana menüsünü açar", _wardenService.CommandKomMenu);
        AddCommand("css_marker", "İşaretleyici boyutunu ayarla", _markerService.CommandMarker);
        AddCommand("css_reloadconfig", "Config dosyasını yeniden yükle", CommandReloadConfig);

        // Sustum Commands
        AddCommand("css_dsustum", "Deagle ödüllü sustum başlat", _sustumService.CommandDsustum);
        AddCommand("css_tsustum", "T'ye geçme ödüllü sustum başlat", _sustumService.CommandTsustum);
        AddCommand("css_olusustum", "Canlanma ödüllü sustum başlat", _sustumService.CommandOlusustum);

        // Freeze Commands
        AddCommand("css_td", "T takımını dondur", _freezeService.CommandFreeze);
        AddCommand("css_tdb", "T takımının donmasını çöz", _freezeService.CommandUnfreeze);
        AddCommand("css_fz", "Gecikmeli dondurma başlat", _freezeService.CommandDelayedFreeze);
        AddCommand("css_fz0", "Dondurmayı sıfırla", _freezeService.CommandResetFreeze);

        // Iseli Commands
        AddCommand("css_iseli", "İseli kapı kontrolü", _iseliService.CommandIseli);
        AddCommand("css_iq", "Kapıları anında aç", _iseliService.CommandQuickIseli);

        // Position Commands
        AddCommand("css_daire", "T takımını daire diz", _positionService.CommandDaire);
        AddCommand("css_diz", "T takımını yan yana diz", _positionService.CommandDiz);

        // FF Menu Commands
        AddCommand("css_ffmenu", "FF silah menüsünü aç", _ffMenuService.CommandFFMenu);
        AddCommand("css_ffkapat", "FF'i kapat", _ffMenuService.CommandFFKapat);
        AddCommand("css_ffk", "FF'i kapat", _ffMenuService.CommandFFKapat);
        AddCommand("css_ff0", "FF'i kapat ve silahları al", _ffMenuService.CommandFF0);
        AddCommand("css_ffondur", "FF aç ve sonunda dondur", _ffMenuService.CommandFFOndur);

        // Utility Commands
        AddCommand("css_hpa", "Herkesin canını 100 yap", _utilityService.CommandHpAll);
        AddCommand("css_hpt", "T takımının canını 100 yap", _utilityService.CommandHpT);
        AddCommand("css_hpct", "CT takımının canını 100 yap", _utilityService.CommandHpCT);
        AddCommand("css_gelt", "Tüm T'leri çek", _utilityService.CommandGetT);
        AddCommand("css_gelct", "Tüm CT'leri çek", _utilityService.CommandGetCT);
        AddCommand("css_gelall", "Tüm oyuncuları çek", _utilityService.CommandGetAll);
        AddCommand("css_af", "Herkesi canlandır ve canını 100 yap", _utilityService.CommandAf);
        AddCommand("css_git", "Oyuncuya git", _utilityService.CommandGit);
        AddCommand("css_haksal", "CT ile T takımını yer değiştir", _utilityService.CommandHakSal);
        AddCommand("css_ba", "Bunnyhop aç", _utilityService.CommandBunnyOpen);
        AddCommand("css_bk", "Bunnyhop kapat", _utilityService.CommandBunnyClose);
        AddCommand("css_umct", "CT takımının mutesini aç", _utilityService.CommandUnmuteCT);
        AddCommand("css_uct", "CT takımının mutesini aç", _utilityService.CommandUnmuteCT);
        AddCommand("css_umt", "T takımının mutesini aç", _utilityService.CommandUnmuteT);
        AddCommand("css_ut", "T takımının mutesini aç", _utilityService.CommandUnmuteT);
        AddCommand("css_ss", "T takımının silahlarını al", _utilityService.CommandSs);
        AddCommand("css_strip", "T takımının silahlarını al", _utilityService.CommandSs);
        AddCommand("css_kaccm", "Kaç cm ölçer", _utilityService.CommandKacCm);
        AddCommand("css_mct", "CT takımını mutele", _utilityService.CommandMuteCT);
        AddCommand("css_mt", "T takımını mutele", _utilityService.CommandMuteT);
        AddCommand("css_topkaccm", "Kaç cm sıralamasını göster", _utilityService.CommandTopKacCm);
        AddCommand("css_otores", "Otomatik canlanmayı aç", _utilityService.CommandOtores);
        AddCommand("css_otores0", "Otomatik canlanmayı kapat", _utilityService.CommandOtores0);

        // LR Commands
        AddCommand("css_sonakalan", "LR menüsünü aç", _lrService.CommandSonaKalan);
        AddCommand("css_sonsec", "Sona kalan hariç öldür ve LR aç", _lrService.CommandSonSec);
        AddCommand("css_sonseç", "Sona kalan hariç öldür ve LR aç", _lrService.CommandSonSec);
        AddCommand("css_isyancılar", "İsyancı listesini göster", _rebelService.CommandRebels);
        AddCommand("css_isyancilar", "İsyancı listesini göster", _rebelService.CommandRebels);

        // Game Mode Commands
        AddCommand("css_box", "Boks modunu başlatır", (p, i) => { if (p != null) _gameManagerService.OpenBoxMenu(p); });
        AddCommand("css_b", "Boks modunu başlatır", (p, i) => { if (p != null) _gameManagerService.OpenBoxMenu(p); });
        AddCommand("css_saklambac", "Saklambaç modunu başlatır", (p, i) => { 
            if (p == null || !_wardenService.HasPermission(p)) return;
            string arg = i.GetArg(1);
            int time = int.TryParse(arg, out int t) ? t : 30;
            _gameManagerService.StartSaklambac(time);
        });
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
        if (player != null && !AdminManager.PlayerHasPermissions(player, "@css/root"))
        {
            player.PrintToChat(PluginHelper.FormatChat(Config.ChatPrefix, Lang.MsgNoPermission));
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
                    OnConfigParsed(Config); // This will also re-save and re-load lang if we modified it, but let's load lang explicitly below just in case.
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
            Console.WriteLine($"[JailBreak] Error reloading config: {ex.Message}");
            player?.PrintToChat(PluginHelper.FormatChat(Config.ChatPrefix, $"{ChatColors.Red}Config yüklenirken hata oluştu: {ex.Message}"));
            return;
        }
        string reloadMsg = PluginHelper.FormatChat(Config.ChatPrefix, Lang.MsgConfigReloaded);
        if (player != null) player.PrintToChat(reloadMsg);
        else info.ReplyToCommand(reloadMsg);
    }
}
