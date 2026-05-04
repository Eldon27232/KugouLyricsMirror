using System.Text.Json;

namespace KugouLyricsMirror;

internal static class AppConfig
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KugouLyricsMirror");
    private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "config.json");
    private static readonly string LegacyConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    public static Config Current { get; set; } = new();
    public static string CurrentPath => ConfigPath;

    public static void Load()
    {
        try
        {
            var path = File.Exists(ConfigPath)
                ? ConfigPath
                : LegacyConfigPath;

            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var cfg = JsonSerializer.Deserialize<Config>(json);
            if (cfg is not null) Current = cfg;
        }
        catch
        {
            Current = new Config();
        }
    }

    public static bool Save(out string? errorMessage)
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
