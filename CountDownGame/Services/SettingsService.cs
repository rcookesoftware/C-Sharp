using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using CountDownGame.Models;
using Microsoft.Maui;

namespace CountDownGame.Services;

public class SettingsService
{
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "settings.json");

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<GameSettings> LoadAsync()
    {
        if (!File.Exists(FilePath))
        {
            var def = new GameSettings();
            await SaveAsync(def);
            ApplyTheme(def.Theme);
            return def;
        }

        using var fs = File.OpenRead(FilePath);
        var s = await JsonSerializer.DeserializeAsync<GameSettings>(fs, Options) ?? new GameSettings();
        ApplyTheme(s.Theme);
        return s;
    }

    // Small file, so a sync path is handy (used by Game VM right before a round).
    public GameSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            var def = new GameSettings();
            Save(def);
            ApplyTheme(def.Theme);
            return def;
        }
        var json = File.ReadAllText(FilePath);
        var s = JsonSerializer.Deserialize<GameSettings>(json, Options) ?? new GameSettings();
        ApplyTheme(s.Theme);
        return s;
    }

    public async Task SaveAsync(GameSettings s)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(s, Options));
        ApplyTheme(s.Theme);
    }

    public void Save(GameSettings s)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(s, Options));
        ApplyTheme(s.Theme);
    }

    public static void ApplyTheme(string? theme)
    {
        var app = Application.Current;
        if (app is null) return;

        app.UserAppTheme = theme switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified // System
        };
    }
}

