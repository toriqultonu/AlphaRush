using UnityEngine;
using UnityEngine.UI;
using TMPro;

// First-launch tutorial: a short paged overlay. Fires OnFinished on Skip or
// after the last step's Next.
public class TutorialOverlay : MonoBehaviour {
    [SerializeField] TMP_Text titleText, bodyText;
    [SerializeField] Button nextBtn, skipBtn;
    [SerializeField] TMP_Text nextBtnLabel;

    public System.Action OnFinished;

    static readonly (string title, string body)[] Steps = {
        ("Find the Words",  "Words from the list are hidden in the letter grid — across, down and diagonally."),
        ("Drag to Select",  "Press a letter and drag in a straight line to the last letter, then release."),
        ("Beat the Clock",  "Finish before time runs out. Fewer hints and a faster time earn more stars!"),
    };

    int step;

    void OnEnable() {
        if (nextBtn != null) nextBtn.onClick.AddListener(HandleNext);
        if (skipBtn != null) skipBtn.onClick.AddListener(Finish);
        step = 0;
        Render();
    }

    void OnDisable() {
        if (nextBtn != null) nextBtn.onClick.RemoveListener(HandleNext);
        if (skipBtn != null) skipBtn.onClick.RemoveListener(Finish);
    }

    public void Show() {
        step = 0;
        gameObject.SetActive(true);
        Render();
    }

    void Render() {
        if (titleText != null) titleText.text = Steps[step].title;
        if (bodyText  != null) bodyText.text  = Steps[step].body;
        if (nextBtnLabel != null) nextBtnLabel.text = step == Steps.Length - 1 ? "Let's Go!" : "Next";
    }

    void HandleNext() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        if (step >= Steps.Length - 1) { Finish(); return; }
        step++;
        Render();
    }

    void Finish() {
        gameObject.SetActive(false);
        OnFinished?.Invoke();
    }
}
