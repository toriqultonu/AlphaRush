using System;
using System.Collections.Generic;

public static class GridGenerator {
    public class Result {
        public char[,] Grid;
        public List<PlacedWord> Placed;
    }

    static readonly WordDirection[] AllDirs = (WordDirection[])Enum.GetValues(typeof(WordDirection));

    public static Result Generate(int size, List<string> words, Difficulty diff, long seed) {
        var rng = new System.Random((int)(seed ^ (seed >> 32)));
        var dirs = AllowedDirections(diff);
        Result best = null;

        var sorted = new List<string>(words);
        sorted.Sort((a, b) => b.Length.CompareTo(a.Length));

        for (int attempt = 0; attempt < 50; attempt++) {
            var grid = new char[size, size];
            for (int r = 0; r < size; r++) for (int c = 0; c < size; c++) grid[r, c] = ' ';
            var placed = new List<PlacedWord>();

            foreach (var word in sorted) {
                if (!TryPlace(grid, word, dirs, size, rng, placed)) continue;
            }

            if (best == null || placed.Count > best.Placed.Count)
                best = new Result { Grid = (char[,])grid.Clone(), Placed = new List<PlacedWord>(placed) };

            if (placed.Count == sorted.Count) break;
        }

        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                if (best.Grid[r, c] == ' ')
                    best.Grid[r, c] = (char)('A' + rng.Next(26));

        return best;
    }

    static bool TryPlace(char[,] grid, string word, WordDirection[] dirs, int size,
                         System.Random rng, List<PlacedWord> placed) {
        for (int i = 0; i < 200; i++) {
            var dir = dirs[rng.Next(dirs.Length)];
            int r = rng.Next(size), c = rng.Next(size);
            if (TryWrite(grid, word, r, c, dir, size)) {
                placed.Add(new PlacedWord { word = word, startRow = r, startCol = c, direction = dir });
                return true;
            }
        }
        // systematic scan fallback
        foreach (var dir in dirs)
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    if (TryWrite(grid, word, r, c, dir, size)) {
                        placed.Add(new PlacedWord { word = word, startRow = r, startCol = c, direction = dir });
                        return true;
                    }
        return false;
    }

    static bool TryWrite(char[,] grid, string word, int r, int c, WordDirection dir, int size) {
        var (dr, dc) = DirToDelta(dir);
        int endR = r + dr * (word.Length - 1);
        int endC = c + dc * (word.Length - 1);
        if (endR < 0 || endR >= size || endC < 0 || endC >= size) return false;
        for (int i = 0; i < word.Length; i++) {
            int rr = r + dr * i, cc = c + dc * i;
            if (grid[rr, cc] != ' ' && grid[rr, cc] != word[i]) return false;
        }
        for (int i = 0; i < word.Length; i++) {
            int rr = r + dr * i, cc = c + dc * i;
            grid[rr, cc] = word[i];
        }
        return true;
    }

    public static (int dr, int dc) DirToDelta(WordDirection dir) => dir switch {
        WordDirection.HORIZONTAL              => (0, 1),
        WordDirection.HORIZONTAL_REVERSE      => (0, -1),
        WordDirection.VERTICAL                => (1, 0),
        WordDirection.VERTICAL_REVERSE        => (-1, 0),
        WordDirection.DIAGONAL_DOWN           => (1, 1),
        WordDirection.DIAGONAL_DOWN_REVERSE   => (-1, -1),
        WordDirection.DIAGONAL_UP             => (-1, 1),
        WordDirection.DIAGONAL_UP_REVERSE     => (1, -1),
        _ => (0, 1)
    };

    static WordDirection[] AllowedDirections(Difficulty d) => d switch {
        Difficulty.EASY => new[] { WordDirection.HORIZONTAL, WordDirection.VERTICAL },
        Difficulty.MEDIUM => new[] {
            WordDirection.HORIZONTAL, WordDirection.VERTICAL,
            WordDirection.DIAGONAL_DOWN, WordDirection.DIAGONAL_UP
        },
        _ => AllDirs
    };
}
