using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour {
    [SerializeField] RectTransform target; // SafeArea

    public void Shake(float amplitude, float duration) {
        if (target == null) return;
        target.DOShakeAnchorPos(duration, new Vector2(amplitude, amplitude), 14, 90, false)
              .OnComplete(() => target.anchoredPosition = Vector2.zero);
    }
}
