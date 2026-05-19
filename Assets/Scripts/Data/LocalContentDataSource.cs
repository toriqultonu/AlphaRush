using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LocalContentDataSource : IContentDataSource {
    List<Topic> cached;

    public async System.Threading.Tasks.Task<List<Topic>> LoadTopicsAsync() {
        if (cached != null) return cached;
        string path = Path.Combine(Application.streamingAssetsPath, "data/topics.json");
        string json;
        if (path.Contains("://")) {
            using var req = UnityWebRequest.Get(path);
            var op = req.SendWebRequest();
            while (!op.isDone) await System.Threading.Tasks.Task.Yield();
            json = req.downloadHandler.text;
        } else {
            json = File.ReadAllText(path);
        }
        cached = JsonConvert.DeserializeObject<List<Topic>>(json);
        return cached;
    }

    public List<Level> GenerateLevels(string topicId) {
        var levels = new List<Level>(AppConfig.LevelsPerTopic);
        for (int i = 1; i <= AppConfig.LevelsPerTopic; i++) {
            Difficulty diff = i <= 8 ? Difficulty.EASY
                            : i <= 18 ? Difficulty.MEDIUM
                            : i <= 26 ? Difficulty.HARD
                            : Difficulty.EXPERT;
            int target = TargetWordCount(diff, i);
            long seed = ((long)topicId.GetHashCode() * 31) + i;
            levels.Add(new Level { id = i, topicId = topicId, difficulty = diff, targetWordCount = target, seed = seed });
        }
        return levels;
    }

    static int TargetWordCount(Difficulty d, int levelInTopic) => d switch {
        Difficulty.EASY   => 3 + ((levelInTopic - 1) / 2),         // 3 → 5
        Difficulty.MEDIUM => 5 + ((levelInTopic - 9) / 3),         // 5 → 8
        Difficulty.HARD   => 8 + ((levelInTopic - 19) / 3),        // 8 → 10
        Difficulty.EXPERT => 10 + ((levelInTopic - 27) / 2),       // 10 → 12
        _ => 5
    };
}
