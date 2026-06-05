using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Saves per (topicId, levelId) as JSON keyed in single saved_games.json map.
public class GameStateStorage {
    string FilePath => Path.Combine(Application.persistentDataPath, "saved_games.json");
    Dictionary<string, SavedGameState> cached;

    Dictionary<string, SavedGameState> Map() {
        if (cached != null) return cached;
        if (!File.Exists(FilePath)) {
            cached = new Dictionary<string, SavedGameState>();
            return cached;
        }
        try {
            cached = JsonConvert.DeserializeObject<Dictionary<string, SavedGameState>>(File.ReadAllText(FilePath))
                     ?? new Dictionary<string, SavedGameState>();
        } catch (System.Exception ex) {
            Debug.LogWarning($"[GameStateStorage] Failed to load {FilePath}: {ex.Message}. Returning empty map.");
            cached = new Dictionary<string, SavedGameState>();
        }
        return cached;
    }

    void Persist() {
        try {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(cached, Formatting.Indented));
        } catch (System.Exception ex) {
            Debug.LogWarning($"[GameStateStorage] Failed to save {FilePath}: {ex.Message}.");
        }
    }

    static string Key(string topicId, int levelId) => $"{topicId}:{levelId}";

    public void Save(SavedGameState state) {
        Map()[Key(state.topicId, state.levelId)] = state;
        Persist();
    }

    public SavedGameState Load(string topicId, int levelId) {
        Map().TryGetValue(Key(topicId, levelId), out var s);
        return s;
    }

    public void Delete(string topicId, int levelId) {
        if (Map().Remove(Key(topicId, levelId))) Persist();
    }

    public bool HasSaved(string topicId, int levelId) => Map().ContainsKey(Key(topicId, levelId));

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
