using System.Text.Json;

namespace TinyClipboardTray;

sealed class StatsStore
{
    public long AllTimeAffectedChars { get; set; }

    public static StatsStore Load()
    {
        try
        {
            var path = GetPath();
            if (!File.Exists(path)) return new StatsStore();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<StatsStore>(json) ?? new StatsStore();
        }
        catch
        {
            return new StatsStore();
        }
    }

    public void Save()
    {
        try
        {
            var path = GetPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch
        {
            // se falhar, só não persiste
        }
    }

    private static string GetPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TinyClipboardTray");

        return Path.Combine(dir, "stats.json");
    }
}
