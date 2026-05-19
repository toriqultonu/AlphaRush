using UnityEngine;
using TMPro;
using DG.Tweening;

// §14.5 — combo banner. Auto-hides after 1.2s.
public class ComboBannerView : MonoBehaviour {
    [SerializeField] GameObject banner;
    [SerializeField] TMP_Text bannerText;

    public void Show(int level) {
        if (banner == null || bannerText == null) return;
        banner.SetActive(true);
        bannerText.text = $"COMBO x{level}!";
        bannerText.color = level >= 5 ? Color.red : Color.yellow;
        var rt = bannerText.rectTransform;
        rt.localScale = Vector3.zero;
        rt.DOScale(1.15f, 0.18f).SetEase(Ease.OutBack)
          .OnComplete(() => rt.DOScale(1f, 0.12f));
        DOVirtual.DelayedCall(1.2f, () => { if (banner != null) banner.SetActive(false); });
    }
}
