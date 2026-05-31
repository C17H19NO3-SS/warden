# Jailbreak Yetkilendirme ve Komut Sistemi

Sunucunuzdaki `admin_groups.json` yapılandırmasına göre optimize edilmiş, modern hiyerarşik yetkilendirme modeli. Tüm komutlar, rollerin sorumluluklarına ve oyun dengesine göre dağıtılmıştır.

1. **Oyun İçi Aktif Komutçu ve Komutçu Adminler:** O an oyunda komutçu (`!w`) olan kişi ve Komutçu Adminliği (`@jailbreak/ka`) bulunan yetkililer, admin yetkisine bakılmaksızın **tüm** oyun içi komutları kullanabilir.
2. **Adminler:** Aktif rolde olmayan yetkililer, sahip oldukları flag'lere göre aşağıda belirtilen sınırlı komutlara erişebilir.

---

## 1. Merkezi Kontrol ve Ana Menü
Tüm yetkililerin hızlı erişim noktasıdır.

- `!k` / `!kommenu` - **Warden Ana Menüsünü** açar. (İçerisinde kapı açma, Box, Saklambaç, FF ve ayarlar bulunur).

---

## 2. Sunucu Çapında Etkili Komutlar (Ban Yetkisi)
Oyunun genel akışını ve dengesini değiştirebilecek kritik komutlardır.

**Kullanılabilen Komutlar:**
- `!rev <hedef>` [`@css/ban`] - (Belirtilen oyuncuyu veya takımı canlandırır)
- `!hpa` [`@css/ban`] - (Tüm oyuncuların canını 100 yapar)
- `!gelall` [`@css/ban`] - (Tüm oyuncuları yanına çeker)
- `!haksal` [`@css/ban`] - (CT ve T takımlarını yer değiştirir)

---

## 3. Oyun Modları ve Harita Yönetimi (Changemap Yetkisi)
Saklambaç, Box ve FF gibi harita çapındaki aktiviteleri yönetir.

**Kullanılabilen Komutlar:**
- `!saklambac <sn>` [`@css/changemap`] - (Saklambaç modunu başlatır)
- `!box <sn>` [`@css/changemap`] - (Boks modunu başlatır)
- `!ffmenu` [`@css/changemap`] - (Özelleştirilebilir FF menüsünü açar)
- `!ffkapat` [`@css/changemap`] - (FF'i anında durdurur)
- `!iseli` / `!iq` [`@css/changemap`] - (Hücre kapılarını açar)

---

## 4. Takımsal Yönetim ve Diziliş (Kick Yetkisi)
Oyuncuları hizalamak ve konumlarını değiştirmek için kullanılır.

**Kullanılabilen Komutlar:**
- `!gelt` / `!gelct` [`@css/kick`] - (Belirli bir takımı yanına çeker)
- `!git <isim>` [`@css/kick`] - (Oyuncunun yanına ışınlanır)
- `!daire` / `!diz` [`@css/kick`] - (T'leri formasyona dizer)

---

## 5. Can, Dondurma ve İnfaz (Slay Yetkisi)
Oyunculara bireysel veya takımsal fiziksel müdahale komutları.

**Kullanılabilen Komutlar:**
- `!af` [`@css/slay`] - (Herkesi canlandırır ve canlarını 100 yapar)
- `!td` / `!tdb` [`@css/slay`] - (T takımını dondurur / çözer)
- `!fz` / `!fz0` [`@css/slay`] - (HUD sayaçlı dondurma başlatır / iptal eder)
- `!sonsec` [`@css/slay`] - (Sona kalan T hariç öldürür ve LR açar)

---

## 6. Duyuru ve Broadcast Araçları
Ekranda görsel bilgilendirme yapmak için kullanılır.

**Kullanılabilen Komutlar:**
- `!msay <mesaj>` [`@css/chat`] - (Ekranın ortasında büyük duyuru açar)
- `!csay <mesaj>` [`@css/chat`] - (Merkezi HTML duyuru yapar)
- `!hsay <mesaj>` [`@css/chat`] - (HUD/Hint mesajı gönderir)
- `!marker` [`@css/chat`] - (İşaretleyici boyutunu ayarlar)

---

## 7. Üst Düzey Yönetim (Root ve Cvar)
Sistemsel ayarlar ve özel yetkiler.

- `!fsay <isim> <msg>` [`@css/root`] - (Oyuncuyu taklit ederek chat'e yazar)
- `!ka` / `!kasil` [`@css/cvar`] - (Komutçu Admini yetkisi verir/alır)
- `!reloadconfig` [`@css/root`] - (Eklenti ayarlarını ve dili yeniler)

---

## 8. Herkes Tarafından Kullanılabilen Bilgi Komutları
- `!komoyla` - (Komutçu oylaması başlatır)
- `!komaday` - (Oylamaya katılır)
- `!isyancilar` - (İsyan edenleri listeler)
- `!sonakalan` - (Sona kalan oyuncu için LR menüsü)
