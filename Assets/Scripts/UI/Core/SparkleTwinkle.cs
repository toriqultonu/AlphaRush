using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Gentle looping twinkle for background sparkle dots (candy theme).
// Phase/duration are serialized so each sparkle drifts out of sync.
[RequireComponent(typeof(Image))]
public class SparkleTwinkle : MonoBehaviour {
    [SerializeField] float duration = 1.6f;
    [SerializeField] float delay;
    [SerializeField] float maxAlpha = 0.85f;

    Image img;
    Sequence seq;

    void OnEnable() {
        img = GetComponent<Image>();
        var c = img.color; c.a = 0f; img.color = c;
        transform.localScale = Vector3.one * 0.6f;

        seq = DOTween.Sequence().SetDelay(delay);
        seq.Append(img.DOFade(maxAlpha, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(transform.DOScale(1f, duration * 0.5f).SetEase(Ease.OutQuad));
        seq.Append(img.DOFade(0f, duration * 0.5f).SetEase(Ease.InOutSine));
        seq.Join(transform.DOScale(0.6f, duration * 0.5f).SetEase(Ease.InQuad));
        seq.SetLoops(-1);
    }

    void OnDisable() {
        seq?.Kill();
    }
}
