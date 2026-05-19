using System.Collections.Generic;
using NUnit.Framework;

public class GridGeneratorTests {
    [Test]
    public void SameSeedProducesSameGrid() {
        var words = new List<string> { "CAT", "DOG", "BIRD" };
        var a = GridGenerator.Generate(10, words, Difficulty.MEDIUM, 12345);
        var b = GridGenerator.Generate(10, words, Difficulty.MEDIUM, 12345);
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                Assert.AreEqual(a.Grid[r, c], b.Grid[r, c]);
    }

    [Test]
    public void AllPlacedWordsRetrievable() {
        var words = new List<string> { "APPLE", "MANGO" };
        var res = GridGenerator.Generate(10, words, Difficulty.MEDIUM, 99);
        foreach (var pw in res.Placed) {
            var (dr, dc) = GridGenerator.DirToDelta(pw.direction);
            for (int i = 0; i < pw.word.Length; i++) {
                Assert.AreEqual(pw.word[i], res.Grid[pw.startRow + dr * i, pw.startCol + dc * i]);
            }
        }
    }
}
