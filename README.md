# CS2 JailBreak Komutçu (Warden) Plugini - Profesyonel Yönetim Paneli

Counter-Strike 2 sunucuları için **CounterStrikeSharp** altyapısı ile geliştirilmiş, Türkiye JailBreak topluluğunun ihtiyaçlarına göre modernize edilmiş en kapsamlı yönetim eklentisidir.

---

## 👑 Merkezi Komutçu Sistemi

Bu plugin, komutçunun (Warden) oyunu yönetmesini kolaylaştırmak için tüm karmaşık komutları tek bir noktada toplar.

-   **👑 Komutçu Ana Menüsü (`!k` / `!kommenu`):** Tüm raunt yönetimi, oyun modları ve ayarlar bu görsel panelde birleşir.
-   **🌈 RGB Warden:** Aktif komutçu sürekli renk değiştiren özel bir efekte sahiptir.
-   **📊 İstatistik Takibi:** En çok komutçu olanlar ve görev süreleri otomatik kaydedilir (`!topkomutcu`).

---

## 🛠️ Tüm Komutlar ve Detaylı Açıklamalar

### 1. Komutçu ve Yönetim Komutları
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!k` | `!kommenu` | `@css/generic` | **Master Panel:** Tüm yetkilere görsel bir menüden erişmenizi sağlar. |
| `!w` | `!warden` | Herkes (CT) | Komutçu koltuğu boşsa sizi komutçu yapar. |
| `!uw` | `!unwarden` | Komutçu | Komutçuluğu bırakmanızı sağlar. |
| `!ka <isim>` | - | `@css/ban` | Seçilen oyuncuyu "Komutçu Admini" yapar. Komutçu ile aynı yetkilere sahip olur. |
| `!kasil <isim>` | - | `@css/ban` | Komutçu Admini yetkisini geri alır. |
| `!komkalan` | - | Herkes | Mevcut komutçunun görev süresinin bitmesine ne kadar kaldığını gösterir. |
| `!topkomutcu` | - | Herkes | Sunucu genelinde en çok ve en uzun süre komutçuluk yapanların listesini açar. |

### 2. Oyun Modları (Game Modes)
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!saklambac <sn>`| `!saklambaç`| `@css/changemap`| **Saklambaç:** CT'ler spawn'a ışınlanır, ters döndürülür, dondurulur ve ekranları karartılır. Süre sonunda CT'ler çözülür, T'ler otomatik dondurulur. |
| `!box <sn>` | `!b` | `@css/changemap`| **Boks Modu:** Hızlıca dost ateşini (FF) açar. Belirlenen süre sonunda FF otomatik kapanır. |
| `!ffdondur <sn>`| `!ffondur` | `@css/changemap`| FF'i anında açar ve geri sayım başlatır. Süre bittiğinde FF kapanır ve tüm T takımı dondurulur. |
| `!dsustum` | - | `@css/chat` | Chat'e belirtilen kelimeyi ilk yazan oyuncuya **Deagle** (tek mermi) ödülü verir. |
| `!tsustum` | - | `@css/chat` | Kazanan Teröristi anında **Counter-Terrorist** takımına transfer eder. |
| `!olusustum` | - | `@css/chat` | Ölü olan ve kelimeyi ilk yazan oyuncuyu **yeniden canlandırır**. |

### 3. Dost Ateşi (FF) ve Silah Yönetimi
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!ffmenu` | - | `@css/changemap`| **Görsel FF Ayarı:** Birincil/İkincil silahları seçebileceğiniz, Bunny Hop durumunu ayarlayabileceğiniz kontrol panelini açar. |
| `!ffkapat` | `!ffk` | `@css/changemap`| Aktif olan tüm FF süreçlerini ve dost ateşini anında kapatır. |
| `!ff0` | - | `@css/changemap`| FF'i kapatır ve tüm Teröristlerin silahlarını silerek sadece bıçak bırakır. |
| `!ss` | `!strip` | `@css/slay` | T takımının tüm silahlarını anında temizler. |

### 4. Dondurma ve Formasyonlar
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!td` | - | `@css/slay` | Tüm T takımını oldukları yerde dondurur (Hareket kısıtlanır, bakış serbest). |
| `!tdb` | `!coz` | `@css/slay` | Donmuş olan oyuncuların hareket yeteneğini geri verir. |
| `!fz <sn>` | - | `@css/slay` | Belirlenen süre sonunda (HUD sayacı ile) herkesi otomatik dondurur. |
| `!fz0` | - | `@css/slay` | Aktif olan `!fz` geri sayımını iptal eder. |
| `!daire <gen>` | - | `@css/kick` | T takımını komutçunun baktığı noktada daire şeklinde hizalar. |
| `!diz <mes>` | - | `@css/kick` | T takımını komutçunun önünde düz bir sıra halinde dizer. |

### 5. Işınlanma ve Can Yönetimi
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!rev <hedef>` | `!kaldır` | `@css/ban` | Belirlenen hedefi (`@t`, `@ct`, `@all` veya isim) anında canlandırır. |
| `!af` | - | `@css/ban` | Ölü herkesi canlandırır ve tüm yaşayanların canını 100 yapar. |
| `!hpa` | - | `@css/ban` | Yaşayan tüm oyuncuların canını 100'e sabitler. |
| `!hpt` / `!hpct` | - | `@css/slay` | Sadece belirlenen takımın canını 100 yapar. |
| `!gelt` / `!gelct` | - | `@css/kick` | Belirlenen takımın tamamını komutçunun yanına ışınlar. |
| `!gelall` | - | `@css/ban` | Sunucudaki herkesi komutçunun yanına ışınlar. |
| `!git <isim>` | - | `@css/kick` | Komutçuyu belirlenen oyuncunun yanına ışınlar. |
| `!haksal <isim>`| - | `@css/ban` | Bir CT ile bir T'nin yerini (takımını) anında değiştirir. |

### 6. Duyuru ve Broadcast Komutları
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!msay <mesaj>` | - | `@css/chat` | Ekranın ortasında büyük bir duyuru kutusu açar (10 saniye kalır). |
| `!csay <mesaj>` | - | `@css/chat` | Ekranın alt-orta (Center) kısmında renkli yazı yazar. |
| `!hsay <mesaj>` | - | `@css/chat` | HUD (Hint) bölgesinde (ekranın en altı) mesaj gösterir. |
| `!fsay <isim> <msg>`| - | `@css/ban` | **Fake Say:** Belirlenen oyuncunun ismini kullanarak chat'e mesaj yazdırır. |

### 7. Genel ve Diğer Komutlar
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!iseli <sn>` | - | `@css/changemap`| Geri sayım başlatır ve süre sonunda tüm hücre kapılarını açar. |
| `!iq` | - | `@css/changemap`| Haritadaki tüm kapıları (hücre, kapı, kırılabilir cam) anında açar. |
| `!marker <boy>` | - | `@css/chat` | Komutçu işaretleyicisinin (Mouse3) boyutunu ayarlar. |
| `!ba` / `!bk` | - | `@css/ban` | Sunucu genelinde Bunny Hop özelliğini açar veya kapatır. |
| `!mct` / `!umct` | - | `@css/ban` | CT takımını toplu susturur veya mutesini açar. |
| `!mt` / `!umt` | - | `@css/ban` | T takımını toplu susturur veya mutesini açar. |
| `!kaccm` | - | Herkes | Eğlence amaçlı uzunluk ölçümü yapar ve en yüksek skoru kaydeder. |
| `!topkaccm` | - | Herkes | Sunucunun en yüksek `kaccm` skorlarını listeler. |
| `!isyancilar` | - | Herkes | Mevcut rauntta gardiyanlara saldıran isyancıları listeler. |
| `!sonakalan` | - | Herkes (Son T)| Sona kalan mahkumun LR (Son İstek) menüsünü açmasını sağlar. |
| `!reloadconfig` | - | `@css/ban` | Eklenti ayarlarını ve dil dosyasını diskten yeniden yükler. |

---

## ⚙️ Yetkilendirme Standartları

Bu eklenti, sunucu yetkililerini teknik detaylarla yormadan, sadece standart **Admin Flag'lerini** (Bayraklarını) kullanacak şekilde optimize edilmiştir. `Root`, `Cvar`, `Cheats` gibi kritik sistem bayrakları yerine, oyun içi yetki seviyeleri şu şekilde hiyerarşize edilmiştir:

1.  **💻 @css/root (Sistem):** `!reloadconfig` ve tüm teknik yönetim yetkileri.
2.  **👑 @css/ban (Kritik Yetki):** Sunucu dengesini değiştiren en üst düzey admin komutları. (`!rev`, `!af`, `!haksal`, `!fsay`, `!ka`).
2.  **🎮 @css/changemap (Mod Yönetimi):** Oyunun genel modunu ve harita mekaniklerini yöneten komutlar. (`!saklambac`, `!box`, `!ffmenu`, `!iseli`).
3.  **🏃 @css/kick (Hareket Yönetimi):** Oyuncuların fiziksel konumlarını ve dizilişlerini yöneten komutlar. (`!gelt`, `!git`, `!daire`, `!diz`).
4.  **👊 @css/slay (Fiziksel Müdahale):** Oyuncuların can ve dondurma durumlarını etkileyen komutlar. (`!td`, `!fz`, `!hpt`, `!hpct`).
5.  **💬 @css/chat (İletişim & Mini Oyunlar):** Duyuru komutları ve sohbet tabanlı oyunlar. (`!msay`, `!sustum`, `!marker`).
6.  **🛡️ @jailbreak/ka (Komutçu Admin):** O anki raundu yöneten yetkililerin sahip olduğu sınırsız oyun içi yetki.

---

## 📜 Krediler
CS2 JailBreak topluluğu için ❤️ ile geliştirildi.
**CounterStrikeSharp** ve **CS2MenuManager** tarafından desteklenmektedir.
