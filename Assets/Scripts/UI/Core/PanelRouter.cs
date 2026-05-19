using System;
using System.Collections.Generic;
using UnityEngine;

// Minimal panel switcher. Wire each named panel root in inspector.
//   PanelRouter.Show("Home");
//   PanelRouter.Show("Game", new GameOpenArgs { topicId = "animals", levelId = 3 });
// Screens with a typed Open(args) method receive args via the dispatcher below.
public class PanelRouter : MonoBehaviour {
    static PanelRouter instance;
    static object pendingArgs;

    [Serializable] public struct PanelEntry { public string name; public GameObject root; }
    [SerializeField] PanelEntry[] panels;
    [SerializeField] string initialPanel = "Splash";

    Dictionary<string, GameObject> map;

    void Awake() {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        map = new Dictionary<string, GameObject>();
        if (panels != null)
            foreach (var p in panels)
                if (!string.IsNullOrEmpty(p.name) && p.root != null)
                    map[p.name] = p.root;
        foreach (var kv in map) if (kv.Value != null) kv.Value.SetActive(false);
        if (!string.IsNullOrEmpty(initialPanel)) SwitchTo(initialPanel);
    }

    public static void Show(string name, object args = null) {
        if (instance == null) {
            Debug.LogWarning($"[PanelRouter] not initialized — Show('{name}') ignored.");
            return;
        }
        pendingArgs = args;
        instance.SwitchTo(name);
    }

    void SwitchTo(string name) {
        foreach (var kv in map) if (kv.Value != null) kv.Value.SetActive(false);
        if (!map.TryGetValue(name, out var go) || go == null) {
            Debug.LogWarning($"[PanelRouter] panel '{name}' not registered.");
            return;
        }
        go.SetActive(true);
        Dispatch(go);
    }

    static void Dispatch(GameObject go) {
        var args = pendingArgs;
        pendingArgs = null;
        if (args == null) return;

        switch (args) {
            case GameOpenArgs ga: {
                var gv = go.GetComponentInChildren<GameView>(true);
                if (gv != null) gv.Open(ga.topicId, ga.levelId);
                break;
            }
            case LevelResult lr: {
                var lc = go.GetComponentInChildren<LevelCompleteView>(true);
                if (lc != null) lc.Open(lr);
                break;
            }
            case string topicId: {
                var ls = go.GetComponentInChildren<LevelSelectView>(true);
                if (ls != null) ls.Open(topicId);
                break;
            }
        }
    }
}

public class GameOpenArgs {
    public string topicId;
    public int levelId;
}
