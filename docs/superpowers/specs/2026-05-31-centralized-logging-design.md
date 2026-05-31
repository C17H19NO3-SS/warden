# Merkezi Log ve Hata Ayıklama (Debug) Sistemi Tasarımı

## 1. Genel Bakış
Eklenti içerisindeki `ILogger` ve dağınık loglama yapıları kaldırılarak, tüm log işlemleri `Server.PrintToConsole` üzerinden çalışacak merkezi bir `LogHelper` sınıfına yönlendirilecektir. Bu yapı, geliştiricilerin hataları ve akışı daha kolay takip edebilmesi için detaylı hata ayıklama (debug) özelliklerini barındıracaktır.

## 2. Yapılacak Değişiklikler

### 2.1. Konfigürasyon (Config/PluginConfig.cs)
`PluginConfig` sınıfına yeni bir özellik eklenecektir:
- `DebugMode` (bool): `true` olduğunda, normal hataların yanı sıra, bilgi (Info) ve detay (Trace/Debug) mesajları da konsola yazdırılır.

### 2.2. Merkezi Log Sınıfı (Helpers/LogHelper.cs)
Yeni bir statik `LogHelper` sınıfı oluşturulacaktır.
Önerilen metodlar:
- `LogError(string message, Exception ex = null)`: Kritik hataları kırmızı/belirgin bir formatta, exception detaylarıyla (Stack Trace) birlikte konsola yazar. `DebugMode` kapalı olsa bile çalışır.
- `LogInfo(string message)`: Sadece genel bilgi mesajları için kullanılır (Örn: "Plugin yüklendi"). `DebugMode` kapalı olsa bile çalışır.
- `LogDebug(string message)`: Sadece `DebugMode` `true` olduğunda çalışır. Geliştiriciye özel, metod giriş-çıkışları veya detaylı akış bilgisi sağlar.

**Log Formatı:**
`[JailBreak] [INFO/ERROR/DEBUG] [Zaman Damgası] Mesaj`
*(Örnek: `[JailBreak] [ERROR] [14:35:20] DispatcherService: Komut çalıştırılamadı. Hata: NullReferenceException...`)*

### 2.3. Mevcut Kodların Güncellenmesi
Aşağıdaki dosyalardaki `_logger.LogError`, `Logger.LogError` ve mevcut `Server.PrintToConsole` kullanımları yeni `LogHelper` yapısına geçirilecektir:
- `JailBreakPlugin.cs`
- `Services/DispatcherService.cs`
- `Services/RebelService.cs`
- `Services/WardenService.cs`
- (Gerekli görülen diğer yerler)

## 3. Avantajlar
- **Tek Merkez:** İleride loglama davranışını (örneğin dosyaya yazma) değiştirmek istersek, sadece `LogHelper`ı güncellemek yeterli olacaktır.
- **Detaylı İzleme:** `DebugMode` sayesinde sadece istendiğinde çalışan, sunucuyu yormayan ancak gerektiğinde çok detaylı bilgi veren bir yapı kurulmuş olur.
