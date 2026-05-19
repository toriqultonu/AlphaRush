using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour {
    Rect lastSafeArea;
    Vector2Int lastScreen;

    void Update() {
        var safe = Screen.safeArea;
        var screen = new Vector2Int(Screen.width, Screen.height);
        if (safe == lastSafeArea && screen == lastScreen) return;
        lastSafeArea = safe; lastScreen = screen;
        Apply(safe);
    }

    void Apply(Rect safe) {
        var rt = (RectTransform)transform;
        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= Screen.width;  anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;  anchorMax.y /= Screen.height;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
    }
}
