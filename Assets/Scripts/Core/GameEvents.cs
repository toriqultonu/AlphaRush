using UnityEngine;

// §17 — central event bus. GameView raises; SoundManager / HapticManager / ParticleSpawner /
// ScreenShake / ComboBannerView subscribe (wire subscriptions in their respective Awake/OnEnable).
public static class GameEvents {
    public static event System.Action<CellSelection> CellTouchDown;
    public static event System.Action<int> CellAddedToSelection;             // arg: chain length
    public static event System.Action InvalidRelease;
    public static event System.Action<PlacedWord, int, Color> WordFound;     // word, combo, color
    public static event System.Action<int> Combo;
    public static event System.Action HintUsed;
    public static event System.Action PauseOpened;
    public static event System.Action TimeWarning;
    public static event System.Action TimeUp;
    public static event System.Action<LevelResult> LevelComplete;
    public static event System.Action<string> TopicUnlocked;
    public static event System.Action<string> BadgeEarned;
    public static event System.Action DailyStreakIncrement;

    public static void Raise_CellTouchDown(CellSelection c)                       => CellTouchDown?.Invoke(c);
    public static void Raise_CellAddedToSelection(int chainLen)                   => CellAddedToSelection?.Invoke(chainLen);
    public static void Raise_InvalidRelease()                                     => InvalidRelease?.Invoke();
    public static void Raise_WordFound(PlacedWord pw, int combo, Color color)     => WordFound?.Invoke(pw, combo, color);
    public static void Raise_Combo(int level)                                     => Combo?.Invoke(level);
    public static void Raise_HintUsed()                                           => HintUsed?.Invoke();
    public static void Raise_PauseOpened()                                        => PauseOpened?.Invoke();
    public static void Raise_TimeWarning()                                        => TimeWarning?.Invoke();
    public static void Raise_TimeUp()                                             => TimeUp?.Invoke();
    public static void Raise_LevelComplete(LevelResult r)                         => LevelComplete?.Invoke(r);
    public static void Raise_TopicUnlocked(string topicId)                        => TopicUnlocked?.Invoke(topicId);
    public static void Raise_BadgeEarned(string badgeId)                          => BadgeEarned?.Invoke(badgeId);
    public static void Raise_DailyStreakIncrement()                               => DailyStreakIncrement?.Invoke();

    // Clears every subscription. Call from AppBootstrap or test teardown to avoid leaks
    // when reloading scenes in the editor.
    public static void ClearAll() {
        CellTouchDown          = null;
        CellAddedToSelection   = null;
        InvalidRelease         = null;
        WordFound              = null;
        Combo                  = null;
        HintUsed               = null;
        PauseOpened            = null;
        TimeWarning            = null;
        TimeUp                 = null;
        LevelComplete          = null;
        TopicUnlocked          = null;
        BadgeEarned            = null;
        DailyStreakIncrement   = null;
    }
}
