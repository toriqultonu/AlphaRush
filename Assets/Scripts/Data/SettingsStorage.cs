using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class Settings {
    public bool soundEnabled = true;
    public bool hapticsEnabled = true;
    public bool tutorialShown = false;
    public float musicVolume = 0.6f;
    public float sfxVolume = 1.0f;
    public bool reduceMotion = false;
    public bool heartsEnabled = false;
}

public class SettingsStorage {
    string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");
    Settings cached;

    public Settings Load() {
        if (cached != null) return cached;
        if (!File.Exists(FilePath)) {
            cached = new Settings();
            return cached;
        }
        try {
            cached = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
        } catch (System.Exception ex) {
            Debug.LogWarning($"[SettingsStorage] Failed to load {FilePath}: {ex.Message}. Returning defaults.");
            cached = new Settings();
        }
        return cached;
    }

    public void Save(Settings s) {
        cached = s;
        try {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(s, Formatting.Indented));
        } catch (System.Exception ex) {
            Debug.LogWarning($"[SettingsStorage] Failed to save {FilePath}: {ex.Message}.");
        }
    }

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
