using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Candy confetti for the victory sheet: colored diamonds/squares rain down
// across this RectTransform, tumbling and fading. Pure UGUI, no textures.
public class ConfettiRain : MonoBehaviour {
    [SerializeField] int pieceCount = 60;
    [SerializeField] float minFall = 1.1f;
    [SerializeField] float maxFall = 2.4f;

    readonly List<GameObject> live = new();

    public void Play() {
        if (ServiceLocator.Settings?.Load()?.reduceMotion ?? false) return;
        Stop();

        var area = (RectTransform)transform;
        float w = area.rect.width, h = area.rect.height;
        if (w <= 0f) { w = 1080f; h = 1920f; }

        for (int i = 0; i < pieceCount; i++) {
            var go = new GameObject("Confetti", typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            float size = Random.Range(16f, 34f);
            rt.sizeDelta = new Vector2(size, size * Random.Range(0.6f, 1f));
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(Random.Range(-w * 0.5f, w * 0.5f),
                                              Random.Range(20f, 240f));
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            var img = go.AddComponent<Image>();
            img.color = AppColors.ChipColors[Random.Range(0, AppColors.ChipColors.Length)];
            img.raycastTarget = false;

            float dur = Random.Range(minFall, maxFall);
            float delay = Random.Range(0f, 0.5f);
            float driftX = rt.anchoredPosition.x + Random.Range(-120f, 120f);
            // DOAnchorPos* live in DOTween's UI module (firstpass asm) — use
            // generic DOTween.To like the rest of the project's shims.
            DOTween.To(() => rt.anchoredPosition.y,
                       y => rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y),
                       -h - 60f, dur)
                   .SetTarget(rt).SetDelay(delay).SetEase(Ease.InQuad);
            DOTween.To(() => rt.anchoredPosition.x,
                       x => rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y),
                       driftX, dur)
                   .SetTarget(rt).SetDelay(delay).SetEase(Ease.InOutSine);
            rt.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), dur, RotateMode.FastBeyond360)
              .SetDelay(delay);
            img.DOFade(0f, 0.35f).SetDelay(delay + dur - 0.35f);

            live.Add(go);
        }

        // Sweep everything once the slowest piece has landed.
        DOVirtual.DelayedCall(maxFall + 0.6f, Stop);
    }

    public void Stop() {
        foreach (var go in live) {
            if (go == null) continue;
            go.transform.DOKill();
            Destroy(go);
        }
        live.Clear();
    }

    void OnDisable() => Stop();
}
