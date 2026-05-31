# Jailbreak Yetkilendirme ve Komut Sistemi

Sunucunuzdaki `admin_groups.json` yapılandırmasına göre oluşturulmuş güncel yetkilendirme modeli aşağıda listelenmiştir. Sistemdeki komutlar (`config`, `password`, `cheats`, `root`, `generic`, `reservation` haricindeki) kullanılabilir admin yetki bayraklarına (flag'lere) dengeli ve rollerin hiyerarşisine uygun şekilde dağıtılmıştır.

1. **Oyun İçi Aktif Komutçu ve Komutçu Adminler:** O an oyunda komutçu (`!w`) olan kişi ve Komutçu Adminliği (`@jailbreak/ka`) bulunan yetkililer, admin yetkisine bakılmaksızın **tüm Jailbreak ve Moderasyon** komutlarını kullanabilir.
2. **Adminler (Komutçu veya Komutçu Admin Olmayanlar):** Bu iki rolde olmayan yetkililer, sahip oldukları yetki gruplarına (flag'lere) göre sadece aşağıda belirtilen komutları kullanabilir.

---

## 1. Oyun İçi Aktif Komutçu ve Komutçu Adminler
Oyunda `!w` yazıp **Komutçu** olan oyuncu ve **Komutçu Admini** (`@jailbreak/ka`) yetkisine sahip yetkililer, sistemdeki tüm moderasyon ve jailbreak komutlarına sınırsız erişime sahiptir.

**Kullanabileceği Komutlar:**
* Aşağıdaki tüm komutlar (Tam liste)

---

## 2. Sunucu Çapında Etkili Güçlü Komutlar (Ban Yetkisi)
Tüm sunucuyu aynı anda etkileyen ve oyunun dengesini değiştirebilecek komutlardır.

**Kullanılabilen Komutlar:**
- `!hpa` [`@css/ban`] - (Tüm oyuncuların canını 100 yapar)
- `!gelall` [`@css/ban`] - (Tüm oyuncuları yanına çeker)
- `!haksal` [`@css/ban`] - (CT ve T takımlarını yer değiştirir)

---

## 3. Oyun Modu ve Harita İçi Sistemler (Changemap Yetkisi)
FF (Dost ateşi) sistemi ve hücre kapıları gibi oyunun genel modunu ve harita unsurlarını yöneten komutlar.

**Kullanılabilen Komutlar:**
- `!ff0` [`@css/changemap`] - (FF'i kapatır ve T'lerin silahlarını alır)
- `!ffondur` [`@css/changemap`] - (Süreli FF açar ve bitiminde herkesi dondurur)
- `!ffmenu` / `!ffkapat` [`@css/changemap`] - (Sıradan FF menüsünü açar veya kapatır)
- `!iseli` / `!iq` [`@css/changemap`] - (Hücre kapılarını süreli veya anında açar)

---

## 4. Takımsal Çekme ve Diziliş Yönetimi (Kick Yetkisi)
Oyuncuların konumlarını değiştirmek ve onları hizalamak için kullanılan komutlar.

**Kullanılabilen Komutlar:**
- `!gelt` [`@css/kick`] - (Tüm T takımını yanına çeker)
- `!gelct` [`@css/kick`] - (Tüm CT takımını yanına çeker)
- `!git` [`@css/kick`] - (Belirtilen oyuncunun yanına ışınlanır)
- `!daire` [`@css/kick`] - (T takımını etrafında daire şeklinde dizer)
- `!diz` [`@css/kick`] - (T takımını önünde yan yana dizer)

---

## 5. Can, Dondurma ve İnfaz (Slay Yetkisi)
Oyunculara bireysel veya takımsal olarak fiziksel müdahalede bulunan komutlar.

**Kullanılabilen Komutlar:**
- `!af` [`@css/slay`] - (Ölü bir oyuncuyu canlandırır ve canını 100 yapar)
- `!hpt` [`@css/slay`] - (Sadece T takımının canını 100 yapar)
- `!hpct` [`@css/slay`] - (Sadece CT takımının canını 100 yapar)
- `!td` [`@css/slay`] - (T takımını dondurur)
- `!tdb` [`@css/slay`] - (T takımının donmasını çözer)
- `!fz` / `!fz0` [`@css/slay`] - (Gecikmeli dondurma başlatır / sıfırlar)
- `!sonsec` [`@css/slay`] - (Sona kalan T dışındakileri öldürür ve LR menüsü açar)

---

## 6. Mini Oyunlar ve Sohbet Araçları (Chat Yetkisi)
Sohbet üzerinden oynanan sessizlik oyunları ve işaretleyici boyutunu ayarlama gibi işlevler.

**Kullanılabilen Komutlar:**
- `!dsustum` / `!tsustum` / `!olusustum` [`@css/chat`] - (Sustum mini oyunlarını başlatır)
- `!marker` [`@css/chat`] - (İşaretleyici boyutunu ayarlar)

---

## 7. Oylama ve Bilgi (Vote ve Herkes)
Komutçu oylamaları, isyancı takibi ve sona kalan oyuncu işlemleri.

**Kullanılabilen Komutlar:**
- `!komoyla` [`@css/vote` veya Herkes] - (Komutçu oylamasını başlatır)
- `!komdk` [`@css/vote` veya Herkes] - (Mevcut komutçuyu atmak için oylama başlatır)
- `!komaday` [Herkes] - (Komutçu oylamasına katılır)
- `!isyancilar` [Herkes] - (O el isyan eden oyuncuların listesini gösterir)
- `!sonakalan` [Herkes] - (Sona kalan oyuncunun LR menüsünü açmasını sağlar)

---

## 8. Komutçu Admin Yönetimi (Cvar Yetkisi)
Jailbreak sistemindeki komutçu yöneticilerini atamak/silmek içindir. `admin_groups.json` dosyasındaki roller incelendiğinde `@css/cvar` yetkisi sadece `#Yönetim` ve daha üst gruplarda bulunduğu için hiyerarşik olarak en uygun olanıdır.

**Kullanılabilen Komutlar:**
- `!ka` [`@jailbreak/ka` veya `@css/cvar`] - (Seçilen oyuncuya Komutçu Admini verir)
- `!kasil` [`@jailbreak/ka` veya `@css/cvar`] - (Seçilen oyuncudan Komutçu Admini alır)