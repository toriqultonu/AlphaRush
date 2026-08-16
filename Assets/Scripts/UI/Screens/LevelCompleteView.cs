using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// §13.6 + §14.3 — modal sheet, sequential star punch animation, three buttons.
public class LevelCompleteView : MonoBehaviour {
    [SerializeField] GameObject[] starSlots;     // 3 star icons, initially hidden
    [SerializeField] TMP_Text xpText, timeText, headerText;
    [SerializeField] Button nextBtn, replayBtn, topicsBtn;
    [SerializeField] GameObject newBestBadge;    // "NEW BEST!" pill, hidden by default
    [SerializeField] ConfettiRain confetti;

    LevelResult result;

    void OnEnable() {
        if (nextBtn   != null) nextBtn.onClick.AddListener(OnNext);
        if (replayBtn != null) replayBtn.onClick.AddListener(OnReplay);
        if (topicsBtn != null) topicsBtn.onClick.AddListener(OnTopics);
    }

    void OnDisable() {
        if (nextBtn   != null) nextBtn.onClick.RemoveListener(OnNext);
        if (replayBtn != null) replayBtn.onClick.RemoveListener(OnReplay);
        if (topicsBtn != null) topicsBtn.onClick.RemoveListener(OnTopics);
    }

    public void Open(LevelResult r, bool isNewBest = false) {
        result = r;
        if (starSlots != null) foreach (var s in starSlots) if (s != null) s.SetActive(false);

        if (headerText != null) {
            headerText.text = "Level Complete!";
            // Bouncy header entrance.
            headerText.transform.localScale = Vector3.zero;
            headerText.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack, 2.2f);
        }
        if (xpText   != null) xpText.text   = $"+{r.xpEarned} XP";
        if (timeText != null) timeText.text = $"{r.timeSeconds / 60:00}:{r.timeSeconds % 60:00}";

        // Next hides on the last level of a topic.
        if (nextBtn != null) nextBtn.gameObject.SetActive(r.levelId < AppConfig.LevelsPerTopic);

        // High-score celebration: badge stamps in after the stars land.
        if (newBestBadge != null) {
            newBestBadge.SetActive(false);
            if (isNewBest) {
                DOVirtual.DelayedCall(0.75f, () => {
                    if (newBestBadge == null || !gameObject.activeInHierarchy) return;
                    newBestBadge.SetActive(true);
                    newBestBadge.transform.localScale = Vector3.one * 2.2f;
                    newBestBadge.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 2.5f);
                    ServiceLocator.Sound?.Play(SoundEvent.UNLOCK);
                    HapticManager.Success();
                });
            }
        }

        confetti?.Play();
        PlayStarSequence(r.stars);
    }

    void PlayStarSequence(int stars) {
        if (starSlots == null) return;
        int count = Mathf.Min(stars, starSlots.Length);
        for (int i = 0; i < count; i++) {
            int idx = i;
            DOVirtual.DelayedCall(idx * 0.2f, () => {
                var s = starSlots[idx];
                if (s == null) return;
                s.SetActive(true);
                s.transform.localScale = Vector3.zero;
                s.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack)
                  .OnComplete(() => s.transform.DOScale(1f, 0.1f));
                ServiceLocator.Sound?.Play(SoundEvent.STAR_POP);
            });
        }
    }

    void OnNext() {
        if (result == null) return;
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        ServiceLocator.Router?.Show(Routes.Game);
        FindAnyObjectByType<GameView>(FindObjectsInactive.Include)?.Open(result.topicId, result.levelId + 1);
    }

    void OnReplay() {
        if (result == null) return;
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        ServiceLocator.Router?.Show(Routes.Game);
        FindAnyObjectByType<GameView>(FindObjectsInactive.Include)?.Open(result.topicId, result.levelId);
    }

    void OnTopics() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        ServiceLocator.Router?.Show(Routes.TopicList);
    }
}
