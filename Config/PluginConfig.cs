using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace JailBreak.Config;

/// <summary>
/// Represents a weapon in the Free-For-All (FF) menu.
/// </summary>
public class FFWeapon
{
    /// <summary> Gets or sets the display name of the weapon. </summary>
    public string Name { get; set; } = "";
    /// <summary> Gets or sets the internal item name used in the game. </summary>
    public string ItemName { get; set; } = "";
}

/// <summary>
/// Defines the plugin configuration settings.
/// </summary>
public class PluginConfig : BasePluginConfig
{
    /// <summary> Gets or sets the log level (1=Error, 2=Debug, 3=Trace). </summary>
    [JsonPropertyName("LogLevel")]
    public int LogLevel { get; set; } = 2; 

    /// <summary> Gets or sets the default duration for a Warden session in minutes. </summary>
    [JsonPropertyName("WardenDurationMinutes")]
    public int WardenDurationMinutes { get; set; } = 30;

    /// <summary> Gets or sets the maximum number of times Counter-Terrorists can be revived per round. </summary>
    [JsonPropertyName("MaxCTRevives")]
    public int MaxCTRevives { get; set; } = 3;

    /// <summary> Gets or sets the credit reward for the last Terrorist. </summary>
    [JsonPropertyName("LRCreditReward")]
    public int LRCreditReward { get; set; } = 1000;

    /// <summary> Gets or sets the chat tag for the Warden. </summary>
    [JsonPropertyName("WardenTag")]
    public string WardenTag { get; set; } = "[{Green}Komutçu{Default}]";

    /// <summary> Gets or sets the name color for the Warden. </summary>
    [JsonPropertyName("WardenNameColor")]
    public string WardenNameColor { get; set; } = "{Blue}";

    /// <summary> Gets or sets the chat color for the Warden. </summary>
    [JsonPropertyName("WardenChatColor")]
    public string WardenChatColor { get; set; } = "{Default}";

    /// <summary> Gets or sets the chat tag for the Warden Admin. </summary>
    [JsonPropertyName("WardenAdminTag")]
    public string WardenAdminTag { get; set; } = "[{Green}Kom. Admin{Default}]";

    /// <summary> Gets or sets the name color for the Warden Admin. </summary>
    [JsonPropertyName("WardenAdminNameColor")]
    public string WardenAdminNameColor { get; set; } = "{DarkBlue}";

    /// <summary> Gets or sets the chat color for the Warden Admin. </summary>
    [JsonPropertyName("WardenAdminChatColor")]
    public string WardenAdminChatColor { get; set; } = "{Default}";

    /// <summary> Gets or sets the general chat prefix. </summary>
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = "{Green}[Mazi] {White}";

    /// <summary> Gets or sets the list of words used in the "Sustum" game mode. </summary>
    [JsonPropertyName("SustumWords")]
    public List<string> SustumWords { get; set; } = new() { "elma", "armut", "kalem", "masa", "kitap", "bilgisayar", "telefon" };

    /// <summary> Gets or sets the list of primary weapons available in the FF menu. </summary>
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

    /// <summary> Gets or sets the list of secondary weapons available in the FF menu. </summary>
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

    /// <summary> Gets or sets the duration of the "Sustum" game mode. </summary>
    [JsonPropertyName("SustumDuration")]
    public int SustumDuration { get; set; } = 15;

    /// <summary> Gets or sets the weapon given as a reward for winning "Sustum". </summary>
    [JsonPropertyName("SustumRewardDeagleWeapon")]
    public string SustumRewardDeagleWeapon { get; set; } = "weapon_deagle";

    /// <summary> Gets or sets the duration of the voting phase. </summary>
    [JsonPropertyName("VotePhaseDuration")]
    public int VotePhaseDuration { get; set; } = 30;

    /// <summary> Gets or sets the duration before a kick vote takes effect. </summary>
    [JsonPropertyName("KickVoteDelayedDuration")]
    public int KickVoteDelayedDuration { get; set; } = 60;
}
