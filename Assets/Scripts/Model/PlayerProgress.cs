using System.Collections.Generic;

[System.Serializable]
public class PlayerProgress {
    public int totalStars;
    public int totalXp;
    public int streakDays;
    public long lastPlayedEpochDay;
    public List<string> unlockedTopicIds;
    public Dictionary<string, LevelResult> bestResults;
    public List<string> badges;
}
