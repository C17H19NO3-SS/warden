# CS2 JailBreak Komutçu (Warden) Plugini

Counter-Strike 2 sunucuları için **CounterStrikeSharp** altyapısı ile geliştirilmiş, profesyonel ve zengin özelliklere sahip bir JailBreak yönetim eklentisidir. Bu plugin, komutçuların (Warden) rauntları, oyunları ve oyuncu formasyonlarını kolayca ve görsel bir netlikle yönetmesini sağlar.

---

## 🚀 Özellikler

### 👑 Komutçu & Admin Sistemi
- **Komutçu Oylaması:** Adaylık süreci ve oylama aşaması içeren otomatik oylama sistemi (`!komoyla`).
- **Hızlı Erişim:** Komutçu olma (`!w`) ve bırakma (`!uw`) komutları.
- **Komutçu Adminleri:** Komutçular, raundu yönetmelerine yardımcı olacak asistanlar (`!ka`) atayabilir.
- **Görsel Kimlik:** Komutçular ve Adminler için özel chat tagları, renkleri ve RGB model efektleri.

### 🔫 Gelişmiş FF & Silah Menüsü
- **İki Aşamalı Seçim:** Komutçu, tüm Terörist takımı için önce bir ana silah, ardından bir tabanca seçer.
- **Hazırlık Geri Sayımı:** Silahlar seçildikten sonra FF açılmadan önce HUD üzerinde bir geri sayım başlar, böylece oyuncular hazırlanabilir.
- **Otomatik Envanter:** Eski silahları otomatik olarak temizler ve seçilen yeni silah setini verir.
- **FF Kontrolü:** FF'i kapatma (`!ffkapat`) veya tüm silahları alıp sadece bıçak bırakma (`!ff0`).

### 🧊 Merkezi Dondurma (Freeze) Servisi
- **Güçlü Dondurma:** Mouse hareketine izin verirken tüm fiziksel hareketi engeller.
- **Gecikmeli Dondurma:** Belirlenen süre sonunda tüm T'leri dondurmak için zamanlayıcı (`!fz <saniye>`).
- **Görsel Geri Bildirim:** Standartlaştırılmış HUD başlıkları ve oyuncular dondurulduğunda net mesajlar.
- **FF Entegrasyonu:** `!ffondur <saniye>` komutu ile geri sayım yapın, FF'i kapatın ve herkesi aynı anda dondurun.

### 🎮 Oyun Servisleri
- **Sustum Modları:** Üç farklı rekabetçi kelime yazma oyunu (Deagle, CT'ye Geçiş, Canlanma ödüllü).
- **İseli Servisi:** Geri sayımlı ve anında açma seçenekli otomatik kapı kontrolü.
- **Marker Sistemi:** Komutçu, yer pinleme tuşu (Mouse3) ile ayarlanabilir boyutlarda (`!marker`) RGB çemberler oluşturabilir.
- **Pozisyonlandırma:** T'leri komutçunun baktığı yere göre anında daire (`!daire`) veya sıra (`!diz`) şeklinde dizer.

---

## 🛠️ Komutlar

### Komutçu & Oylama
| Komut | Açıklama |
| :--- | :--- |
| `!w` | Komutçu ol (eğer yoksa) |
| `!uw` | Komutçuluğu bırak |
| `!komoyla` | Komutçu seçimi için oylama başlatır |
| `!komaday` | Devam eden oylamaya aday olarak katılır |
| `!ka <isim>` | Bir oyuncuyu Komutçu Admini olarak atar |
| `!kasil` | Komutçu Admin yetkisini kaldırır |

### FF Menüsü
| Komut | Açıklama |
| :--- | :--- |
| `!ffmenu <sn>` | İki aşamalı silah menüsünü ve hazırlık süresini başlatır |
| `!ffondur <sn>` | FF kapatma geri sayımını başlatır -> FF Kapanır -> Herkes Dondurulur |
| `!ffkapat` / `!ffk` | FF'i anında kapatır |
| `!ff0` | FF'i kapatır ve T'lerin tüm silahlarını alıp sadece bıçak bırakır |

### Dondurma & Hareket
| Komut | Açıklama |
| :--- | :--- |
| `!td` | Tüm Teröristleri anında dondurur |
| `!tdb` | Tüm Teröristlerin donmasını çözer |
| `!fz <sn>` | HUD geri sayımı ile gecikmeli dondurma başlatır |
| `!fz0` | Aktif dondurma geri sayımını iptal eder |
| `!daire <genişlik>` | T'leri bakılan noktada daire şeklinde dizer |
| `!diz <mesafe>` | T'leri bakılan noktada yan yana dizer |
| `!gelt` | Tüm Teröristleri yanına çeker |
| `!git <isim>` | Belirtilen oyuncunun yanına ışınlar |

### Diğer Komutlar
| Komut | Açıklama |
| :--- | :--- |
| `!iseli <sn>` | Kapı açma geri sayımını başlatır |
| `!iq` | Haritadaki tüm kapıları anında açar |
| `!dsustum` | Deagle ödüllü Sustum oyununu başlatır |
| `!tsustum` | CT Takımına geçiş ödüllü Sustum oyununu başlatır |
| `!olusustum` | Canlanma ödüllü Sustum oyununu başlatır |
| `!marker <boyut>` | Komutçu marker boyutunu ayarlar (1-250) |
| `!hpa` | Herkesin canını 100 yapar |
| `!hpt` | T takımının canını 100 yapar |
| `!hpct` | CT takımının canını 100 yapar |

---

## ⚙️ Yapılandırma (Config)

Config dosyası `configs/plugins/JailBreak/JailBreak.json` dizininde bulunur. Şunları özelleştirebilirsiniz:
- **Mesajlar & Renkler:** Tüm chat ve HUD mesajları renk taglarını destekler.
- **Silah Listeleri:** FF Menüsü için Birincil ve İkincil silah seçeneklerini düzenleyebilirsiniz.
- **Süreler:** Komutçu görev süresi sınırlarını ve varsayılan geri sayım sürelerini ayarlayabilirsiniz.
- **Yetkiler:** Her komut için gereken yetki flaglarını tanımlayabilirsiniz.

---

## 📦 Kurulum

1. CS2 sunucunuza **CounterStrikeSharp** kurun.
2. Warden Plugin'in son sürümünü indirin.
3. İçeriği `game/csgo/addons/counterstrikesharp/plugins/` dizinine çıkartın.
4. (Opsiyonel) `JailBreak.json` dosyasını kendinize göre düzenleyin.
5. Sunucuyu yeniden başlatın veya `!reloadconfig` komutu ile ayarları yükleyin.

---

## 🎨 HUD Tasarım Standartları

Tüm HUD (Center-HTML) mesajları için tek tip ve profesyonel bir stil kullanıyoruz:
- 🟡 **Altın (Gold):** Başlıklar ve Sistem Durumu.
- 🔴 **Kırmızı (Red):** Zamanlayıcılar ve Geri Sayım değerleri.
- 🟢 **Yeşil (Green):** Hedef kelimeler ve Aktif silahlar.
- 🥈 **Gümüş (Silver):** Kontrol talimatları (Örn: Tab/Shift).

---

## 📜 Krediler
CS2 JailBreak topluluğu için ❤️ ile geliştirildi.
**CounterStrikeSharp** tarafından desteklenmektedir.
