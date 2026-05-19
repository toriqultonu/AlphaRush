[System.Serializable]
public enum Difficulty { EASY, MEDIUM, HARD, EXPERT }

public static class DifficultyExt {
    public static int GridSize(this Difficulty d) => d switch {
        Difficulty.EASY => 8, Difficulty.MEDIUM => 10,
        Difficulty.HARD => 12, Difficulty.EXPERT => 14, _ => 10
    };
    public static int MaxWords(this Difficulty d) => d switch {
        Difficulty.EASY => 5, Difficulty.MEDIUM => 8,
        Difficulty.HARD => 10, Difficulty.EXPERT => 12, _ => 8
    };
    public static int TimeBonusSec(this Difficulty d) => d switch {
        Difficulty.EASY => 60, Difficulty.MEDIUM => 90,
        Difficulty.HARD => 150, Difficulty.EXPERT => 240, _ => 90
    };
}
