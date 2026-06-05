using UnityEngine;
using UnityEngine.UI;

public class HomeView : MonoBehaviour {
    [SerializeField] StatChipView topicsChip, levelsChip, starsChip, streakChip;
    [SerializeField] Button playBtn, dailyBtn, profileBtn, settingsBtn;

    void OnEnable() {
        if (playBtn     != null) playBtn.onClick.AddListener(OnPlay);
        if (dailyBtn    != null) dailyBtn.onClick.AddListener(OnDaily);
        if (profileBtn  != null) profileBtn.onClick.AddListener(OnProfile);
        if (settingsBtn != null) settingsBtn.onClick.AddListener(OnSettings);
        Refresh();
    }

    void OnDisable() {
        if (playBtn     != null) playBtn.onClick.RemoveListener(OnPlay);
        if (dailyBtn    != null) dailyBtn.onClick.RemoveListener(OnDaily);
        if (profileBtn  != null) profileBtn.onClick.RemoveListener(OnProfile);
        if (settingsBtn != null) settingsBtn.onClick.RemoveListener(OnSettings);
    }

    async void Refresh() {
        var progress = ServiceLocator.Progress?.Load();
        var topics   = ServiceLocator.Content != null ? await ServiceLocator.Content.GetTopicsAsync() : null;
        if (progress == null || topics == null) return;

        int unlocked  = progress.unlockedTopicIds?.Count ?? 0;
        int completed = progress.bestResults?.Count ?? 0;

        topicsChip?.Set("Topics", $"{unlocked} / {topics.Count}");
        levelsChip?.Set("Levels", completed.ToString());
        starsChip ?.Set("Stars",  progress.totalStars.ToString());
        streakChip?.Set("Streak", $"{progress.streakDays}d");
    }

    void OnPlay()     { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); ServiceLocator.Router?.Show(Routes.TopicList); }
    void OnDaily()    { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); ServiceLocator.Router?.Show(Routes.Daily); }
    void OnProfile()  { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); ServiceLocator.Router?.Show(Routes.Profile); }
    void OnSettings() { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); ServiceLocator.Router?.Show(Routes.Settings); }
}
