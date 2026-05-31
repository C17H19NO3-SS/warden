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
| `!k` | `!kommenu` | Komutçu/Admin | **Master Panel:** Tüm yetkilere görsel bir menüden erişmenizi sağlar. |
| `!w` | `!warden` | Herkes (CT) | Komutçu koltuğu boşsa sizi komutçu yapar. |
| `!uw` | `!unwarden` | Komutçu | Komutçuluğu bırakmanızı sağlar. |
| `!ka <isim>` | - | Root/Admin | Seçilen oyuncuyu "Komutçu Admini" yapar. Komutçu ile aynı yetkilere sahip olur. |
| `!kasil <isim>` | - | Root/Admin | Komutçu Admini yetkisini geri alır. |
| `!komkalan` | - | Herkes | Mevcut komutçunun görev süresinin bitmesine ne kadar kaldığını gösterir. |
| `!topkomutcu` | - | Herkes | Sunucu genelinde en çok ve en uzun süre komutçuluk yapanların listesini açar. |

### 2. Oyun Modları (Game Modes)
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!saklambac <sn>`| `!saklambaç`| Komutçu/Admin | **Saklambaç:** CT'ler spawn'a ışınlanır, ters döndürülür, dondurulur ve ekranları karartılır. Süre sonunda CT'ler çözülür, T'ler otomatik dondurulur. |
| `!box <sn>` | `!b` | Komutçu/Admin | **Boks Modu:** Hızlıca dost ateşini (FF) açar. Belirlenen süre sonunda FF otomatik kapanır. |
| `!ffdondur <sn>`| `!ffondur` | Komutçu/Admin | FF'i anında açar ve geri sayım başlatır. Süre bittiğinde FF kapanır ve tüm T takımı dondurulur. |
| `!dsustum` | - | Komutçu/Admin | Chat'e belirtilen kelimeyi ilk yazan oyuncuya **Deagle** (tek mermi) ödülü verir. |
| `!tsustum` | - | Komutçu/Admin | Kazanan Teröristi anında **Counter-Terrorist** takımına transfer eder. |
| `!olusustum` | - | Komutçu/Admin | Ölü olan ve kelimeyi ilk yazan oyuncuyu **yeniden canlandırır**. |

### 3. Dost Ateşi (FF) ve Silah Yönetimi
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!ffmenu` | - | Komutçu/Admin | **Görsel FF Ayarı:** Birincil/İkincil silahları seçebileceğiniz, Bunny Hop durumunu ayarlayabileceğiniz kontrol panelini açar. |
| `!ffkapat` | `!ffk` | Komutçu/Admin | Aktif olan tüm FF süreçlerini ve dost ateşini anında kapatır. |
| `!ff0` | - | Komutçu/Admin | FF'i kapatır ve tüm Teröristlerin silahlarını silerek sadece bıçak bırakır. |
| `!ss` | `!strip` | Komutçu/Admin | T takımının tüm silahlarını anında temizler. |

### 4. Dondurma ve Formasyonlar
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!td` | `!stop` | Komutçu/Admin | Tüm T takımını oldukları yerde dondurur (Hareket kısıtlanır, bakış serbest). |
| `!tdb` | `!coz` | Komutçu/Admin | Donmuş olan oyuncuların hareket yeteneğini geri verir. |
| `!fz <sn>` | - | Komutçu/Admin | Belirlenen süre sonunda (HUD sayacı ile) herkesi otomatik dondurur. |
| `!fz0` | - | Komutçu/Admin | Aktif olan `!fz` geri sayımını iptal eder. |
| `!daire <gen>` | - | Komutçu/Admin | T takımını komutçunun baktığı noktada daire şeklinde hizalar. |
| `!diz <mes>` | - | Komutçu/Admin | T takımını komutçunun önünde düz bir sıra halinde dizer. |

### 5. Işınlanma ve Can Yönetimi
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!rev <hedef>` | `!kaldır` | Admin (@ban) | Belirlenen hedefi (`@t`, `@ct`, `@all` veya isim) anında canlandırır. |
| `!af` | - | Admin (@ban) | Ölü herkesi canlandırır ve tüm yaşayanların canını 100 yapar. |
| `!hpa` | - | Admin (@ban) | Yaşayan tüm oyuncuların canını 100'e sabitler. |
| `!hpt` / `!hpct` | - | Komutçu/Admin | Sadece belirlenen takımın canını 100 yapar. |
| `!gelt` / `!gelct` | - | Komutçu/Admin | Belirlenen takımın tamamını komutçunun yanına ışınlar. |
| `!gelall` | - | Admin (@ban) | Sunucudaki herkesi komutçunun yanına ışınlar. |
| `!git <isim>` | - | Admin (@kick) | Komutçuyu belirlenen oyuncunun yanına ışınlar. |
| `!haksal <isim>`| - | Admin (@ban) | Bir CT ile bir T'nin yerini (takımını) anında değiştirir. |

### 6. Duyuru ve Broadcast Komutları
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!msay <mesaj>` | - | Admin (@ban) | Ekranın ortasında büyük bir duyuru kutusu açar (10 saniye kalır). |
| `!csay <mesaj>` | - | Admin (@ban) | Ekranın alt-orta (Center) kısmında kırmızı renkli büyük duyuru yapar. |
| `!hsay <mesaj>` | - | Admin (@ban) | HUD (Hint) bölgesinde (ekranın en altı) mesaj gösterir. |
| `!fsay <isim> <msg>`| - | Root | **Fake Say:** Belirlenen oyuncunun ismini kullanarak chat'e mesaj yazdırır. |

### 7. Genel ve Diğer Komutlar
| Komut | Alternatif | Yetki | Açıklama |
| :--- | :--- | :--- | :--- |
| `!iseli <sn>` | - | Komutçu/Admin | Geri sayım başlatır ve süre sonunda tüm hücre kapılarını açar. |
| `!iq` | - | Komutçu/Admin | Haritadaki tüm kapıları (hücre, kapı, kırılabilir cam) anında açar. |
| `!marker <boy>` | - | Komutçu/Admin | Komutçu işaretleyicisinin (Mouse3) boyutunu ayarlar. |
| `!ba` / `!bk` | - | Admin (@ban) | Sunucu genelinde Bunny Hop özelliğini açar veya kapatır. |
| `!mct` / `!umct` | - | Admin (@ban) | CT takımını toplu susturur veya mutesini açar. |
| `!mt` / `!umt` | - | Admin (@ban) | T takımını toplu susturur veya mutesini açar. |
| `!kaccm` | - | Herkes | Eğlence amaçlı uzunluk ölçümü yapar ve en yüksek skoru kaydeder. |
| `!topkaccm` | - | Herkes | Sunucunun en yüksek `kaccm` skorlarını listeler. |
| `!isyancilar` | - | Herkes | Mevcut rauntta gardiyanlara saldıran isyancıları listeler. |
| `!sonakalan` | - | Herkes (Son T)| Sona kalan mahkumun LR (Son İstek) menüsünü açmasını sağlar. |
| `!reloadconfig` | - | Root | Eklenti ayarlarını ve dil dosyasını diskten yeniden yükler. |

---

## ⚙️ Teknik Detaylar ve Kurulum

### Yetki Sistemi (Default Permissions)
Eklenti, Türkiye hiyerarşisine uygun olarak yapılandırılmıştır:
-   **@css/root:** Tüm komutlar ve `!fsay`, `!reloadconfig`.
-   **@css/ban:** Kritik yönetim komutları (`!rev`, `!af`, `!haksal`, `!msay`).
-   **@jailbreak/ka:** Aktif Komutçu ve Komutçu Adminlerinin sahip olduğu tüm oyun içi yetkiler.

### HUD Tasarım Standartları
Tüm görsel geri bildirimler (CenterHTML) şu renk şemasına sadıktır:
-   🟡 **Gold:** Başlıklar.
-   🔴 **Red:** Sayaçlar ve kritik uyarılar.
-   🟢 **Green:** Başarı mesajları ve ödüller.
-   🔵 **Cyan:** Sistem bilgilendirmeleri.

---

## 📜 Krediler
CS2 JailBreak topluluğu için ❤️ ile geliştirildi.
**CounterStrikeSharp** ve **CS2MenuManager** tarafından desteklenmektedir.
