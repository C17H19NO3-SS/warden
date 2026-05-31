using CounterStrikeSharp.API.Core;
using MySqlConnector;
using Dapper;
using Tomlyn;
using Tomlyn.Model;

namespace StoreApi;

/// <summary>
/// Store eklentisinin veritabanına doğrudan erişim sağlayan, API bağımsız köprü sınıfı.
/// </summary>
public static class StoreBridge
{
    private static string? _configPath;
    private static string? _connectionString;
    private static string _playersTable = "store_players";
    private static bool _configLoaded;

    /// <summary>
    /// Config dosyasının yolunu manuel olarak belirler.
    /// </summary>
    public static void SetConfigPath(string path)
    {
        _configPath = path;
    }

    private static string GetConfigPath()
    {
        if (!string.IsNullOrEmpty(_configPath)) return _configPath;

        // Varsayılan arama yolları
        string[] paths = {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../configs/plugins/cs2-store/config.toml"),
            "/home/container/game/csgo/addons/counterstrikesharp/configs/plugins/cs2-store/config.toml"
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }

        return string.Empty;
    }

    /// <summary>
    /// Store eklentisinin config.toml dosyasından veritabanı bilgilerini ve tablo adlarını yükler.
    /// </summary>
    private static void EnsureConfigLoaded()
    {
        if (_configLoaded) return;

        try
        {
            string configPath = GetConfigPath();

            if (File.Exists(configPath))
            {
                string configText = File.ReadAllText(configPath);
                var model = Toml.ToModel(configText);

                if (model.TryGetValue("DatabaseConnection", out object? dbObj) && dbObj is TomlTable dbTable)
                {
                    string host = dbTable.ContainsKey("Host") ? dbTable["Host"].ToString()! : "localhost";
                    string port = dbTable.ContainsKey("Port") ? dbTable["Port"].ToString()! : "3306";
                    string user = dbTable.ContainsKey("User") ? dbTable["User"].ToString()! : "root";
                    string pass = dbTable.ContainsKey("Pass") ? dbTable["Pass"].ToString()! : "";
                    string name = dbTable.ContainsKey("Name") ? dbTable["Name"].ToString()! : "store";

                    // Tablo adını config'den dinamik al
                    _playersTable = dbTable.ContainsKey("StorePlayersName") ? dbTable["StorePlayersName"].ToString()! : "store_players";

                    _connectionString = $"Server={host};Port={port};Database={name};Uid={user};Pwd={pass};";
                }
            }
        }
        catch
        {
            // Hata durumunda varsayılan değerler kalır
        }
        finally
        {
            _configLoaded = true;
        }
    }

    /// <summary>
    /// Oyuncunun kredisini doğrudan veritabanından çeker.
    /// </summary>
    public static int GetCredits(CCSPlayerController player)
    {
        EnsureConfigLoaded();
        if (string.IsNullOrEmpty(_connectionString)) return -1;

        using var conn = new MySqlConnection(_connectionString);
        try
        {
            return conn.ExecuteScalar<int>(
                $"SELECT Credits FROM {_playersTable} WHERE SteamID = @SteamID",
                new { SteamID = player.SteamID }
            );
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Oyuncuya kredi ekler (Doğrudan DB güncellemesi).
    /// </summary>
    public static void GiveCredits(CCSPlayerController player, int amount)
    {
        EnsureConfigLoaded();
        if (string.IsNullOrEmpty(_connectionString)) return;

        using var conn = new MySqlConnection(_connectionString);
        try
        {
            conn.Execute(
                $"UPDATE {_playersTable} SET Credits = GREATEST(Credits + @Amount, 0) WHERE SteamID = @SteamID",
                new { Amount = amount, SteamID = player.SteamID }
            );
        }
        catch
        {
        }
    }

    /// <summary>
    /// Oyuncunun kredisini belirli bir miktara sabitler.
    /// </summary>
    public static void SetCredits(CCSPlayerController player, int amount)
    {
        EnsureConfigLoaded();
        if (string.IsNullOrEmpty(_connectionString)) return;

        using var conn = new MySqlConnection(_connectionString);
        try
        {
            conn.Execute(
                $"UPDATE {_playersTable} SET Credits = @Amount WHERE SteamID = @SteamID",
                new { Amount = amount, SteamID = player.SteamID }
            );
        }
        catch
        {
        }
    }

    /// <summary>
    /// Oyuncunun VIP durumunu doğrudan veritabanından kontrol eder.
    /// </summary>
    public static bool IsVip(CCSPlayerController player)
    {
        EnsureConfigLoaded();
        if (string.IsNullOrEmpty(_connectionString)) return false;

        using var conn = new MySqlConnection(_connectionString);
        try
        {
            return conn.ExecuteScalar<bool>(
                $"SELECT Vip FROM {_playersTable} WHERE SteamID = @SteamID",
                new { SteamID = player.SteamID }
            );
        }
        catch
        {
            return false;
        }
    }
}
