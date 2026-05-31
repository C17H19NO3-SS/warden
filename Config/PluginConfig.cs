using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace JailBreak.Config;

public class FFWeapon
{
    public string Name { get; set; } = "";
    public string ItemName { get; set; } = "";
}

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("WardenDurationMinutes")]
    public int WardenDurationMinutes { get; set; } = 30;

    [JsonPropertyName("MaxCTRevives")]
    public int MaxCTRevives { get; set; } = 3;

    [JsonPropertyName("LRCreditReward")]
    public int LRCreditReward { get; set; } = 1000;

    [JsonPropertyName("WardenTag")]
    public string WardenTag { get; set; } = "[{Green}Komutçu{Default}]";

    [JsonPropertyName("WardenNameColor")]
    public string WardenNameColor { get; set; } = "{Blue}";

    [JsonPropertyName("WardenChatColor")]
    public string WardenChatColor { get; set; } = "{Default}";

    [JsonPropertyName("WardenAdminTag")]
    public string WardenAdminTag { get; set; } = "[{Green}Kom. Admin{Default}]";

    [JsonPropertyName("WardenAdminNameColor")]
    public string WardenAdminNameColor { get; set; } = "{DarkBlue}";

    [JsonPropertyName("WardenAdminChatColor")]
    public string WardenAdminChatColor { get; set; } = "{Default}";

    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = "{Green}[Mazi] {White}";

    // Sustum Config
    [JsonPropertyName("SustumWords")]
    public List<string> SustumWords { get; set; } = new() { "elma", "armut", "kalem", "masa", "kitap", "bilgisayar", "telefon" };

    // FF Menu Config
    [JsonPropertyName("FFPrimaryWeaponList")]
    public List<FFWeapon> FFPrimaryWeaponList { get; set; } = new()
{
    new FFWeapon { Name = "AK-47", ItemName = "weapon_ak47" },
    new FFWeapon { Name = "M4A1-S", ItemName = "weapon_m4a1_silencer" },
    new FFWeapon { Name = "AWP", ItemName = "weapon_awp" },
    new FFWeapon { Name = "Galil", ItemName = "weapon_galilar" },
    new FFWeapon { Name = "Famas", ItemName = "weapon_famas" },
    new FFWeapon { Name = "SSG 08", ItemName = "weapon_ssg08" },
    new FFWeapon { Name = "Nova", ItemName = "weapon_nova" },
    new FFWeapon { Name = "Yok", ItemName = "none" }
};

    [JsonPropertyName("FFSecondaryWeaponList")]
    public List<FFWeapon> FFSecondaryWeaponList { get; set; } = new()
{
    new FFWeapon { Name = "Deagle", ItemName = "weapon_deagle" },
    new FFWeapon { Name = "USP-S", ItemName = "weapon_usp_silencer" },
    new FFWeapon { Name = "Glock", ItemName = "weapon_glock" },
    new FFWeapon { Name = "P250", ItemName = "weapon_p250" },
    new FFWeapon { Name = "Five-Seven", ItemName = "weapon_fiveseven" },
    new FFWeapon { Name = "Tec-9", ItemName = "weapon_tec9" },
    new FFWeapon { Name = "Yok", ItemName = "none" }
};

    [JsonPropertyName("SustumDuration")]
    public int SustumDuration { get; set; } = 15;

    [JsonPropertyName("SustumRewardDeagleWeapon")]
    public string SustumRewardDeagleWeapon { get; set; } = "weapon_deagle";

    // Vote Config
    [JsonPropertyName("VotePhaseDuration")]
    public int VotePhaseDuration { get; set; } = 30;

    [JsonPropertyName("KickVoteDelayedDuration")]
    public int KickVoteDelayedDuration { get; set; } = 60;
}
