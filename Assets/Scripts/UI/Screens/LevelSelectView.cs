using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectView : MonoBehaviour {
    [SerializeField] Transform gridContainer;  // GridLayoutGroup, 3 columns
    [SerializeField] LevelDotView dotPrefab;
    [SerializeField] Button backBtn;
    [SerializeField] float zigzagOffsetX = 30f;

    readonly List<LevelDotView> spawned = new();
    string topicId;

    void OnEnable() {
        if (backBtn != null) backBtn.onClick.AddListener(OnBack);
        if (!string.IsNullOrEmpty(topicId)) Refresh();
    }

    void OnDisable() {
        if (backBtn != null) backBtn.onClick.RemoveListener(OnBack);
    }

    public void Open(string topicId) {
        this.topicId = topicId;
        if (gameObject.activeInHierarchy) Refresh();
    }

    void Refresh() {
        ClearSpawned();
        if (gridContainer == null || dotPrefab == null || string.IsNullOrEmpty(topicId)) return;

        var progress = ServiceLocator.Progress?.Load();
        int prevStars = 1; // first level always playable.

        for (int i = 1; i <= AppConfig.LevelsPerTopic; i++) {
            var dot   = Instantiate(dotPrefab, gridContainer);
            string key = $"{topicId}:{i}";
            int stars  = (progress?.bestResults != null && progress.bestResults.TryGetValue(key, out var r)) ? r.stars : 0;
            bool unlocked = (i == 1) || prevStars > 0;
            dot.Bind(i, stars, unlocked);

            int capturedLevel = i;
            bool capturedUnlocked = unlocked;
            dot.OnPressed = () => {
                if (!capturedUnlocked) { ServiceLocator.Sound?.Play(SoundEvent.MISS); return; }
                ServiceLocator.Router?.Show(Routes.Game);
                FindAnyObjectByType<GameView>(FindObjectsInactive.Include)?.Open(topicId, capturedLevel);
            };

            // Zig-zag: shift every second row +X.
            int row = (i - 1) / 3;
            if (row % 2 == 1 && dot.transform is RectTransform rt)
                rt.anchoredPosition += new Vector2(zigzagOffsetX, 0f);

            spawned.Add(dot);
            prevStars = stars;
        }
    }

    void ClearSpawned() {
        foreach (var d in spawned) if (d != null) Destroy(d.gameObject);
        spawned.Clear();
    }

    void OnBack() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        ServiceLocator.Router?.Show(Routes.TopicList);
    }
}
