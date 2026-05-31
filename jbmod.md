# Jailbreak Yetkilendirme ve Komut Sistemi

Sunucunuzdaki `admin_groups.json` yapılandırmasına göre optimize edilmiş, standart admin bayraklarına (flag) dayalı modern yetkilendirme modeli. Bu sistemde `Root`, `Cvar` veya `Cheats` gibi teknik bayraklar yerine, oyun içi etkinin büyüklüğüne göre kademelendirilmiş bir yapı kullanılır.

1. **Oyun İçi Aktif Komutçu ve Komutçu Adminler:** O an oyunda komutçu (`!w`) olan kişi ve Komutçu Adminliği (`@jailbreak/ka`) bulunan yetkililer, admin yetkisine bakılmaksızın **tüm** oyun içi komutları kullanabilir.
2. **Adminler:** Aktif rolde olmayan yetkililer, sahip oldukları bayraklara (flag) göre sadece kendi seviyelerindeki komutları kullanabilir.

---

## 👑 1. @css/ban (Kritik Yetki / Üst Yönetim)
Sunucu dengesini, takımları ve oyun akışını kökten değiştiren en güçlü komutlar.

- *(Bu grup alt gruplara dağıtıldı, sadece en kritik admin işlemleri için ayrıldı)*

---

## 🎮 2. @css/changemap (Oyun Modu Yönetimi)
Aktif harita üzerindeki oyun modlarını ve fiziksel engelleri yönetir.

- `!saklambac <sn>` - (Saklambaç modunu başlatır)
- `!box <sn>` / `!b` - (Boks modunu başlatır)
- `!ffmenu` - (Görsel FF ayar menüsünü açar)
- `!ffdondur <sn>` - (Süreli FF açar ve sonunda dondurur)
- `!ffkapat` / `!ffk` - (Aktif FF'i durdurur)
- `!ff0` - (FF kapatır ve T'lerin silahlarını siler)
- `!iseli <sn>` - (Hücre kapılarını süreli açar)
- `!iq` - (Tüm kapıları anında açar)

---

## 🏃 3. @css/kick (Hareket ve Konum Yönetimi)
Oyuncuların konumlarını düzenlemek ve onları takip etmek için kullanılır.

- `!git <isim>` - (Belirtilen oyuncuya ışınlanır)
- `!gelt` / `!gelct` - (Belirli bir takımı yanına çeker)
- `!gelall` - (Herkesi yanına çeker)
- `!daire <genişlik>` - (T takımını daire formasyonuna dizer)
- `!diz <mesafe>` - (T takımını önünde sıraya dizer)

---

## 👊 4. @css/slay (Fiziksel Müdahale)
Oyuncuların can, donma ve round içindeki durumlarını etkileyen komutlar.

- `!rev <hedef>` - (Oyuncu veya takımı canlandırır)
- `!af` - (Herkesi canlandırır ve canlarını 100 yapar)
- `!hpa` / `!hpt` / `!hpct` - (Canları 100 yapar)
- `!td` / `!tdb` - (T takımını dondurur veya çözer)
- `!fz <sn>` / `!fz0` - (Süreli dondurma başlatır veya iptal eder)
- `!ss` / `!strip` - (T takımının silahlarını siler)
- `!sonsec` - (Sona kalan T dışındakileri öldürür ve LR açar (Sona Kalan Menüsü))
- `!sustum` (dsustum/tsustum/olusustum) - (Sustum oyunlarını başlatır)

---

## 💬 5. @css/chat (İletişim ve Mini Oyunlar)
Duyuru yapmak ve sohbet tabanlı mini oyunlar başlatmak için kullanılır.

- `!msay <mesaj>` - (Büyük kutu içinde duyuru yapar)
- `!csay <mesaj>` - (Merkezi renkli yazı yazar)
- `!hsay <mesaj>` - (HUD/Hint mesajı gönderir)
- `!marker <boyut>` - (Komutçu işaretçisini ayarlar)

---

## 🛡️ 6. @css/generic (Genel Yetki)
- `!k` / `!kommenu` - **Warden Ana Menüsünü** açar.
- `!ka` / `!kasil` - (Komutçu Admini yetkisi verir/alır)
- `!topka` - (En çok komutçu admin olanları listeler)
- `!ba` / `!bk` - (Bunnyhop özelliğini açar/kapatır)
- `!mct` / `!umct` / `!mt` / `!umt` - (Takım mutesini yönetir)
- `!haksal <isim>` - (CT ve T takımlarını yer değiştirir)
- `!otores` / `!otores0` - (Otomatik canlanmayı ayarlar)

---

## 💻 7. @css/root (Sistem Yönetimi)
Sistemsel ayarlar ve en üst düzey yetkiler.

- `!reloadconfig` - (Eklenti ayarlarını ve dili yeniler)
- `!fsay <isim> <msg>` - (Zorunlu say yazdırır)

---

## 👥 8. Herkes Tarafından Kullanılabilenler
- `!w` / `!uw`, `!komkalan`, `!topkomutcu`, `!topka`, `!isyancilar`, `!sonakalan`, `!delay`, `!kaccm`, `!topkaccm`.
