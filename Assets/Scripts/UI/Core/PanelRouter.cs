using System;
using UnityEngine;
using DG.Tweening;

// Inspector-wired panel switcher. Parallel arrays: mainPanels[i] is shown when
// Show(panelNames[i]) is called; all others are hidden. Registers itself to
// ServiceLocator.Router in Awake — call via ServiceLocator.Router.Show(Routes.X).
//
// Args-style navigation (passing state through Show) is intentionally not
// supported. Callers that need to seed a view (e.g. GameView.Open(topicId,
// levelId)) should resolve the target view directly with FindAnyObjectByType,
// then invoke its typed Open() before/after Show.
public class PanelRouter : MonoBehaviour {
    [SerializeField] GameObject[] mainPanels;
    [SerializeField] string[] panelNames;

    public string Current { get; private set; }
    public event Action<string> OnPanelChanged;

    void Awake() {
        ServiceLocator.Router = this;
    }

    void OnDestroy() {
        if (ServiceLocator.Router == this) ServiceLocator.Router = null;
    }

    public void Show(string panelName) {
        if (mainPanels == null || mainPanels.Length == 0) {
            Debug.LogWarning($"[PanelRouter] mainPanels unset — Show('{panelName}') ignored.");
            return;
        }

        int match = -1;
        if (panelNames != null) {
            int n = Mathf.Min(mainPanels.Length, panelNames.Length);
            for (int i = 0; i < n; i++) {
                if (panelNames[i] == panelName) { match = i; break; }
            }
        }

        if (match < 0) {
            Debug.LogWarning($"[PanelRouter] panel '{panelName}' not registered.");
            return;
        }

        for (int i = 0; i < mainPanels.Length; i++) {
            if (mainPanels[i] != null) mainPanels[i].SetActive(i == match);
        }

        AnimateIn(mainPanels[match]);

        Current = panelName;
        OnPanelChanged?.Invoke(panelName);
    }

    // Candy-style pop: fade + overshoot scale on every panel entrance.
    static void AnimateIn(GameObject panel) {
        if (panel == null) return;
        if (ServiceLocator.Settings?.Load()?.reduceMotion ?? false) return;

        var t = panel.transform;
        t.DOKill();
        t.localScale = Vector3.one * 0.94f;
        t.DOScale(1f, 0.28f).SetEase(Ease.OutBack, 2.2f);

        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null) {
            cg.DOKill();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.2f);
        }
    }
}
