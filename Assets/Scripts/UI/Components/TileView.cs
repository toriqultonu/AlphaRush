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

    // Candy-style staggered pop-in when the grid builds.
    public void PlaySpawn(float delay) {
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 2.4f).SetDelay(delay);
    }

    public void SetSelected(bool on) {
        highlight.DOFade(on ? 1f : 0f, 0.1f);
        transform.DOScale(on ? 1.1f : 1f, 0.1f);
    }

    // Jelly wobble + candy tint flood on found.
    public void PlayFound(Color tint) {
        foundOverlay.color = new Color(tint.r, tint.g, tint.b, 0f);
        foundOverlay.DOFade(0.85f, 0.18f);
        transform.DOKill(complete: true);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.22f, 0.4f, 8, 0.6f);
    }

    public void PlayHintPulse() {
        var seq = DOTween.Sequence();
        seq.Append(highlight.DOFade(1f, 0.2f));
        seq.Append(highlight.DOFade(0f, 0.2f));
        seq.SetLoops(3);
    }

    // Named ResetVisuals (not Reset) — Reset is a Unity editor-time message and
    // fires on AddComponent before serialized refs exist.
    public void ResetVisuals() {
        if (highlight != null)
            highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0f);
        if (foundOverlay != null) foundOverlay.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;
    }
}
