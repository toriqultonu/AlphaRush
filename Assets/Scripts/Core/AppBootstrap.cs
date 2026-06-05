using UnityEngine;
using UnityEngine.SceneManagement;

// Entry point. Place a single GameObject with this component in Bootstrap.unity.
// Spawns DontDestroyOnLoad services, preloads topics.json, then loads Main.unity.
public class AppBootstrap : MonoBehaviour {
    [SerializeField] string mainSceneName = "Main";
    [SerializeField] string bootstrapSceneName = "Bootstrap";

    void Awake() {
        DontDestroyOnLoad(gameObject);

        // POCO services.
        ServiceLocator.Progress = new ProgressStorage();
        ServiceLocator.Settings = new SettingsStorage();
        ServiceLocator.GameState = new GameStateStorage();
        ServiceLocator.Daily = new DailyChallengeStorage();

        // Content pipeline — local for v1; remote stub gated by AppConfig flag.
        IContentDataSource source = AppConfig.UseRemoteContent
            ? (IContentDataSource)new RemoteContentDataSource()
            : new LocalContentDataSource();
        ServiceLocator.Content = new ContentRepository(source);

        // MonoBehaviour services live on this GameObject so they share DontDestroyOnLoad.
        ServiceLocator.Sound = GetComponent<SoundManager>() ?? gameObject.AddComponent<SoundManager>();
        ServiceLocator.Music = GetComponent<MusicManager>() ?? gameObject.AddComponent<MusicManager>();
    }

    async void Start() {
        var settings = ServiceLocator.Settings.Load();
        ServiceLocator.Sound.SetEnabled(settings.soundEnabled);
        ServiceLocator.Music.SetEnabled(settings.soundEnabled);
        ServiceLocator.Music.SetVolume(settings.musicVolume);
        HapticManager.Enabled = settings.hapticsEnabled;

        await ServiceLocator.Content.GetTopicsAsync();

        if (SceneManager.GetActiveScene().name == bootstrapSceneName)
            _ = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
    }
}
