using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class DailyChallengeBlob {
    public DailyChallenge current;
    public int streakDays;
}

public class DailyChallengeStorage {
    string FilePath => Path.Combine(Application.persistentDataPath, "daily.json");
    DailyChallengeBlob cached;

    public DailyChallengeBlob Load() {
        if (cached != null) return cached;
        if (!File.Exists(FilePath)) {
            cached = new DailyChallengeBlob();
            return cached;
        }
        cached = JsonConvert.DeserializeObject<DailyChallengeBlob>(File.ReadAllText(FilePath))
                 ?? new DailyChallengeBlob();
        return cached;
    }

    public void Save(DailyChallengeBlob blob) {
        cached = blob;
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(blob, Formatting.Indented));
    }

    public DailyChallenge GetCurrent() => Load().current;

    public void SetCurrent(DailyChallenge dc) {
        var b = Load();
        b.current = dc;
        Save(b);
    }

    public int GetStreak() => Load().streakDays;

    public void IncrementStreak() {
        var b = Load();
        b.streakDays++;
        Save(b);
    }

    public void ResetStreak() {
        var b = Load();
        b.streakDays = 0;
        Save(b);
    }

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
