using NUnit.Framework;

public class ScoreCalculatorTests {
    [Test]
    public void FastFinishGetsThreeStars() {
        Assert.AreEqual(3, ScoreCalculator.ComputeStars(20, Difficulty.MEDIUM, 0));
    }

    [Test]
    public void HintsReduceStars() {
        Assert.AreEqual(2, ScoreCalculator.ComputeStars(20, Difficulty.MEDIUM, 1));
    }

    [Test]
    public void NeverBelowOneStar() {
        Assert.AreEqual(1, ScoreCalculator.ComputeStars(500, Difficulty.EASY, 5));
    }
}
