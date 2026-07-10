using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VisualMetronome;

public class BpmCacheService
{
    private readonly string _cacheFilePath;
    private Dictionary<string, BpmCacheEntry> _cache = new();

    public BpmCacheService()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VisualMetronome");
        
        Directory.CreateDirectory(appDataFolder);
        _cacheFilePath = Path.Combine(appDataFolder, "bpm_cache.json");
        LoadCache();
    }

    public int? GetCachedBpm(string spotifyTrackId)
    {
        if (_cache.TryGetValue(spotifyTrackId, out var entry))
        {
            Console.WriteLine($"✓ Cache hit for track {spotifyTrackId}: {entry.Bpm} BPM");
            return entry.Bpm;
        }
        return null;
    }

    public void CacheBpm(string spotifyTrackId, string songName, string artist, int bpm)
    {
        _cache[spotifyTrackId] = new BpmCacheEntry
        {
            SpotifyTrackId = spotifyTrackId,
            SongName = songName,
            Artist = artist,
            Bpm = bpm,
            CachedAt = DateTime.UtcNow
        };
        SaveCache();
        Console.WriteLine($"✓ Cached BPM {bpm} for '{songName}' (ID: {spotifyTrackId})");
    }

    private void LoadCache()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                var json = File.ReadAllText(_cacheFilePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, BpmCacheEntry>>(json) ?? new();
                Console.WriteLine($"Loaded BPM cache: {_cache.Count} entries from {_cacheFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading BPM cache: {ex.Message}");
            _cache = new();
        }
    }

    private void SaveCache()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_cache, options);
            File.WriteAllText(_cacheFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving BPM cache: {ex.Message}");
        }
    }
}

public class BpmCacheEntry
{
    public string SpotifyTrackId { get; set; } = string.Empty;
    public string SongName { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Bpm { get; set; }
    public DateTime CachedAt { get; set; }
}
