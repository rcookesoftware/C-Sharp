using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace CountDownGame.Services;

public class DictionaryService
{
    // URL from the brief (large word list)
    private const string DictionaryUrl =
        "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/cdwords.txt";

    // Where we’ll cache it
    private static readonly string LocalPath =
        Path.Combine(FileSystem.AppDataDirectory, "cdwords.txt");

    /// <summary>
    /// Loads the dictionary from local cache if present; otherwise downloads and caches it.
    /// Returns uppercase words for fast lookups.
    /// </summary>
    public async Task<HashSet<string>> LoadOrDownloadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(LocalPath) || new FileInfo(LocalPath).Length == 0)
            await DownloadAsync(ct).ConfigureAwait(false);

        return await ReadFromLocalAsync(ct).ConfigureAwait(false);
    }

    private static async Task DownloadAsync(CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient();
            using var response = await http.GetAsync(DictionaryUrl, HttpCompletionOption.ResponseHeadersRead, ct)
                                           .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var netStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            Directory.CreateDirectory(Path.GetDirectoryName(LocalPath)!);
            await using var file = File.Create(LocalPath);
            await netStream.CopyToAsync(file, ct).ConfigureAwait(false);
        }
        catch
        {
            // Fallback: use bundled copy in Resources/Raw (Build Action = MauiAsset)
            try
            {
                await using var pkg = await FileSystem.OpenAppPackageFileAsync("cdwords.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(LocalPath)!);
                await using var file = File.Create(LocalPath);
                await pkg.CopyToAsync(file, ct);
            }
            catch
            {
                // If both download and fallback fail, rethrow so caller can show a message
                throw;
            }
        }
    }


    private static async Task<HashSet<string>> ReadFromLocalAsync(CancellationToken ct)
    {
        var set = new HashSet<string>();
        using var fs = File.OpenRead(LocalPath);
        using var sr = new StreamReader(fs);
        string? line;
        while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            ct.ThrowIfCancellationRequested();
            line = line.Trim();
            if (line.Length == 0) continue;

            // store uppercase for O(1) case-insensitive lookups
            set.Add(line.ToUpperInvariant());
        }
        return set;
    }
}

