using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using CountDownGame.Models;

namespace CountDownGame.Services;

public class GameStorageService
{
    private static readonly string FilePath =
        Path.Combine(FileSystem.AppDataDirectory, "games.json");

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<List<GameResult>> LoadAllAsync(CancellationToken ct = default)
    {
        if (!File.Exists(FilePath)) return new();
        await using var fs = File.OpenRead(FilePath);
        var list = await JsonSerializer.DeserializeAsync<List<GameResult>>(fs, Options, ct)
                   ?? new List<GameResult>();
        return list;
    }

    public async Task AddAsync(GameResult result, CancellationToken ct = default)
    {
        var list = await LoadAllAsync(ct);
        list.Insert(0, result); // newest first
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        await using var fs = File.Create(FilePath);
        await JsonSerializer.SerializeAsync(fs, list, Options, ct);
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        return Task.CompletedTask;
    }
}

