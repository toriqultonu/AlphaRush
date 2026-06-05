using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shim for the subset of DOTween's optional UI/Audio module extensions we use.
// DOTween's Setup Panel post-processor wipes asmdefs added to the DOTween Modules folder,
// so we keep these in AlphaRush.Runtime instead. Only the call sites we use are implemented.
public static class DOTweenShortcuts {
    public static Tween DOFade(this Image target, float endValue, float duration) =>
        DOTween.ToAlpha(() => target.color, c => target.color = c, endValue, duration).SetTarget(target);

    public static Tween DOColor(this Image target, Color endValue, float duration) =>
        DOTween.To(() => target.color, c => target.color = c, endValue, duration).SetTarget(target);

    public static Tween DOFade(this CanvasGroup target, float endValue, float duration) =>
        DOTween.To(() => target.alpha, a => target.alpha = a, endValue, duration).SetTarget(target);

    public static Tween DOFade(this AudioSource target, float endValue, float duration) =>
        DOTween.To(() => target.volume, v => target.volume = v, endValue, duration).SetTarget(target);

    public static Tween DOColor(this TMP_Text target, Color endValue, float duration) =>
        DOTween.To(() => target.color, c => target.color = c, endValue, duration).SetTarget(target);

    public static Tween DOFade(this TMP_Text target, float endValue, float duration) =>
        DOTween.ToAlpha(() => target.color, c => target.color = c, endValue, duration).SetTarget(target);

    public static Tween DOShakeAnchorPos(this RectTransform target, float duration, Vector2 strength,
                                        int vibrato = 14, float randomness = 90f, bool snapping = false, bool fadeOut = true) {
        var startPos = target.anchoredPosition;
        var seq = DOTween.Sequence().SetTarget(target);
        int steps = Mathf.Max(4, Mathf.CeilToInt(duration * 60f));
        float stepDur = duration / steps;
        for (int i = 0; i < steps; i++) {
            float decay = fadeOut ? 1f - (float)i / steps : 1f;
            var offset = new Vector2(
                (Random.value * 2f - 1f) * strength.x * decay,
                (Random.value * 2f - 1f) * strength.y * decay);
            var to = startPos + offset;
            seq.Append(DOTween.To(() => target.anchoredPosition, v => target.anchoredPosition = v, to, stepDur).SetEase(Ease.Linear));
        }
        seq.Append(DOTween.To(() => target.anchoredPosition, v => target.anchoredPosition = v, startPos, stepDur).SetEase(Ease.Linear));
        return seq;
    }
}
