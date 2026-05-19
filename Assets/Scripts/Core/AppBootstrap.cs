using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Entry point. Place a single GameObject with this component in Bootstrap.unity.
// Spawns DontDestroyOnLoad services, preloads topics.json, then additively loads Main.unity.
public class AppBootstrap : MonoBehaviour {
    public static AppBootstrap Instance { get; private set; }

    public SoundManager Sound { get; private set; }
    public MusicManager Music { get; private set; }
    public ProgressStorage Progress { get; private set; }
    public SettingsStorage SettingsStore { get; private set; }
    public GameStateStorage GameStateStore { get; private set; }
    public DailyChallengeStorage DailyStore { get; private set; }
    public ContentRepository Content { get; private set; }

    [SerializeField] string mainSceneName = "Main";

    async void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // MonoBehaviour services — parent under this object so DontDestroyOnLoad propagates.
        var soundGo = new GameObject("SoundManager");
        soundGo.transform.SetParent(transform);
        Sound = soundGo.AddComponent<SoundManager>();

        var musicGo = new GameObject("MusicManager");
        musicGo.transform.SetParent(transform);
        Music = musicGo.AddComponent<MusicManager>();

        // POCO services — instance lifetime tracked by AppBootstrap.
        Progress = new ProgressStorage();
        SettingsStore = new SettingsStorage();
        GameStateStore = new GameStateStorage();
        DailyStore = new DailyChallengeStorage();

        IContentDataSource source = AppConfig.UseRemoteContent
            ? (IContentDataSource)new RemoteContentDataSource()
            : new LocalContentDataSource();
        Content = new ContentRepository(source);

        // Apply persisted settings to runtime services.
        var settings = SettingsStore.Load();
        Sound.SetEnabled(settings.soundEnabled);
        Music.SetVolume(settings.musicVolume);
        HapticManager.Enabled = settings.hapticsEnabled;

        // Preload topics so first screen has data ready synchronously.
        await Content.GetTopicsAsync();

        await LoadMainAdditiveAsync();
    }

    Task LoadMainAdditiveAsync() {
        if (SceneManager.GetSceneByName(mainSceneName).isLoaded)
            return Task.CompletedTask;
        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        if (op == null) { tcs.SetResult(false); return tcs.Task; }
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }
}
