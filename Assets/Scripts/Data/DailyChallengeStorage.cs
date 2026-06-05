using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class DailyChallengeBlob {
    public DailyChallenge Current;
    public int StreakDays;
    public string LastCompletedDate;
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
        try {
            cached = JsonConvert.DeserializeObject<DailyChallengeBlob>(File.ReadAllText(FilePath))
                     ?? new DailyChallengeBlob();
        } catch (System.Exception ex) {
            Debug.LogWarning($"[DailyChallengeStorage] Failed to load {FilePath}: {ex.Message}. Returning defaults.");
            cached = new DailyChallengeBlob();
        }
        return cached;
    }

    public void Save(DailyChallengeBlob blob) {
        cached = blob;
        try {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(blob, Formatting.Indented));
        } catch (System.Exception ex) {
            Debug.LogWarning($"[DailyChallengeStorage] Failed to save {FilePath}: {ex.Message}.");
        }
    }

    public DailyChallenge GetCurrent() => Load().Current;

    public void SetCurrent(DailyChallenge challenge) {
        var b = Load();
        b.Current = challenge;
        Save(b);
    }

    public int GetStreak() => Load().StreakDays;

    // Marks the current challenge complete with `stars`, bumps streak if this is a new day
    // (or starts at 1 on first completion / after a gap), and records today's date.
    public void MarkCompleted(int stars) {
        var b = Load();
        if (b.Current != null) {
            b.Current.completed = true;
            b.Current.stars = stars;
        }
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        if (!string.IsNullOrEmpty(b.LastCompletedDate)) {
            if (System.DateTime.TryParse(b.LastCompletedDate, out var prev)) {
                int daysSince = (System.DateTime.Now.Date - prev.Date).Days;
                if (daysSince == 0) {
                    // Already counted today; leave streak as-is.
                } else if (daysSince == 1) {
                    b.StreakDays++;
                } else {
                    b.StreakDays = 1;
                }
            } else {
                b.StreakDays = 1;
            }
        } else {
            b.StreakDays = 1;
        }
        b.LastCompletedDate = today;
        Save(b);
    }

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
