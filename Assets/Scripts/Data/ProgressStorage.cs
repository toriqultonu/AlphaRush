using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ProgressStorage {
    string FilePath => Path.Combine(Application.persistentDataPath, "progress.json");
    PlayerProgress cached;

    public PlayerProgress Load() {
        if (cached != null) return cached;
        if (!File.Exists(FilePath)) {
            cached = new PlayerProgress {
                unlockedTopicIds = new() { "animals", "fruits", "colors", "family" },
                bestResults = new(),
                badges = new()
            };
            return cached;
        }
        cached = JsonConvert.DeserializeObject<PlayerProgress>(File.ReadAllText(FilePath));
        return cached;
    }

    public void Save(PlayerProgress p) {
        cached = p;
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(p, Formatting.Indented));
    }

    public void RecordLevelResult(LevelResult r) {
        var p = Load();
        string key = $"{r.topicId}:{r.levelId}";
        if (!p.bestResults.TryGetValue(key, out var prev) || r.stars > prev.stars
            || (r.stars == prev.stars && r.timeSeconds < prev.timeSeconds)) {
            p.bestResults[key] = r;
        }
        p.totalStars = 0;
        foreach (var v in p.bestResults.Values) p.totalStars += v.stars;
        p.totalXp += r.xpEarned;
        Save(p);
    }

    public void UnlockBadge(string id) {
        var p = Load();
        if (!p.badges.Contains(id)) { p.badges.Add(id); Save(p); }
    }

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
