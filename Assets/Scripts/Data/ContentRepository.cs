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

    public Task<List<Level>> GetLevelsAsync(string topicId) {
        if (!levelCache.TryGetValue(topicId, out var levels)) {
            levels = source.GenerateLevels(topicId);
            levelCache[topicId] = levels;
        }
        return Task.FromResult(levels);
    }

    public async Task<Level> GetLevelAsync(string topicId, int levelId) {
        var levels = await GetLevelsAsync(topicId);
        return levels.Find(l => l.id == levelId);
    }

    // Deterministically picks `level.targetWordCount` words from the topic pool using level.seed.
    public async Task<List<string>> GetWordsForLevelAsync(Level level) {
        if (level == null) return new List<string>();
        var topic = await GetTopicAsync(level.topicId);
        if (topic == null) return new List<string>();

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
