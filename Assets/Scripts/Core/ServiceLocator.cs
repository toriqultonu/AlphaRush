// Static facade over AppBootstrap-owned services so screen code stays terse:
//   ServiceLocator.Sound.Play(SoundEvent.FOUND);
// All getters return null until AppBootstrap.Awake completes — guard with null-conditional.
public static class ServiceLocator {
    public static SoundManager Sound        => AppBootstrap.Instance != null ? AppBootstrap.Instance.Sound          : null;
    public static MusicManager Music        => AppBootstrap.Instance != null ? AppBootstrap.Instance.Music          : null;
    public static ProgressStorage Progress  => AppBootstrap.Instance != null ? AppBootstrap.Instance.Progress       : null;
    public static SettingsStorage Settings  => AppBootstrap.Instance != null ? AppBootstrap.Instance.SettingsStore  : null;
    public static GameStateStorage GameState=> AppBootstrap.Instance != null ? AppBootstrap.Instance.GameStateStore : null;
    public static DailyChallengeStorage Daily=> AppBootstrap.Instance != null ? AppBootstrap.Instance.DailyStore     : null;
    public static ContentRepository Content => AppBootstrap.Instance != null ? AppBootstrap.Instance.Content        : null;
}
