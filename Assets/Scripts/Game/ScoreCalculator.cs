public static class ScoreCalculator {
    public static int ComputeStars(int elapsedSec, Difficulty diff, int hintsUsed) {
        float budget = diff.TimeBonusSec();
        float ratio = elapsedSec / budget;
        int baseStars = ratio <= 0.5f ? 3 : ratio <= 0.8f ? 2 : 1;
        return System.Math.Max(1, baseStars - hintsUsed);
    }

    public static int ComputeXp(int stars, Difficulty diff, int words) {
        int mult = diff switch {
            Difficulty.EASY => 10, Difficulty.MEDIUM => 18,
            Difficulty.HARD => 28, Difficulty.EXPERT => 42, _ => 18
        };
        return stars * mult + words * 2;
    }
}
