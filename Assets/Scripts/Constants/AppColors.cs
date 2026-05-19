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

    public static readonly Color BackgroundLight  = Hex("#FFF8E1");
    public static readonly Color BackgroundMedium = Hex("#E3F2FD");
    public static readonly Color CardBackground   = Color.white;

    public static readonly Color[] HighlightColors = {
        Hex("#FFEB3B"), Hex("#90CAF9"), Hex("#F48FB1"), Hex("#A5D6A7"),
        Hex("#CE93D8"), Hex("#FFCC80"), Hex("#80DEEA"), Hex("#FFAB91"),
        Hex("#B39DDB"), Hex("#FFF59D")
    };

    static Color Hex(string hex) {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
