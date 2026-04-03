using System.Text.Json;

namespace KugouLyricsMirror;

internal static class AppConfig
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    public static Config Current { get; set; } = new();

    public static void Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return;
            var json = File.ReadAllText(ConfigPath);
            var cfg = JsonSerializer.Deserialize<Config>(json);
            if (cfg is not null) Current = cfg;
        }
        catch
        {
            Current = new Config();
        }
    }

    public static void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
        }
    }
}
