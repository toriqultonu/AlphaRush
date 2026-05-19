using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WordChipView : MonoBehaviour {
    [SerializeField] TMP_Text label;
    [SerializeField] Image background;

    public string Word { get; private set; }
    bool found;

    public void Bind(string word) {
        Word = word;
        found = false;
        if (label != null) {
            label.text = word;
            label.alpha = 1f;
            label.fontStyle = FontStyles.Normal;
        }
        if (background != null) background.color = AppColors.CardBackground;
        transform.localScale = Vector3.one;
    }

    // §14.2 — yoyo scale punch + tinted background fade on word-found event.
    public void MarkFound(Color highlight) {
        if (found) return;
        found = true;
        if (background != null) background.DOColor(highlight, 0.2f);
        if (label != null) {
            label.fontStyle = FontStyles.Strikethrough;
            label.DOFade(0.55f, 0.2f);
        }
        transform.DOScale(1.05f, 0.15f).SetLoops(2, LoopType.Yoyo);
    }
}
