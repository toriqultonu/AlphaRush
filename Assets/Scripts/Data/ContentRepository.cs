using System.Collections.Generic;
using System.Threading.Tasks;

public class ContentRepository {
    readonly IContentDataSource source;
    readonly Dictionary<string, List<Level>> levelCache = new();

    public ContentRepository(IContentDataSource source) {
        this.source = source;
    }

    public Task<List<Topic>> GetTopicsAsync() => source.LoadTopicsAsync();

    public async Task<Topic> GetTopicAsync(string topicId) {
        var topics = await source.LoadTopicsAsync();
        return topics.Find(t => t.id == topicId);
    }

    public async Task<Level> GetLevelAsync(string topicId, int levelId) {
        if (!levelCache.TryGetValue(topicId, out var levels)) {
            levels = source.GenerateLevels(topicId);
            levelCache[topicId] = levels;
        }
        await Task.CompletedTask;
        return levels.Find(l => l.id == levelId);
    }

    // Deterministically picks `targetWordCount` words from the topic pool using the level seed.
    public async Task<List<string>> GetWordsForLevelAsync(string topicId, int levelId) {
        var topic = await GetTopicAsync(topicId);
        var level = await GetLevelAsync(topicId, levelId);
        if (topic == null || level == null) return new List<string>();

        var rng = new System.Random((int)(level.seed ^ (level.seed >> 32)));
        var pool = new List<string>(topic.wordPool);
        for (int i = pool.Count - 1; i > 0; i--) {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        int take = System.Math.Min(level.targetWordCount, pool.Count);
        return pool.GetRange(0, take);
    }
}
