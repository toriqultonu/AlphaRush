using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyChallengeView : MonoBehaviour {
    [SerializeField] TMP_Text dateText, topicText, statusText, streakText;
    [SerializeField] Button playBtn, backBtn;

    DailyChallenge current;

    void OnEnable() {
        if (playBtn != null) playBtn.onClick.AddListener(OnPlay);
        if (backBtn != null) backBtn.onClick.AddListener(OnBack);
        Refresh();
    }

    void OnDisable() {
        if (playBtn != null) playBtn.onClick.RemoveListener(OnPlay);
        if (backBtn != null) backBtn.onClick.RemoveListener(OnBack);
    }

    async void Refresh() {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        current = ServiceLocator.Daily?.GetCurrent();
        if (current == null || current.date != today) {
            current = await RollForToday(today);
            ServiceLocator.Daily?.SetCurrent(current);
        }

        int streak = ServiceLocator.Daily?.GetStreak() ?? 0;

        if (dateText   != null) dateText.text   = current.date;
        if (topicText  != null) topicText.text  = current.topicId;
        if (statusText != null) statusText.text = current.completed ? $"Completed · {current.stars}★" : "Tap Play";
        if (streakText != null) streakText.text = $"Streak: {streak}d";
    }

    async Task<DailyChallenge> RollForToday(string date) {
        var topics = await ServiceLocator.Content.GetTopicsAsync();
        long seed  = (long)date.GetHashCode();
        var rng    = new System.Random((int)(seed ^ (seed >> 32)));
        var topic  = topics[rng.Next(topics.Count)];
        return new DailyChallenge {
            date       = date,
            topicId    = topic.id,
            difficulty = Difficulty.MEDIUM,
            seed       = seed,
            completed  = false,
            stars      = 0
        };
    }

    void OnPlay() {
        if (current == null) return;
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        // Use level id 1 as the playable surface. GameView ignores daily-specific scoring.
        PanelRouter.Show("Game", new GameOpenArgs { topicId = current.topicId, levelId = 1 });
    }

    void OnBack() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        PanelRouter.Show("Home");
    }
}
