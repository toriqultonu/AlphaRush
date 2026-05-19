using System.Collections.Generic;

[System.Serializable]
public class SavedGameState {
    public string topicId;
    public int levelId;
    public string[] gridRows;       // row strings (CharArray flattened)
    public List<PlacedWord> placedWords;
    public List<string> foundWords;
    public int elapsedSeconds;
    public int hintsUsed;
    public int colorIndex;
}
