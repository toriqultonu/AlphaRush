using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LevelDotView : MonoBehaviour {
    [SerializeField] TMP_Text numberLabel;
    [SerializeField] GameObject[] starIcons;   // expect length 3
    [SerializeField] GameObject lockIcon, checkRing;
    [SerializeField] Button pressArea;

    public System.Action OnPressed;

    public void Bind(int levelId, int stars, bool unlocked) {
        if (numberLabel != null) numberLabel.text = levelId.ToString();
        if (lockIcon  != null) lockIcon.SetActive(!unlocked);
        if (checkRing != null) checkRing.SetActive(stars > 0);
        if (starIcons != null) {
            for (int i = 0; i < starIcons.Length; i++)
                if (starIcons[i] != null) starIcons[i].SetActive(i < stars);
        }

        if (pressArea == null) return;
        pressArea.interactable = unlocked;
        pressArea.onClick.RemoveAllListeners();
        pressArea.onClick.AddListener(HandlePress);
    }

    void HandlePress() {
        transform.DOScale(0.92f, 0.08f).OnComplete(() =>
            transform.DOScale(1f, 0.12f).SetEase(Ease.OutBack));
        OnPressed?.Invoke();
    }
}
