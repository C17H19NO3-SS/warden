# CS2 JailBreak Komutçu (Warden) Plugini - Profesyonel Yönetim Paneli

Counter-Strike 2 sunucuları için **CounterStrikeSharp** altyapısı ile geliştirilmiş, Türkiye JailBreak topluluğunun ihtiyaçlarına göre modernize edilmiş en kapsamlı yönetim eklentisidir.

---

## 👑 Merkezi Komutçu Sistemi

Bu plugin, komutçunun (Warden) oyunu yönetmesini kolaylaştırmak için tüm karmaşık komutları tek bir noktada toplar.

-   **👑 Komutçu Ana Menüsü (`!k` / `!kommenu`):** Tüm raunt yönetimi, oyun modları ve ayarlar bu görsel panelde birleşir.
-   **🌈 RGB Warden:** Aktif komutçu sürekli renk değiştiren özel bir efekte sahiptir.
-   **📊 İstatistik Takibi:** En çok komutçu olanlar ve görev süreleri otomatik kaydedilir (`!topkomutcu`).
-   **🛡️ Dokunulmazlık Koruması:** Komutçular ve asistanları görevleri süresince `100 Immunity` kazanır, görev sonunda orijinal yetki seviyelerine geri dönerler.

---

## 🛠️ Tüm Komutlar ve Detaylı Açıklamalar

### 1. Komutçu ve Yönetim Komutları
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!k` | `!kommenu` | Komutçu | **Master Panel:** Tüm yetkilere görsel bir menüden erişmenizi sağlar. |
| `!w` | `!warden` | Herkes (CT) | Komutçu koltuğu boşsa sizi komutçu yapar. |
| `!uw` | `!unwarden` | Komutçu | Komutçuluğu bırakmanızı sağlar. |
| `!ka <isim>` | - | `@css/cvar` | Seçilen oyuncuyu "Komutçu Admini" yapar. |
| `!kasil <isim>` | - | `@css/ban` | Komutçu Admini yetkisini geri alır. |
| `!komkalan` | - | Herkes | Mevcut komutçunun görev süresinin bitmesine ne kadar kaldığını gösterir. |
| `!topkomutcu` | - | Herkes | En çok ve en uzun süre komutçuluk yapanları listeler. |
| `!topka` | - | Herkes | En çok ve en uzun süre komutçu admin olanları listeler. |
| `!q` | - | `@css/generic` | **Warden Koruma:** Tüm CT takımına God verir ve herkesin canını 100 yapar. |
| `!qq` | - | `@css/generic` | **Koruma Kapat:** CT'lerin God modunu kaldırır. |
| `!komoyla` | - | `@css/vote` | Komutçu oylamasını başlatır. |
| `!komaday` | - | Herkes | Komutçu oylamasına aday olarak katılır. |
| `!komdk` | - | `@css/vote` | Komutçuyu görevden alma oylaması başlatır. |

### 2. Oyun Modları (Game Modes)
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!saklambac <sn>`| `!saklambaç`| `@css/changemap`| **Saklambaç:** CT'ler spawn'a ışınlanır ve kör edilir. Süre sonunda T'ler dondurulur. |
| `!box <sn>` | `!b` | `@css/changemap`| **Boks Modu:** Dost ateşini açar, süre sonunda otomatik kapatır. |
| `!ffdondur <sn>`| `!ffondur` | `@css/changemap`| FF'i açar ve süre bittiğinde tüm T takımını dondurur. |
| `!dsustum` | - | `@css/slay` | Kelimeyi ilk yazana **Deagle** ödülü verir. |
| `!tsustum` | - | `@css/slay` | Kelimeyi ilk yazana **CT** takımına geçme ödülü verir. |
| `!olusustum` | - | `@css/slay` | Kelimeyi ilk yazan ölü oyuncuyu **canlandırır**. |

### 3. Dost Ateşi (FF) ve Silah Yönetimi
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!ffmenu` | - | `@css/changemap`| **Görsel FF Ayarı:** Silahları ve Bunny durumunu ayarladığınız paneldir. |
| `!ffkapat` | `!ffk` | `@css/changemap`| Aktif olan tüm FF süreçlerini anında durdurur. |
| `!ff0` | - | `@css/changemap`| FF'i kapatır ve tüm T'lerin silahlarını siler. |
| `!ss` | `!strip` | `@css/slay` | T takımının tüm silahlarını anında temizler. |

### 4. Dondurma ve Formasyonlar
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!td` | - | `@css/slay` | Tüm T takımını oldukları yerde dondurur. |
| `!tdb` | `!coz` | `@css/slay` | Donmuş olan oyuncuların dondurmasını çözer. |
| `!fz <sn>` | - | `@css/slay` | Belirlenen süre sonunda herkesi otomatik dondurur. |
| `!fz0` | - | `@css/slay` | Aktif dondurma sayacını iptal eder. |
| `!daire <gen>` | - | `@css/kick` | T takımını bakılan noktada daire şeklinde dizer. |
| `!diz <mes>` | - | `@css/kick` | T takımını bakılan noktada düz bir sıraya dizer. |
| `!gom <isim>` | - | `@css/slay` | Hedef oyuncuyu yere gömer ve dondurur. |
| `!gom0 <isim>`| - | `@css/slay` | Oyuncuyu gömülmekten çıkarır ve dondurmasını çözer. |

### 5. Işınlanma ve Can Yönetimi
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!rev <hedef>` | `!kaldır` | `@css/slay` | Hedefi (`@t`, `@ct`, `@all` veya isim) canlandırır. |
| `!af` | - | `@css/slay` | Herkesi canlandırır ve canlarını 100 yapar. |
| `!hpa` | - | `@css/slay` | Yaşayan tüm oyuncuların canını 100 yapar. |
| `!hpt` | - | `@css/slay` | Sadece T takımının canını 100 yapar. |
| `!hpct` | - | `@css/slay` | Sadece CT takımının canını 100 yapar. |
| `!gelt` | - | `@css/kick` | Tüm T takımını komutçunun yanına ışınlar. |
| `!gelct` | - | `@css/kick` | Tüm CT takımını komutçunun yanına ışınlar. |
| `!gelall` | - | `@css/kick` | Sunucudaki herkesi komutçunun yanına ışınlar. |
| `!git <isim>` | - | `@css/kick` | Belirlenen oyuncunun yanına ışınlar. |
| `!haksal <isim>`| - | `@css/generic` | Bir CT ile bir T'nin yerini (takımını) değiştirir. |

### 6. Duyuru ve Broadcast Komutları
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!msay <mesaj>` | - | `@css/chat` | Ekranın ortasında büyük duyuru kutusu açar. |
| `!csay <mesaj>` | - | `@css/chat` | Ekranın alt-orta kısmında renkli duyuru yapar. |
| `!hsay <mesaj>` | - | `@css/chat` | HUD (Hint) bölgesinde mesaj gösterir. |
| `!marker <boy>` | - | `@css/chat` | Komutçu işaretçisinin boyutunu ayarlar. |
| `!fsay <isim> <msg>`| - | `@css/root` | Belirlenen oyuncunun adıyla chat'e yazı yazar. |

### 7. Genel ve Diğer Komutlar
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!iseli <sn>` | - | `@css/changemap`| Geri sayımlı hücre kapısı açma. |
| `!iq` | - | `@css/changemap`| Haritadaki tüm kapıları anında açar. |
| `!ba` / `!bk` | - | `@css/generic` | Bunny Hop özelliğini açar veya kapatır. |
| `!mct` / `!umct` | - | `@css/generic` | CT takımını mutele veya mutesini aç. |
| `!mt` / `!umt` | - | `@css/generic` | T takımını mutele veya mutesini aç. |
| `!otores` / `!otores0`| - | `@css/generic` | Ölenlerin otomatik canlanmasını ayarlar. |
| `!reloadconfig` | - | `@css/root` | Ayarları ve dil dosyasını yeniler. |
| `!kaccm` | - | Herkes | Eğlence amaçlı boy ölçümü yapar. |
| `!topkaccm` | - | Herkes | Sunucunun en yüksek skorlarını listeler. |
| `!delay` | - | Herkes | 3 saniyelik ses gecikmesini (delay) giderir. |
| `!isyancilar` | - | Herkes | Gardiyanlara saldıran isyancıları listeler. |
| `!sonakalan` | - | Herkes (Son T)| **LR (Sona Kalan Menüsü)** açar. |
| `!sonsec` | `!sonseç` | `@css/slay` | Sona kalan T dışındakileri öldürür ve LR açar. |

---

## ⚙️ Yetkilendirme Standartları (Tam Liste)

Eklenti, komutların oyun içi etkisine göre şu hiyerarşide yapılandırılmıştır:

### 💻 1. @css/root (Sistem Sahibi)
-   `!reloadconfig`, `!fsay`

### 👑 2. @css/ban (Üst Yönetim)
-   `!kasil`

### 🛡️ 3. @css/generic (Standart Admin)
-   `!ba`, `!bk`, `!mct`, `!umct`, `!mt`, `!umt`
-   `!haksal`, `!otores`, `!otores0`

### 🔧 4. @css/cvar (Yetki Admini)
-   `!ka`

### 🗳️ 5. @css/vote (Oylama Admini)
-   `!komoyla`, `!komdk`

### 🎮 6. @css/changemap (Mod & Harita Admini)
-   `!saklambac`, `!box`, `!b`, `!ffmenu`, `!ffkapat`, `!ff0`, `!ffdondur`
-   `!iseli`, `!iq`

### 🏃 7. @css/kick (Hareket & Konum Admini)
-   `!daire`, `!diz`, `!git`, `!gelt`, `!gelct`, `!gelall`

### 👊 8. @css/slay (Müdahale & Can Admini)
-   `!rev`, `!af`, `!hpa`, `!hpt`, `!hpct`
-   `!td`, `!tdb`, `!fz`, `!fz0`, `!ss`, `!strip`
-   `!dsustum`, `!tsustum`, `!olusustum`, `!sonsec`

### 💬 9. @css/chat (Duyuru & İletişim Admini)
-   `!msay`, `!csay`, `!hsay`, `!marker`

### 👑 10. Komutçu (Warden)
-   `!k`, `!kommenu`
-   Warden ve Asistanları o an oyunda olan **tüm** yukarıdaki komutları kullanabilir.

### 👥 11. Herkes Tarafından Kullanılabilenler
-   `!w`, `!uw`, `!komkalan`, `!topkomutcu`, `!topka`, `!isyancilar`, `!sonakalan`, `!delay`, `!kaccm`, `!topkaccm`, `!komaday`.

---

## 📜 Krediler
CS2 JailBreak topluluğu için ❤️ ile geliştirildi.
**CounterStrikeSharp** ve **CS2MenuManager** tarafından desteklenmektedir.
