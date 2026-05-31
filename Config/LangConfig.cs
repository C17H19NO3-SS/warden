using System.Text.Json.Serialization;

namespace JailBreak.Config;

public class LangConfig
{

    // Messages
    [JsonPropertyName("MsgOnlyJailbreakMap")]
    public string MsgOnlyJailbreakMap { get; set; } = "{White}Bu komut sadece JailBreak haritalarında kullanılabilir!";


    [JsonPropertyName("MsgWardenDurationExpired")]
    public string MsgWardenDurationExpired { get; set; } = "{White}Komutçunun süresi doldu, otomatik oylama başlatılıyor.";


    [JsonPropertyName("MsgNewWarden")]
    public string MsgNewWarden { get; set; } = "{White}{Red}{0}{White} yeni komutçu oldu!";


    [JsonPropertyName("MsgOnlyCTCanBeWarden")]
    public string MsgOnlyCTCanBeWarden { get; set; } = "{White}Sadece CT takımındakiler komutçu olabilir.";


    [JsonPropertyName("MsgWardenExists")]
    public string MsgWardenExists { get; set; } = "{White}Zaten bir komutçu var: {Red}{0}{White}";


    [JsonPropertyName("MsgWardenLeft")]
    public string MsgWardenLeft { get; set; } = "{White}{Red}{0}{White} komutçuluğu bıraktı.";


    [JsonPropertyName("MsgNotWarden")]
    public string MsgNotWarden { get; set; } = "{White}Sen komutçu değilsin!";


    [JsonPropertyName("MsgOnlyWardenCanAddAdmin")]
    public string MsgOnlyWardenCanAddAdmin { get; set; } = "{White}Sadece komutçu admin ekleyebilir.";


    [JsonPropertyName("MsgPlayerNotFound")]
    public string MsgPlayerNotFound { get; set; } = "{White}Oyuncu bulunamadı.";


    [JsonPropertyName("MsgNoOtherAdmins")]
    public string MsgNoOtherAdmins { get; set; } = "{White}Admin yetkisi olan başka oyuncu yok.";


    [JsonPropertyName("MsgPlayerHasNoAdminPerms")]
    public string MsgPlayerHasNoAdminPerms { get; set; } = "{White}Bu oyuncunun admin yetkisi yok.";


    [JsonPropertyName("MsgWardenAdminSelected")]
    public string MsgWardenAdminSelected { get; set; } = "{White}{Red}{0}{White}, komutçu admini seçildi!";


    [JsonPropertyName("MsgWardenAdminRemoved")]
    public string MsgWardenAdminRemoved { get; set; } = "{White}{Red}{0}{White} komutçu adminliğinden alındı.";


    [JsonPropertyName("MsgPlayerNotWardenAdmin")]
    public string MsgPlayerNotWardenAdmin { get; set; } = "{White}Oyuncu zaten komutçu admin değil.";


    [JsonPropertyName("MsgOnlyAdminsCanVote")]
    public string MsgOnlyAdminsCanVote { get; set; } = "{White}Bu komutu sadece adminler kullanabilir.";


    [JsonPropertyName("MsgVoteAlreadyActive")]
    public string MsgVoteAlreadyActive { get; set; } = "{White}Şu anda zaten bir oylama süreci aktif.";


    [JsonPropertyName("MsgNoActiveWarden")]
    public string MsgNoActiveWarden { get; set; } = "{White}Şu anda aktif bir komutçu yok.";


    [JsonPropertyName("MsgCandidatePhaseStarted")]
    public string MsgCandidatePhaseStarted { get; set; } = "{White}Komutçu oylaması için aday süreci başladı! Aday olmak için sohbete !komaday yazın.";


    [JsonPropertyName("MsgKickVotePhaseStarted")]
    public string MsgKickVotePhaseStarted { get; set; } = "{White}Mevcut komutçu için oylama başladı! Sohbete 1 (Kal) veya 2 (Değiş/Atılsın) yazın.";


    [JsonPropertyName("MsgCandidatePhaseNotActive")]
    public string MsgCandidatePhaseNotActive { get; set; } = "{White}Şu anda adaylık süreci aktif değil.";


    [JsonPropertyName("MsgAlreadyCandidate")]
    public string MsgAlreadyCandidate { get; set; } = "{White}Zaten adaysınız.";


    [JsonPropertyName("MsgCandidateListFull")]
    public string MsgCandidateListFull { get; set; } = "{White}Aday listesi dolu (Maksimum 5).";


    [JsonPropertyName("MsgPlayerBecameCandidate")]
    public string MsgPlayerBecameCandidate { get; set; } = "{White}{Red}{0}{White} komutçuluk için aday oldu! ({Red}{1}{White}/5)";


    [JsonPropertyName("MsgNoCandidates")]
    public string MsgNoCandidates { get; set; } = "{White}Hiç aday çıkmadığı için oylama iptal edildi.";


    [JsonPropertyName("MsgVotePhaseStarted")]
    public string MsgVotePhaseStarted { get; set; } = "{White}Komutçu oylaması başladı! Oy vermek için sohbete adayın numarasını yazın (Örnek: 1).";


    [JsonPropertyName("MsgVoteEndedNewWarden")]
    public string MsgVoteEndedNewWarden { get; set; } = "{White}Oylama bitti! Yeni komutçu: {Red}{0}{White}";


    [JsonPropertyName("MsgVoteCancelled")]
    public string MsgVoteCancelled { get; set; } = "{White}Oylama iptal edildi veya kazanan oyundan ayrıldı.";


    [JsonPropertyName("MsgKickVoteDecided")]
    public string MsgKickVoteDecided { get; set; } = "{White}Oylama sonucu: Komutçunun değişmesine karar verildi. Komutçu 1 dakika sonra atılacak! (Atılsın: {Red}{0}{White} - Kalsın: {Red}{1}{White})";


    [JsonPropertyName("MsgKickVoteDelayedSuccess")]
    public string MsgKickVoteDelayedSuccess { get; set; } = "{White}Komutçu atıldı ve tüm CT'ler T takımına geçirildi!";


    [JsonPropertyName("MsgKickVoteStayed")]
    public string MsgKickVoteStayed { get; set; } = "{White}Oylama sonucu: Komutçu görevinde kalmaya devam ediyor! (Kalsın: {Red}{0}{White} - Atılsın: {Red}{1}{White})";


    [JsonPropertyName("MsgVoteCast")]
    public string MsgVoteCast { get; set; } = "{White}Oyunu {Red}{0}{White} adlı oyuncuya verdin.";


    [JsonPropertyName("MsgCannotVoteSelf")]
    public string MsgCannotVoteSelf { get; set; } = "{White}Kendi oylamanıza katılamazsınız.";


    [JsonPropertyName("MsgKickVoteKeepCast")]
    public string MsgKickVoteKeepCast { get; set; } = "{White}Oyunuzu 'Komutçu Kalsın' olarak kullandınız.";


    [JsonPropertyName("MsgKickVoteKickCast")]
    public string MsgKickVoteKickCast { get; set; } = "{White}Oyunuzu 'Komutçu Atılsın' olarak kullandınız.";


    [JsonPropertyName("MsgMarkerSizeSet")]
    public string MsgMarkerSizeSet { get; set; } = "{White}Marker boyutu {Red}{0}{White} olarak ayarlandı.";


    [JsonPropertyName("MsgInvalidNumber")]
    public string MsgInvalidNumber { get; set; } = "{White}Lütfen geçerli bir sayı girin.";


    [JsonPropertyName("MsgCannotJoinCT")]
    public string MsgCannotJoinCT { get; set; } = "{White}CT takımına kendiniz geçemezsiniz, T takımına yönlendiriliyorsunuz.";


    [JsonPropertyName("MsgNoPermission")]
    public string MsgNoPermission { get; set; } = "{White}Bu komutu kullanma yetkiniz yok.";


    [JsonPropertyName("MsgConfigReloaded")]
    public string MsgConfigReloaded { get; set; } = "{White}Config dosyası yeniden yüklendi.";


    [JsonPropertyName("MsgMarkerUsage")]
    public string MsgMarkerUsage { get; set; } = "{White}Kullanım: !marker <boyut> (1-250, Varsayılan: 80)";


    [JsonPropertyName("MsgKasilUsage")]
    public string MsgKasilUsage { get; set; } = "{White}Kullanım: !kasil <isim>";


    [JsonPropertyName("MsgDaireUsage")]
    public string MsgDaireUsage { get; set; } = "{White}Kullanım: !daire <genişlik>";


    [JsonPropertyName("MsgDizUsage")]
    public string MsgDizUsage { get; set; } = "{White}Kullanım: !diz <mesafe>";


    [JsonPropertyName("MsgDaireApplied")]
    public string MsgDaireApplied { get; set; } = "{White}T takımı {Red}{0}{White} genişliğinde daire şeklinde dizildi!";

    // Position Commands
    [JsonPropertyName("MsgDizApplied")]
    public string MsgDizApplied { get; set; } = "{White}T takımı {Red}{0}{White} mesafesinde yan yana dizildi!";


    [JsonPropertyName("MsgFFStarted")]
    public string MsgFFStarted { get; set; } = "{White}Friend Fire (FF) başladı! Süre: {Red}{0}{White} saniye. Silah: {Red}{1}{White}";


    [JsonPropertyName("MsgFFEnded")]
    public string MsgFFEnded { get; set; } = "{White}FF süresi doldu, FF kapatıldı.";


    [JsonPropertyName("MsgFFDisabled")]
    public string MsgFFDisabled { get; set; } = "{White}FF kapatıldı.";


    [JsonPropertyName("MsgFF0Applied")]
    public string MsgFF0Applied { get; set; } = "{White}FF kapatıldı ve T takımı silahsızlandırıldı!";


    [JsonPropertyName("MsgFFOndurApplied")]
    public string MsgFFOndurApplied { get; set; } = "{White}FF süresi sonunda T'ler dondurulacak!";


    [JsonPropertyName("MsgFFMenuUsage")]
    public string MsgFFMenuUsage { get; set; } = "{White}Kullanım: !ffmenu <saniye>";


    [JsonPropertyName("MsgFFOndurUsage")]
    public string MsgFFOndurUsage { get; set; } = "{White}Kullanım: !ffondur <saniye>";


    [JsonPropertyName("MsgOnlyWardenAdminOrRootCanUse")]
    public string MsgOnlyWardenAdminOrRootCanUse { get; set; } = "{White}Bu komutu sadece komutçu, komutçu admini veya root yetkilileri kullanabilir.";


    // Utility Messages
    [JsonPropertyName("MsgHpAllSet")]
    public string MsgHpAllSet { get; set; } = "{White}Tüm oyuncuların canı 100 olarak ayarlandı!";

    [JsonPropertyName("MsgHpTSet")]
    public string MsgHpTSet { get; set; } = "{White}T takımının canı 100 olarak ayarlandı!";

    [JsonPropertyName("MsgHpCTSet")]
    public string MsgHpCTSet { get; set; } = "{White}CT takımının canı 100 olarak ayarlandı!";

    [JsonPropertyName("MsgGetTApplied")]
    public string MsgGetTApplied { get; set; } = "{White}T takımı komutçunun yanına çekildi!";

    [JsonPropertyName("MsgGitUsage")]
    public string MsgGitUsage { get; set; } = "{White}Kullanım: !git <isim>";

    [JsonPropertyName("MsgGitApplied")]
    public string MsgGitApplied { get; set; } = "{White}{Red}{0}{White} adlı oyuncunun yanına ışınlandın.";

    [JsonPropertyName("MsgHakSalUsage")]
    public string MsgHakSalUsage { get; set; } = "{White}Kullanım: !haksal <isim>";

    [JsonPropertyName("MsgHakSalTargetNotFound")]
    public string MsgHakSalTargetNotFound { get; set; } = "{White}Belirtilen T oyuncusu bulunamadı.";

    [JsonPropertyName("MsgHakSalApplied")]
    public string MsgHakSalApplied { get; set; } = "{White}{Red}{0}{White} hakkını {Red}{1}{White} oyuncusuna saldı!";


    // LR Messages
    [JsonPropertyName("MsgLROnlyLastT")]
    public string MsgLROnlyLastT { get; set; } = "{White}Bu komutu sadece sona kalan T kullanabilir.";

    [JsonPropertyName("MsgSonSecUsage")]
    public string MsgSonSecUsage { get; set; } = "{White}Kullanım: !sonseç <isim>";

    [JsonPropertyName("MsgSonSecApplied")]
    public string MsgSonSecApplied { get; set; } = "{White}{Red}{0}{White} sona bırakıldı ve Sona Kalan menüsü açıldı!";

    [JsonPropertyName("MsgLRStarted")]
    public string MsgLRStarted { get; set; } = "{White}Son İstek (LR) Başladı! {Red}{0}{White} vs {Red}{1}{White}";

    [JsonPropertyName("MsgLRTypeInfo")]
    public string MsgLRTypeInfo { get; set; } = "{White}Tür: {Red}{0}{White}";

    [JsonPropertyName("MsgLRDeagleTurn")]
    public string MsgLRDeagleTurn { get; set; } = "{White}Sıra sende! Ateş et.";

    [JsonPropertyName("MsgLRDeagleWait")]
    public string MsgLRDeagleWait { get; set; } = "{White}Rakibinin ateş etmesini bekle.";

    [JsonPropertyName("MsgLRCTLost")]
    public string MsgLRCTLost { get; set; } = "{White}{Red}{0}{White} Son İstek'i kaybettiği için T takımına atıldı!";

    [JsonPropertyName("MsgLRRebellion")]
    public string MsgLRRebellion { get; set; } = "{White}{Red}{0}{White} isyan etmeyi seçti!";

    [JsonPropertyName("MsgLRNoCTFound")]
    public string MsgLRNoCTFound { get; set; } = "{White}Düello yapacak CT bulunamadı.";


    // LR HUD Titles/Content
    [JsonPropertyName("HudTitleLRMain")]
    public string HudTitleLRMain { get; set; } = "SONA KALAN MENÜSÜ";

    [JsonPropertyName("HudTitleLRType")]
    public string HudTitleLRType { get; set; } = "SON İSTEK (LR) TÜRÜ SEÇİN";

    [JsonPropertyName("HudTitleLRTarget")]
    public string HudTitleLRTarget { get; set; } = "RAKİP SEÇİN";

    [JsonPropertyName("HudContentLRMain")]
    public string HudContentLRMain { get; set; } = "!1 Son İstek (LR)<br>!2 İsyan<br>!3 Kredi Al ({0})";

    [JsonPropertyName("HudContentLRType")]
    public string HudContentLRType { get; set; } = "!1 Deagle Düellosu<br>!2 Bıçak Düellosu";

    [JsonPropertyName("MsgLRChoiceLR")]
    public string MsgLRChoiceLR { get; set; } = "{White}{Red}{0}{White} son istek atmayı seçti!";

    [JsonPropertyName("MsgLRCreditReceived")]
    public string MsgLRCreditReceived { get; set; } = "{White}{Red}{0}{White} kredi alarak round'u bitirdi!";

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
    public string MsgFFWeaponSelected { get; set; } = "{White}{Red}{0}{White} seçildi! FF {Red}{1}{White} saniye sonra başlayacak.";

    [JsonPropertyName("MsgFFActiveNow")]
    public string MsgFFActiveNow { get; set; } = "{White}FF Aktif Edildi!";

    [JsonPropertyName("MsgFFOnlyKnife")]
    public string MsgFFOnlyKnife { get; set; } = "{White}Sadece Bıçak";

    [JsonPropertyName("MsgWardenAdminMenuTitle")]
    public string MsgWardenAdminMenuTitle { get; set; } = "{White}Komutçu Admin Seçimi";



    [JsonPropertyName("MsgSustumWinner")]
    public string MsgSustumWinner { get; set; } = "{White}{Red}{0}{White} kazananı: {Red}{1}{White}! Ödül: {Red}{2}{White}";


    [JsonPropertyName("MsgSustumRewardDeagle")]
    public string MsgSustumRewardDeagle { get; set; } = "{White}Tek mermili Deagle";


    [JsonPropertyName("MsgSustumRewardCT")]
    public string MsgSustumRewardCT { get; set; } = "{White}CT Takımına Geçiş";


    [JsonPropertyName("MsgSustumRewardRespawn")]
    public string MsgSustumRewardRespawn { get; set; } = "{White}Canlanma";


    [JsonPropertyName("MsgSustumExpired")]
    public string MsgSustumExpired { get; set; } = "{White}{Red}{0}{White} süresi doldu, kimse yazamadı.";


    [JsonPropertyName("MsgTFreezeStarted")]
    public string MsgTFreezeStarted { get; set; } = "{White}T takımı donduruldu!";


    [JsonPropertyName("MsgTFreezeEnded")]
    public string MsgTFreezeEnded { get; set; } = "{White}T takımının donması çözüldü!";


    [JsonPropertyName("MsgTFreezeCountdownStarted")]
    public string MsgTFreezeCountdownStarted { get; set; } = "{White}T takımı {Red}{0}{White} saniye sonra dondurulacak!";


    [JsonPropertyName("MsgTFreezeCountdownCancelled")]
    public string MsgTFreezeCountdownCancelled { get; set; } = "{White}Donma süresi sıfırlandı.";


    [JsonPropertyName("MsgIseliStarted")]
    public string MsgIseliStarted { get; set; } = "{White}İseli süreci başladı! Kapılar {Red}{0}{White} saniye sonra açılacak.";


    [JsonPropertyName("MsgIseliDoorsOpened")]
    public string MsgIseliDoorsOpened { get; set; } = "{White}İseli süresi doldu, tüm kapılar açıldı!";


    [JsonPropertyName("MsgIseliQuickOpened")]
    public string MsgIseliQuickOpened { get; set; } = "{White}İseli anında tamamlandı, tüm kapılar açıldı!";


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

    [JsonPropertyName("MsgCTRevUsage")]
    public string MsgCTRevUsage { get; set; } = "{White}Kullanım: !ctrev <isim>";

    [JsonPropertyName("MsgCTRevNoRevivesLeft")]
    public string MsgCTRevNoRevivesLeft { get; set; } = "{White}Canlandırma hakkınız kalmadı!";

    [JsonPropertyName("MsgCTRevTargetMustBeDeadCT")]
    public string MsgCTRevTargetMustBeDeadCT { get; set; } = "{White}Hedef oyuncu ölü bir CT olmalıdır.";

    [JsonPropertyName("MsgCTRevApplied")]
    public string MsgCTRevApplied { get; set; } = "{White}{Red}{0}{White} adlı CT canlandırıldı! (Kalan Hak: {Red}{1}{White})";

    [JsonPropertyName("MsgSustumStarted")]
    public string MsgSustumStarted { get; set; } = "{White}{Red}{0}{White} süreci başladı! Kelime: {Red}{1}{White}";

    [JsonPropertyName("HudTitleSustum")]
    public string HudTitleSustum { get; set; } = "SUSTUM SİSTEMİ";

    [JsonPropertyName("HudTitleFormation")]
    public string HudTitleFormation { get; set; } = "T TAKIMI DONDURULDU";

    [JsonPropertyName("HudContentFormation")]
    public string HudContentFormation { get; set; } = "Formasyon tamamlandı, T'ler donduruldu!";

    [JsonPropertyName("MsgGetCTApplied")]
    public string MsgGetCTApplied { get; set; } = "{White}CT takımı komutçunun yanına çekildi!";

    [JsonPropertyName("MsgGetAllApplied")]
    public string MsgGetAllApplied { get; set; } = "{White}Tüm oyuncular komutçunun yanına çekildi!";

    [JsonPropertyName("MsgAfApplied")]
    public string MsgAfApplied { get; set; } = "{White}Tüm oyuncular canlandırıldı ve canları 100 yapıldı!";

    [JsonPropertyName("HudTitleVoteCandidate")]
    public string HudTitleVoteCandidate { get; set; } = "Komutçu Adaylık Süreci";

    [JsonPropertyName("HudContentVoteCandidate")]
    public string HudContentVoteCandidate { get; set; } = "<font color='green'>{0}</font><br>Kalan Süre: {1} saniye<br><br>Aday olmak için <b>!komaday</b> yazın.<br>Adaylar:<br>";

    [JsonPropertyName("HudTitleVoteSelection")]
    public string HudTitleVoteSelection { get; set; } = "Komutçu Oylaması";

    [JsonPropertyName("HudContentVoteSelection")]
    public string HudContentVoteSelection { get; set; } = "<font color='blue'>{0}</font><br>Kalan Süre: {1} saniye<br><br>Oy vermek için sohbete numarayı yazın!<br>";

    [JsonPropertyName("HudTitleVoteKick")]
    public string HudTitleVoteKick { get; set; } = "Komutçu Oylaması: {0}";

    [JsonPropertyName("HudContentVoteKick")]
    public string HudContentVoteKick { get; set; } = "<font color='red'>{0}</font><br>Kalan Süre: {1} saniye<br><br>Oy vermek için sohbete numarayı yazın!<br>";


    [JsonPropertyName("MsgBunnyEnabled")]
    public string MsgBunnyEnabled { get; set; } = "{White}Bunnyhop açıldı!";


    [JsonPropertyName("MsgBunnyDisabled")]
    public string MsgBunnyDisabled { get; set; } = "{White}Bunnyhop kapatıldı!";


    [JsonPropertyName("MsgUnmuteCTApplied")]
    public string MsgUnmuteCTApplied { get; set; } = "{White}CT takımının mutesi açıldı!";


    [JsonPropertyName("MsgUnmuteTApplied")]
    public string MsgUnmuteTApplied { get; set; } = "{White}T takımının mutesi açıldı!";


    [JsonPropertyName("MsgSsApplied")]
    public string MsgSsApplied { get; set; } = "{White}T takımının silahları alındı!";


    [JsonPropertyName("MsgKacCmApplied")]
    public string MsgKacCmApplied { get; set; } = "{Red}{0}{White}'unki {Red}{1}cm";


    [JsonPropertyName("MsgTopKacCmTitle")]
    public string MsgTopKacCmTitle { get; set; } = "{Green}--- EN YÜKSEK KAÇ CM LİSTESİ ---";


    [JsonPropertyName("MsgTopKacCmEntry")]
    public string MsgTopKacCmEntry { get; set; } = "{White}{0}. {Red}{1}{White}: {Red}{2}cm";


    [JsonPropertyName("MsgTopKacCmEmpty")]
    public string MsgTopKacCmEmpty { get; set; } = "{White}Henüz kimse ölçüm yapmadı!";


    [JsonPropertyName("MsgMuteCTApplied")]
    public string MsgMuteCTApplied { get; set; } = "{White}CT takımının mutesi kapatıldı!";


    [JsonPropertyName("MsgMuteTApplied")]
    public string MsgMuteTApplied { get; set; } = "{White}T takımının mutesi kapatıldı!";


    [JsonPropertyName("MsgOtoresEnabled")]
    public string MsgOtoresEnabled { get; set; } = "{White}Otomatik canlanma {Green}açıldı!";


    [JsonPropertyName("MsgOtoresDisabled")]
    public string MsgOtoresDisabled { get; set; } = "{White}Otomatik canlanma {Red}kapatıldı!";


    [JsonPropertyName("MsgWardenTimeRemaining")]
    public string MsgWardenTimeRemaining { get; set; } = "{White}Mevcut komutçunun kalan süresi: {Red}{0}";


    [JsonPropertyName("HudTitleTopWarden")]
    public string HudTitleTopWarden { get; set; } = "EN ÇOK KOMUTÇULUK YAPANLAR";


    [JsonPropertyName("MsgTopWardenEmpty")]
    public string MsgTopWardenEmpty { get; set; } = "{White}Henüz komutçuluk yapan kimse kaydedilmedi!";
}
