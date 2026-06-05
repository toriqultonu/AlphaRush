using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopicListView : MonoBehaviour {
    [SerializeField] Transform gridContainer;       // GridLayoutGroup, 2 columns
    [SerializeField] TopicCardView cardPrefab;
    [SerializeField] Button backBtn;

    readonly List<TopicCardView> spawned = new();

    void OnEnable() {
        if (backBtn != null) backBtn.onClick.AddListener(OnBack);
        Refresh();
    }

    void OnDisable() {
        if (backBtn != null) backBtn.onClick.RemoveListener(OnBack);
    }

    async void Refresh() {
        ClearSpawned();
        var repo     = ServiceLocator.Content;
        var progress = ServiceLocator.Progress?.Load();
        if (repo == null) return;

        var topics = await repo.GetTopicsAsync();
        if (topics == null || gridContainer == null || cardPrefab == null) return;

        foreach (var t in topics) {
            var card     = Instantiate(cardPrefab, gridContainer);
            bool unlocked = progress?.unlockedTopicIds?.Contains(t.id) ?? false;
            int starsInTopic = CountStarsForTopic(progress, t.id);
            card.Bind(t, unlocked, progress?.totalStars ?? 0, starsInTopic);
            string capturedId = t.id;
            bool capturedUnlocked = unlocked;
            card.OnPressed = () => {
                if (!capturedUnlocked) {
                    ServiceLocator.Sound?.Play(SoundEvent.MISS);
                    return;
                }
                ServiceLocator.Router?.Show(Routes.LevelSelect);
                FindAnyObjectByType<LevelSelectView>(FindObjectsInactive.Include)?.Open(capturedId);
            };
            spawned.Add(card);
        }
    }

    static int CountStarsForTopic(PlayerProgress p, string topicId) {
        if (p?.bestResults == null) return 0;
        string prefix = topicId + ":";
        int total = 0;
        foreach (var kv in p.bestResults)
            if (kv.Key.StartsWith(prefix)) total += kv.Value.stars;
        return total;
    }

    void ClearSpawned() {
        foreach (var c in spawned) if (c != null) Destroy(c.gameObject);
        spawned.Clear();
    }

    void OnBack() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        ServiceLocator.Router?.Show(Routes.Home);
    }
}
