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
        cached = JsonConvert.DeserializeObject<Dictionary<string, SavedGameState>>(File.ReadAllText(FilePath))
                 ?? new Dictionary<string, SavedGameState>();
        return cached;
    }

    void Persist() {
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(cached, Formatting.Indented));
    }

    static string Key(string topicId, int levelId) => $"{topicId}:{levelId}";

    public SavedGameState Get(string topicId, int levelId) {
        Map().TryGetValue(Key(topicId, levelId), out var s);
        return s;
    }

    public bool Has(string topicId, int levelId) => Map().ContainsKey(Key(topicId, levelId));

    public void Put(SavedGameState state) {
        Map()[Key(state.topicId, state.levelId)] = state;
        Persist();
    }

    public void Remove(string topicId, int levelId) {
        if (Map().Remove(Key(topicId, levelId))) Persist();
    }

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
