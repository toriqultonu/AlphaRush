// Simple settable-static facade. AppBootstrap populates these in Awake;
// screen code reads via ServiceLocator.X. All getters return null until
// AppBootstrap.Awake has run — guard with null-conditional in early callers.
public static class ServiceLocator {
    public static SoundManager Sound { get; set; }
    public static MusicManager Music { get; set; }
    public static ContentRepository Content { get; set; }
    public static ProgressStorage Progress { get; set; }
    public static SettingsStorage Settings { get; set; }
    public static GameStateStorage GameState { get; set; }
    public static DailyChallengeStorage Daily { get; set; }
    public static PanelRouter Router { get; set; }
}
