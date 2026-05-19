using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// §13.6 + §14.3 — modal sheet, sequential star punch animation, three buttons.
public class LevelCompleteView : MonoBehaviour {
    [SerializeField] GameObject[] starSlots;     // 3 star icons, initially hidden
    [SerializeField] TMP_Text xpText, timeText, headerText;
    [SerializeField] Button nextBtn, replayBtn, topicsBtn;

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

    public void Open(LevelResult r) {
        result = r;
        if (starSlots != null) foreach (var s in starSlots) if (s != null) s.SetActive(false);

        if (headerText != null) headerText.text = "Level Complete!";
        if (xpText     != null) xpText.text     = $"+{r.xpEarned} XP";
        if (timeText   != null) timeText.text   = $"{r.timeSeconds / 60:00}:{r.timeSeconds % 60:00}";

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
        PanelRouter.Show("Game", new GameOpenArgs { topicId = result.topicId, levelId = result.levelId + 1 });
    }

    void OnReplay() {
        if (result == null) return;
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        PanelRouter.Show("Game", new GameOpenArgs { topicId = result.topicId, levelId = result.levelId });
    }

    void OnTopics() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        PanelRouter.Show("TopicList");
    }
}
