# CS2 JailBreak Komutçu (Warden) Plugini

Counter-Strike 2 sunucuları için **CounterStrikeSharp** altyapısı ile geliştirilmiş, profesyonel ve zengin özelliklere sahip bir JailBreak yönetim eklentisidir. Bu plugin, komutçuların (Warden) rauntları, oyunları ve oyuncu formasyonlarını tamamen görsel (CenterHTML) menüler ve modern bir HUD ile yönetmesini sağlar.

---

## 🚀 Öne Çıkan Özellikler

### 👑 Merkezi Komutçu Sistemi
- **Unified Menu (`!k`):** Tüm komutçu yetkilerini (Kapı açma, Box, Saklambaç, FF, Revive vb.) tek bir görsel menüde toplayan devrimsel kontrol paneli.
- **Komutçu Oylaması:** Adaylık süreci ve demokratik oylama aşaması (`!komoyla`).
- **Warden Adminleri:** Komutçuların raundu yönetmesine yardımcı olan, yüksek yetkili asistanlar (`!ka`).
- **Görsel RGB Efekti:** Aktif komutçu, diğer oyunculardan ayırt edilebilmesi için sürekli renk değiştiren RGB efektine sahiptir.

### 🎮 Yeni Nesil Oyun Modları
- **Saklambaç (`!saklambac`):** 
    - CT takımı otomatik olarak spawn noktalarına ışınlanır.
    - CT'ler 180 derece ters döndürülür, dondurulur ve ekranları karartılır (Blind).
    - Saklanma süresi sonunda CT'ler çözülür, tüm T takımı otomatik dondurulur.
- **Boks Modu (`!box`):** 
    - Tek tıkla süre seçimi (10-60sn) yapılan, basitleştirilmiş boks arenas başlatan mod.
    - Süre sonunda dost ateşi otomatik kapanır.

### ⚔️ Gelişmiş FF & Silah Seçimi
- **Görsel Menü:** Artık chat'e yazmak yok! Komutçu, FF ayarlarını (silahlar, bunny hop, süre) tamamen görsel bir menü üzerinden yapar.
- **Oyuncu Seçimi:** FF başlamadan önce T oyuncuları kendi birincil ve ikincil silahlarını görsel menülerden seçer.
- **Akıllı HUD:** Seçim aşamasında ekran kirliliğini önlemek için sayaç gizlenir, seçimler bitince FF geri sayımı başlar.

### 🧊 Gelişmiş Dondurma & Hareket Servisi
- **Merkezi Kontrol:** Mouse hareketine izin veren, fiziksel hareketi engelleyen optimize edilmiş dondurma sistemi.
- **Otomatik Formasyonlar:** T oyuncularını anında daire (`!daire`) veya sıra (`!diz`) şeklinde hizalama.

---

## 🛠️ Komutlar

### Komutçu & Yönetim
| Komut | Alternatif | Açıklama |
| :--- | :--- | :--- |
| `!k` | `!kommenu` | **Warden Ana Menüsünü** açar (Önerilen) |
| `!w` | `!warden` | Komutçu ol |
| `!uw` | `!unwarden` | Komutçuluğu bırak |
| `!ka <isim>` | - | Komutçu Admini atar |
| `!kasil` | - | Komutçu Admin yetkisini kaldırır |

### Oyun Modları
| Komut | Alternatif | Açıklama |
| :--- | :--- | :--- |
| `!saklambac <sn>` | `!saklambaç` | Saklambaç modunu başlatır (Default 30sn) |
| `!box <sn>` | `!b` | Boks modunu başlatır (Default 30sn) |
| `!ffmenu` | - | Özelleştirilebilir FF menüsünü açar |
| `!ffkapat` | `!ffk` | FF'i anında sonlandırır |
| `!ff0` | - | FF'i kapatır ve tüm T'lerin silahlarını siler |

### Duyuru & Broadcast
| Komut | Yetki | Açıklama |
| :--- | :--- | :--- |
| `!msay <mesaj>` | Admin | Ekranın ortasında büyük duyuru kutusu açar |
| `!csay <mesaj>` | Admin | Ekranın alt-orta kısmında renkli yazı yazar |
| `!hsay <mesaj>` | Admin | HUD (Hint) kısmında duyuru yapar |
| `!fsay <isim> <msg>`| Root | Belirtilen oyuncunun ağzından chat'e yazı yazar |

### Yardımcı Komutlar
| Komut | Açıklama |
| :--- | :--- |
| `!rev <hedef>` | Belirtilen hedefi (`@t`, `@ct`, `@all` veya isim) canlandırır |
| `!af` | Herkesi canlandırır ve canlarını 100 yapar |
| `!iseli <sn>` | Geri sayımlı hücre kapısı açma |
| `!iq` | Tüm kapıları anında açar |
| `!td` / `!tdb` | T takımını dondurur / çözer |
| `!ss` / `!strip` | T takımının tüm silahlarını alır |
| `!marker <boyut>` | Komutçu işaretleyici boyutunu ayarla |

---

## ⚙️ Yapılandırma & Yetkiler

- **Dosya Yolları:**
    - Ana Ayarlar: `configs/plugins/JailBreak/JailBreak.json`
    - Dil & Mesajlar: `configs/plugins/JailBreak/lang.json`
- **Yetki Sistemi:** 
    - `@css/root`: `!fsay` ve tüm yönetim komutları.
    - `@css/ban`: Kritik denge komutları (`!rev`, `!hpa`, `!haksal`).
    - `@jailbreak/ka`: Komutçunun sahip olduğu tüm oyun içi yetkiler.

---

## 🎨 HUD Tasarım Standartları
Tüm görsel geri bildirimler profesyonel bir JailBreak deneyimi için standardize edilmiştir:
- 🟡 **Altın (Gold):** Başlıklar.
- 🔴 **Kırmızı (Red):** Zamanlayıcılar ve Kritik uyarılar.
- 🟢 **Yeşil (Green):** Başarı ve Aktif durumlar.
- 🔵 **Cyan:** Sistem bilgilendirmeleri.

---

## 📜 Krediler
CS2 JailBreak topluluğu için ❤️ ile geliştirildi.
**CounterStrikeSharp** ve **CS2MenuManager** tarafından desteklenmektedir.
