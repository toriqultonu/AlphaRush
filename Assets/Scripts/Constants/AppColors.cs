using UnityEngine;

public static class AppColors {
    public static readonly Color FunPurple = Hex("#6200EE");
    public static readonly Color FunBlue   = Hex("#2196F3");
    public static readonly Color FunGreen  = Hex("#4CAF50");
    public static readonly Color FunOrange = Hex("#FF9800");
    public static readonly Color FunPink   = Hex("#E91E63");
    public static readonly Color FunYellow = Hex("#FFEB3B");
    public static readonly Color FunTeal   = Hex("#009688");
    public static readonly Color FunRed    = Hex("#F44336");

    // Candy theme per docs/new_demo.jpeg — peach sparkle background, pink
    // glossy chrome, biscuit tiles, bright candy-pill accents.
    public static readonly Color BackgroundLight  = Hex("#FDE8C3");
    public static readonly Color BackgroundMedium = Hex("#F6D19B");
    public static readonly Color CardBackground   = Color.white;

    public static readonly Color CandyPink     = Hex("#F782B4");
    public static readonly Color CandyPinkDeep = Hex("#E2447E");
    public static readonly Color BoardMaroon   = Hex("#5C2438");
    public static readonly Color BiscuitTile   = Hex("#F7E8C9");
    public static readonly Color LetterBrown   = Hex("#5C3A21");
    public static readonly Color CandyTeal     = Hex("#35C3C1");
    public static readonly Color CandyPurple   = Hex("#C05BD4");
    public static readonly Color CandyBlue     = Hex("#4FA8F5");
    public static readonly Color CandyOrange   = Hex("#F5A623");
    public static readonly Color CandyGreen    = Hex("#7DC942");
    public static readonly Color StarGold      = Hex("#FFC93C");

    // Word-list pill colors (cycled per chip, demo style).
    public static readonly Color[] ChipColors = {
        Hex("#35C3C1"), Hex("#F2607A"), Hex("#7DC942"), Hex("#C05BD4"),
        Hex("#4FA8F5"), Hex("#F5A623"), Hex("#F782B4")
    };

    // Found-word tile tints — bright candy shades.
    public static readonly Color[] HighlightColors = {
        Hex("#FF9EC4"), Hex("#7FE3E1"), Hex("#B4E88C"), Hex("#DDA4EC"),
        Hex("#9CCBFA"), Hex("#FFD08A"), Hex("#FFB3C8"), Hex("#A8E6CF"),
        Hex("#C9B6F5"), Hex("#FFE49C")
    };

    static Color Hex(string hex) {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
