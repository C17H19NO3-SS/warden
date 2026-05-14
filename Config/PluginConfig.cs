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
    public string ChatPrefix { get; set; } = "[{Red}JailBreak{Default}]";

    // Sustum Config
    [JsonPropertyName("SustumWords")]
    public List<string> SustumWords { get; set; } = new() { "elma", "armut", "kalem", "masa", "kitap", "bilgisayar", "telefon" };

    // Messages
    [JsonPropertyName("MsgOnlyJailbreakMap")]
    public string MsgOnlyJailbreakMap { get; set; } = "{Red}Bu komut sadece JailBreak haritalarında kullanılabilir!";

    [JsonPropertyName("MsgWardenDurationExpired")]
    public string MsgWardenDurationExpired { get; set; } = "Komutçunun süresi doldu, otomatik oylama başlatılıyor.";

    [JsonPropertyName("MsgNewWarden")]
    public string MsgNewWarden { get; set; } = "{0} yeni komutçu oldu!";

    [JsonPropertyName("MsgOnlyCTCanBeWarden")]
    public string MsgOnlyCTCanBeWarden { get; set; } = "Sadece CT takımındakiler komutçu olabilir.";

    [JsonPropertyName("MsgWardenExists")]
    public string MsgWardenExists { get; set; } = "Zaten bir komutçu var: {0}";

    [JsonPropertyName("MsgWardenLeft")]
    public string MsgWardenLeft { get; set; } = "{0} komutçuluğu bıraktı.";

    [JsonPropertyName("MsgNotWarden")]
    public string MsgNotWarden { get; set; } = "Sen komutçu değilsin!";

    [JsonPropertyName("MsgOnlyWardenCanAddAdmin")]
    public string MsgOnlyWardenCanAddAdmin { get; set; } = "Sadece komutçu admin ekleyebilir.";

    [JsonPropertyName("MsgPlayerNotFound")]
    public string MsgPlayerNotFound { get; set; } = "Oyuncu bulunamadı.";

    [JsonPropertyName("MsgNoOtherAdmins")]
    public string MsgNoOtherAdmins { get; set; } = "Admin yetkisi olan başka oyuncu yok.";

    [JsonPropertyName("MsgPlayerHasNoAdminPerms")]
    public string MsgPlayerHasNoAdminPerms { get; set; } = "Bu oyuncunun admin yetkisi yok.";

    [JsonPropertyName("MsgWardenAdminSelected")]
    public string MsgWardenAdminSelected { get; set; } = "{0}, komutçu admini seçildi!";

    [JsonPropertyName("MsgWardenAdminRemoved")]
    public string MsgWardenAdminRemoved { get; set; } = "{0} komutçu adminliğinden alındı.";

    [JsonPropertyName("MsgPlayerNotWardenAdmin")]
    public string MsgPlayerNotWardenAdmin { get; set; } = "Oyuncu zaten komutçu admin değil.";

    [JsonPropertyName("MsgOnlyAdminsCanVote")]
    public string MsgOnlyAdminsCanVote { get; set; } = "Bu komutu sadece adminler kullanabilir.";

    [JsonPropertyName("MsgVoteAlreadyActive")]
    public string MsgVoteAlreadyActive { get; set; } = "Şu anda zaten bir oylama süreci aktif.";

    [JsonPropertyName("MsgNoActiveWarden")]
    public string MsgNoActiveWarden { get; set; } = "Şu anda aktif bir komutçu yok.";

    [JsonPropertyName("MsgCandidatePhaseStarted")]
    public string MsgCandidatePhaseStarted { get; set; } = "Komutçu oylaması için aday süreci başladı! Aday olmak için sohbete !komaday yazın.";

    [JsonPropertyName("MsgKickVotePhaseStarted")]
    public string MsgKickVotePhaseStarted { get; set; } = "Mevcut komutçu için oylama başladı! Sohbete 1 (Kal) veya 2 (Değiş/Atılsın) yazın.";

    [JsonPropertyName("MsgCandidatePhaseNotActive")]
    public string MsgCandidatePhaseNotActive { get; set; } = "Şu anda adaylık süreci aktif değil.";

    [JsonPropertyName("MsgAlreadyCandidate")]
    public string MsgAlreadyCandidate { get; set; } = "Zaten adaysınız.";

    [JsonPropertyName("MsgCandidateListFull")]
    public string MsgCandidateListFull { get; set; } = "Aday listesi dolu (Maksimum 5).";

    [JsonPropertyName("MsgPlayerBecameCandidate")]
    public string MsgPlayerBecameCandidate { get; set; } = "{0} komutçuluk için aday oldu! ({1}/5)";

    [JsonPropertyName("MsgNoCandidates")]
    public string MsgNoCandidates { get; set; } = "Hiç aday çıkmadığı için oylama iptal edildi.";

    [JsonPropertyName("MsgVotePhaseStarted")]
    public string MsgVotePhaseStarted { get; set; } = "Komutçu oylaması başladı! Oy vermek için sohbete adayın numarasını yazın (Örnek: 1).";

    [JsonPropertyName("MsgVoteEndedNewWarden")]
    public string MsgVoteEndedNewWarden { get; set; } = "Oylama bitti! Yeni komutçu: {0}";

    [JsonPropertyName("MsgVoteCancelled")]
    public string MsgVoteCancelled { get; set; } = "Oylama iptal edildi veya kazanan oyundan ayrıldı.";

    [JsonPropertyName("MsgKickVoteDecided")]
    public string MsgKickVoteDecided { get; set; } = "Oylama sonucu: Komutçunun değişmesine karar verildi. Komutçu 1 dakika sonra atılacak! (Atılsın: {0} - Kalsın: {1})";

    [JsonPropertyName("MsgKickVoteDelayedSuccess")]
    public string MsgKickVoteDelayedSuccess { get; set; } = "Komutçu atıldı ve tüm CT'ler T takımına geçirildi!";

    [JsonPropertyName("MsgKickVoteStayed")]
    public string MsgKickVoteStayed { get; set; } = "Oylama sonucu: Komutçu görevinde kalmaya devam ediyor! (Kalsın: {0} - Atılsın: {1})";

    [JsonPropertyName("MsgVoteCast")]
    public string MsgVoteCast { get; set; } = "Oyunu {0} adlı oyuncuya verdin.";

    [JsonPropertyName("MsgCannotVoteSelf")]
    public string MsgCannotVoteSelf { get; set; } = "Kendi oylamanıza katılamazsınız.";

    [JsonPropertyName("MsgKickVoteKeepCast")]
    public string MsgKickVoteKeepCast { get; set; } = "Oyunuzu 'Komutçu Kalsın' olarak kullandınız.";

    [JsonPropertyName("MsgKickVoteKickCast")]
    public string MsgKickVoteKickCast { get; set; } = "Oyunuzu 'Komutçu Atılsın' olarak kullandınız.";

    [JsonPropertyName("MsgMarkerSizeSet")]
    public string MsgMarkerSizeSet { get; set; } = "Marker boyutu {0} olarak ayarlandı.";

    [JsonPropertyName("MsgInvalidNumber")]
    public string MsgInvalidNumber { get; set; } = "Lütfen geçerli bir sayı girin.";

    [JsonPropertyName("MsgCannotJoinCT")]
    public string MsgCannotJoinCT { get; set; } = "CT takımına kendiniz geçemezsiniz, T takımına yönlendiriliyorsunuz.";

    [JsonPropertyName("MsgNoPermission")]
    public string MsgNoPermission { get; set; } = "Bu komutu kullanma yetkiniz yok.";

    [JsonPropertyName("MsgConfigReloaded")]
    public string MsgConfigReloaded { get; set; } = "Config dosyası yeniden yüklendi.";

    [JsonPropertyName("MsgMarkerUsage")]
    public string MsgMarkerUsage { get; set; } = "Kullanım: !marker <boyut> (1-250, Varsayılan: 80)";

    [JsonPropertyName("MsgKasilUsage")]
    public string MsgKasilUsage { get; set; } = "Kullanım: !kasil <isim>";

    [JsonPropertyName("MsgDaireUsage")]
    public string MsgDaireUsage { get; set; } = "Kullanım: !daire <genişlik>";

    [JsonPropertyName("MsgDizUsage")]
    public string MsgDizUsage { get; set; } = "Kullanım: !diz <mesafe>";

    [JsonPropertyName("MsgDaireApplied")]
    public string MsgDaireApplied { get; set; } = "T takımı {0} genişliğinde daire şeklinde dizildi!";
    // Position Commands
    [JsonPropertyName("MsgDizApplied")]
    public string MsgDizApplied { get; set; } = "T takımı {0} mesafesinde yan yana dizildi!";

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

    [JsonPropertyName("MsgFFStarted")]
    public string MsgFFStarted { get; set; } = "Friend Fire (FF) başladı! Süre: {0} saniye. Silah: {1}";

    [JsonPropertyName("MsgFFEnded")]
    public string MsgFFEnded { get; set; } = "FF süresi doldu, FF kapatıldı.";

    [JsonPropertyName("MsgFFDisabled")]
    public string MsgFFDisabled { get; set; } = "FF kapatıldı.";

    [JsonPropertyName("MsgFF0Applied")]
    public string MsgFF0Applied { get; set; } = "FF kapatıldı ve T takımı silahsızlandırıldı!";

    [JsonPropertyName("MsgFFOndurApplied")]
    public string MsgFFOndurApplied { get; set; } = "FF süresi sonunda T'ler dondurulacak!";

    [JsonPropertyName("MsgFFMenuUsage")]
    public string MsgFFMenuUsage { get; set; } = "Kullanım: !ffmenu <saniye>";

    [JsonPropertyName("MsgFFOndurUsage")]
    public string MsgFFOndurUsage { get; set; } = "Kullanım: !ffondur <saniye>";

    [JsonPropertyName("MsgOnlyWardenAdminOrRootCanUse")]
    public string MsgOnlyWardenAdminOrRootCanUse { get; set; } = "Bu komutu sadece komutçu, komutçu admini veya root yetkilileri kullanabilir.";

    // Utility Messages
    [JsonPropertyName("MsgHpAllSet")]
    public string MsgHpAllSet { get; set; } = "{Green}Tüm oyuncuların canı 100 olarak ayarlandı!";
    [JsonPropertyName("MsgHpTSet")]
    public string MsgHpTSet { get; set; } = "{Green}T takımının canı 100 olarak ayarlandı!";
    [JsonPropertyName("MsgHpCTSet")]
    public string MsgHpCTSet { get; set; } = "{Green}CT takımının canı 100 olarak ayarlandı!";
    [JsonPropertyName("MsgGetTApplied")]
    public string MsgGetTApplied { get; set; } = "{Green}T takımı komutçunun yanına çekildi!";
    [JsonPropertyName("MsgGitUsage")]
    public string MsgGitUsage { get; set; } = "{Red}Kullanım: !git <isim>";
    [JsonPropertyName("MsgGitApplied")]
    public string MsgGitApplied { get; set; } = "{Green}{0} adlı oyuncunun yanına ışınlandın.";
    [JsonPropertyName("MsgHakSalUsage")]
    public string MsgHakSalUsage { get; set; } = "{Red}Kullanım: !haksal <isim>";
    [JsonPropertyName("MsgHakSalTargetNotFound")]
    public string MsgHakSalTargetNotFound { get; set; } = "{Red}Belirtilen T oyuncusu bulunamadı.";
    [JsonPropertyName("MsgHakSalApplied")]
    public string MsgHakSalApplied { get; set; } = "{Blue}{0} {Default}hakkını {Red}{1} {Default}oyuncusuna saldı!";

    // LR Messages
    [JsonPropertyName("MsgLROnlyLastT")]
    public string MsgLROnlyLastT { get; set; } = "{Red}Bu komutu sadece sona kalan T kullanabilir.";
    [JsonPropertyName("MsgSonSecUsage")]
    public string MsgSonSecUsage { get; set; } = "{Red}Kullanım: !sonseç <isim>";
    [JsonPropertyName("MsgSonSecApplied")]
    public string MsgSonSecApplied { get; set; } = "{Green}{0} {Default}sona bırakıldı ve LR menüsü açıldı!";
    [JsonPropertyName("MsgLRStarted")]
    public string MsgLRStarted { get; set; } = "{Green}LR Başladı! {Red}{0} vs {Blue}{1}";
    [JsonPropertyName("MsgLRTypeInfo")]
    public string MsgLRTypeInfo { get; set; } = "{Yellow}Tür: {0}";
    [JsonPropertyName("MsgLRDeagleTurn")]
    public string MsgLRDeagleTurn { get; set; } = "{Green}Sıra sende! Ateş et.";
    [JsonPropertyName("MsgLRDeagleWait")]
    public string MsgLRDeagleWait { get; set; } = "{Yellow}Rakibinin ateş etmesini bekle.";
    [JsonPropertyName("MsgLRCTLost")]
    public string MsgLRCTLost { get; set; } = "{Red}{0} {Default}LR kaybettiği için T takımına atıldı!";
    [JsonPropertyName("MsgLRRebellion")]
    public string MsgLRRebellion { get; set; } = "{Red}{0} {Default}isyan etmeyi seçti!";
    [JsonPropertyName("MsgLRNoCTFound")]
    public string MsgLRNoCTFound { get; set; } = "{Red}Düello yapacak CT bulunamadı.";

    // LR HUD Titles/Content
    [JsonPropertyName("HudTitleLRMain")]
    public string HudTitleLRMain { get; set; } = "SONA KALAN MENÜSÜ";
    [JsonPropertyName("HudTitleLRType")]
    public string HudTitleLRType { get; set; } = "LR TÜRÜ SEÇİN";
    [JsonPropertyName("HudTitleLRTarget")]
    public string HudTitleLRTarget { get; set; } = "RAKİP SEÇİN";
    [JsonPropertyName("HudContentLRMain")]
    public string HudContentLRMain { get; set; } = "!1 LR (Son İstek)<br>!2 İSİAN (Rebellion)";
    [JsonPropertyName("HudContentLRType")]
    public string HudContentLRType { get; set; } = "!1 Deagle Düellosu<br>!2 Bıçak Düellosu";
    [JsonPropertyName("HudInstructionLRNextPage")]
    public string HudInstructionLRNextPage { get; set; } = "Tab: Sonraki Sayfa";

    // FF HUD Titles/Content
    [JsonPropertyName("HudTitleFFSystem")]
    public string HudTitleFFSystem { get; set; } = "FF SİSTEMİ";
    [JsonPropertyName("HudTitleFFPrimary")]
    public string HudTitleFFPrimary { get; set; } = "FF BİRİNCİL SİLAH";
    [JsonPropertyName("HudTitleFFSecondary")]
    public string HudTitleFFSecondary { get; set; } = "FF TABANCA SEÇİMİ";
    [JsonPropertyName("HudTitleFFStartDelay")]
    public string HudTitleFFStartDelay { get; set; } = "FF BAŞLAMASINA";
    [JsonPropertyName("HudTitleFFEndDelay")]
    public string HudTitleFFEndDelay { get; set; } = "FF KAPANMASINA";
    [JsonPropertyName("HudTitleFFActive")]
    public string HudTitleFFActive { get; set; } = "FF AKTİF";
    [JsonPropertyName("HudContentFFMenuTime")]
    public string HudContentFFMenuTime { get; set; } = "Seçim İçin Kalan: <font color='{0}'><b>{1}s</b></font><br><br>";
    [JsonPropertyName("HudContentFFStartDelay")]
    public string HudContentFFStartDelay { get; set; } = "Silahlar: <font color='{0}'><b>{1}</b></font><br>FF Açılmasına: <font color='{2}'><b>{3}s</b></font>";
    [JsonPropertyName("HudContentFFEndDelay")]
    public string HudContentFFEndDelay { get; set; } = "FF Kapanmasına: <font color='{0}'><b>{1}s</b></font>";
    [JsonPropertyName("HudContentFFFreezeWarning")]
    public string HudContentFFFreezeWarning { get; set; } = "<br><font color='{0}'>Sonunda T'ler dondurulacak!</font>";
    [JsonPropertyName("HudContentFFActiveWeapons")]
    public string HudContentFFActiveWeapons { get; set; } = "Silahlar: <font color='{0}'><b>{1}</b></font>";
    [JsonPropertyName("HudInstructionFFPagination")]
    public string HudInstructionFFPagination { get; set; } = "Tab: Sonraki, Shift: Önceki";
    [JsonPropertyName("HudInstructionFFOndurInfo")]
    public string HudInstructionFFOndurInfo { get; set; } = "!ffondur <süre> ile kapatma sayacı başlatılabilir.";

    [JsonPropertyName("MsgFFWeaponSelected")]
    public string MsgFFWeaponSelected { get; set; } = "{Green}{0} seçildi! FF {1} saniye sonra başlayacak.";
    [JsonPropertyName("MsgFFActiveNow")]
    public string MsgFFActiveNow { get; set; } = "{Green}FF Aktif Edildi!";
    [JsonPropertyName("MsgFFOnlyKnife")]
    public string MsgFFOnlyKnife { get; set; } = "Sadece Bıçak";
    [JsonPropertyName("MsgWardenAdminMenuTitle")]
    public string MsgWardenAdminMenuTitle { get; set; } = "Komutçu Admin Seçimi";


    [JsonPropertyName("MsgSustumWinner")]
    public string MsgSustumWinner { get; set; } = "{0} kazananı: {1}! Ödül: {2}";

    [JsonPropertyName("MsgSustumRewardDeagle")]
    public string MsgSustumRewardDeagle { get; set; } = "Tek mermili Deagle";

    [JsonPropertyName("MsgSustumRewardCT")]
    public string MsgSustumRewardCT { get; set; } = "CT Takımına Geçiş";

    [JsonPropertyName("MsgSustumRewardRespawn")]
    public string MsgSustumRewardRespawn { get; set; } = "Canlanma";

    [JsonPropertyName("MsgSustumExpired")]
    public string MsgSustumExpired { get; set; } = "{0} süresi doldu, kimse yazamadı.";

    [JsonPropertyName("MsgTFreezeStarted")]
    public string MsgTFreezeStarted { get; set; } = "T takımı donduruldu!";

    [JsonPropertyName("MsgTFreezeEnded")]
    public string MsgTFreezeEnded { get; set; } = "T takımının donması çözüldü!";

    [JsonPropertyName("MsgTFreezeCountdownStarted")]
    public string MsgTFreezeCountdownStarted { get; set; } = "T takımı {0} saniye sonra dondurulacak!";

    [JsonPropertyName("MsgTFreezeCountdownCancelled")]
    public string MsgTFreezeCountdownCancelled { get; set; } = "Donma süresi sıfırlandı.";

    [JsonPropertyName("MsgIseliStarted")]
    public string MsgIseliStarted { get; set; } = "İseli süreci başladı! Kapılar {0} saniye sonra açılacak.";

    [JsonPropertyName("MsgIseliDoorsOpened")]
    public string MsgIseliDoorsOpened { get; set; } = "İseli süresi doldu, tüm kapılar açıldı!";

    [JsonPropertyName("MsgIseliQuickOpened")]
    public string MsgIseliQuickOpened { get; set; } = "İseli anında tamamlandı, tüm kapılar açıldı!";

    // HUD Design
    [JsonPropertyName("HudTitleIseli")]
    public string HudTitleIseli { get; set; } = "İSELİ GERİ SAYIM";

    [JsonPropertyName("HudContentIseli")]
    public string HudContentIseli { get; set; } = "Kapıların açılmasına: <font color='red'><b>{0}s</b></font>";

    [JsonPropertyName("HudTitleFreeze")]
    public string HudTitleFreeze { get; set; } = "T DONDURULUYOR";

    [JsonPropertyName("HudContentFreeze")]
    public string HudContentFreeze { get; set; } = "Donmaya kalan süre: <font color='red'><b>{0}s</b></font>";

    [JsonPropertyName("HudContentSustum")]
    public string HudContentSustum { get; set; } = "Yazman gereken: <font color='green'><b>{0}</b></font><br>Kalan Süre: <font color='red'><b>{1}s</b></font>";
}
