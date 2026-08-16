using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// Candy-style squash-and-bounce on press. Attach next to any Button.
public class ButtonBouncer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] float pressScale = 0.9f;

    public void OnPointerDown(PointerEventData e) {
        if (ReduceMotion) return;
        transform.DOKill();
        transform.DOScale(pressScale, 0.08f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData e) {
        if (ReduceMotion) { transform.localScale = Vector3.one; return; }
        transform.DOKill();
        transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack, 3f);
    }

    void OnDisable() {
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    static bool ReduceMotion => ServiceLocator.Settings?.Load()?.reduceMotion ?? false;
}
