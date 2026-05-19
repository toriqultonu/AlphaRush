using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

// 1.2s scale+fade title, then route to Home.
public class SplashView : MonoBehaviour {
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform logoRoot;
    [SerializeField] TMP_Text title;
    [SerializeField] float holdSeconds = 1.2f;

    void OnEnable() {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (logoRoot != null) logoRoot.localScale = Vector3.zero;
        if (title != null && string.IsNullOrEmpty(title.text)) title.text = "🔍 AlphaRush 🎯";
        StartCoroutine(PlayAndAdvance());
    }

    IEnumerator PlayAndAdvance() {
        if (canvasGroup != null) canvasGroup.DOFade(1f, 0.4f);
        if (logoRoot != null)    logoRoot.DOScale(1f, 0.6f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(holdSeconds);

        if (canvasGroup != null) canvasGroup.DOFade(0f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        PanelRouter.Show("Home");
    }
}
