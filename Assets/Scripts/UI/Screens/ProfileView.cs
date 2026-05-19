using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileView : MonoBehaviour {
    [SerializeField] Transform badgeContainer;
    [SerializeField] GameObject badgeCellPrefab; // child should have a TMP_Text label
    [SerializeField] TMP_Text totalStarsText, totalXpText, streakText, badgeCountText;
    [SerializeField] Button backBtn, resetBtn;

    readonly List<GameObject> spawned = new();

    void OnEnable() {
        if (backBtn  != null) backBtn.onClick.AddListener(OnBack);
        if (resetBtn != null) resetBtn.onClick.AddListener(OnReset);
        Refresh();
    }

    void OnDisable() {
        if (backBtn  != null) backBtn.onClick.RemoveListener(OnBack);
        if (resetBtn != null) resetBtn.onClick.RemoveListener(OnReset);
    }

    void Refresh() {
        var p = ServiceLocator.Progress?.Load();
        if (p == null) return;

        if (totalStarsText != null) totalStarsText.text = p.totalStars.ToString();
        if (totalXpText    != null) totalXpText.text    = p.totalXp.ToString();
        if (streakText     != null) streakText.text     = $"{p.streakDays}d";
        if (badgeCountText != null) badgeCountText.text = $"{p.badges?.Count ?? 0} badges";

        foreach (var g in spawned) if (g != null) Destroy(g);
        spawned.Clear();

        if (badgeContainer == null || badgeCellPrefab == null || p.badges == null) return;
        foreach (var badgeId in p.badges) {
            var cell  = Instantiate(badgeCellPrefab, badgeContainer);
            var label = cell.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = badgeId;
            spawned.Add(cell);
        }
    }

    void OnBack() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        PanelRouter.Show("Home");
    }

    void OnReset() {
        ServiceLocator.Progress ?.ClearAll();
        ServiceLocator.GameState?.ClearAll();
        ServiceLocator.Daily    ?.ClearAll();
        ServiceLocator.Sound    ?.Play(SoundEvent.MISS);
        Refresh();
    }
}
