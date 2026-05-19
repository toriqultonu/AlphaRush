using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TopicCardView : MonoBehaviour {
    [SerializeField] TMP_Text titleText, iconText, statsText, lockText;
    [SerializeField] Image accentStripe;
    [SerializeField] GameObject lockOverlay;
    [SerializeField] Button pressArea;

    public System.Action OnPressed;

    public void Bind(Topic topic, bool unlocked, int totalStars, int starsInTopic) {
        if (titleText != null) titleText.text = topic.name;
        if (iconText  != null) iconText.text  = topic.icon;
        if (accentStripe != null) accentStripe.color = UnpackColor(topic.accentColor);
        if (statsText != null) statsText.text = $"{starsInTopic} ★";
        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
        if (!unlocked && lockText != null)
            lockText.text = $"Need {topic.unlockStarsRequired} ★ (you have {totalStars})";

        if (pressArea == null) return;
        pressArea.onClick.RemoveAllListeners();
        pressArea.onClick.AddListener(HandlePress);
    }

    // §14.1 — press tween: 0.95 down (Linear) → 1.0 up (OutBack).
    void HandlePress() {
        transform.DOScale(AppDimensions.TopicPressScale, 0.08f).OnComplete(() =>
            transform.DOScale(1f, 0.12f).SetEase(Ease.OutBack));
        OnPressed?.Invoke();
    }

    static Color UnpackColor(long packed) {
        float a = ((packed >> 24) & 0xFF) / 255f;
        float r = ((packed >> 16) & 0xFF) / 255f;
        float g = ((packed >> 8)  & 0xFF) / 255f;
        float b = ( packed        & 0xFF) / 255f;
        return new Color(r, g, b, a);
    }
}
