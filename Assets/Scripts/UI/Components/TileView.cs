using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TileView : MonoBehaviour {
    [SerializeField] Image background;
    [SerializeField] Image highlight;
    [SerializeField] Image foundOverlay;
    [SerializeField] TMP_Text letter;

    public int Row, Col;

    public void Set(char c, int row, int col) {
        Row = row; Col = col;
        letter.text = c.ToString();
    }

    public void SetSelected(bool on) {
        highlight.DOFade(on ? 1f : 0f, 0.1f);
        transform.DOScale(on ? 1.08f : 1f, 0.1f);
    }

    public void PlayFound(Color tint) {
        foundOverlay.color = new Color(tint.r, tint.g, tint.b, 0f);
        foundOverlay.DOFade(0.85f, 0.18f);
        transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 6, 0.5f);
    }

    public void PlayHintPulse() {
        var seq = DOTween.Sequence();
        seq.Append(highlight.DOFade(1f, 0.2f));
        seq.Append(highlight.DOFade(0f, 0.2f));
        seq.SetLoops(3);
    }

    public void Reset() {
        highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0f);
        foundOverlay.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;
    }
}
