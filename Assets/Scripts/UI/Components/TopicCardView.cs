using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TopicCardView : MonoBehaviour {
    [SerializeField] TMP_Text titleText, iconText, statsText, lockText;
    [SerializeField] Image accentStripe;
    [SerializeField] Image iconImage;            // sprite icon from Resources/TopicIcons
    [SerializeField] GameObject lockOverlay;
    [SerializeField] Button pressArea;

    public System.Action OnPressed;

    public void Bind(Topic topic, bool unlocked, int totalStars, int starsInTopic) {
        if (titleText != null) titleText.text = topic.name;
        BindIcon(topic);
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

    // Prefer a generated sprite icon (Resources/TopicIcons/<id>); fall back to
    // the emoji glyph if the font can render it, else the topic's initial.
    void BindIcon(Topic topic) {
        var sprite = Resources.Load<Sprite>($"TopicIcons/{topic.id}");
        if (iconImage != null) {
            iconImage.sprite = sprite;
            iconImage.gameObject.SetActive(sprite != null);
        }
        if (iconText != null) {
            iconText.gameObject.SetActive(sprite == null);
            if (sprite == null) iconText.text = RenderableIcon(topic);
        }
    }

    string RenderableIcon(Topic topic) {
        string icon = topic.icon;
        if (!string.IsNullOrEmpty(icon) && iconText != null && iconText.font != null) {
            uint cp = (uint)char.ConvertToUtf32(icon, 0);
            var table = iconText.font.characterLookupTable;
            if (table != null && table.ContainsKey(cp)) return icon;
        }
        return string.IsNullOrEmpty(topic.name) ? "?" : topic.name.Substring(0, 1).ToUpperInvariant();
    }

    static Color UnpackColor(long packed) {
        float a = ((packed >> 24) & 0xFF) / 255f;
        float r = ((packed >> 16) & 0xFF) / 255f;
        float g = ((packed >> 8)  & 0xFF) / 255f;
        float b = ( packed        & 0xFF) / 255f;
        return new Color(r, g, b, a);
    }
}
