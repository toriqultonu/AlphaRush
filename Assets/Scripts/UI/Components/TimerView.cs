using UnityEngine;
using TMPro;

// Count-up stopwatch (no time limit). Tint shifts once the 3-star window
// (half the difficulty budget) has passed, as a gentle "hurry for stars" cue.
public class TimerView : MonoBehaviour {
    [SerializeField] TMP_Text label;

    public virtual void SetTime(int elapsedSec, int budgetSec) {
        if (label == null) return;
        label.text = $"{elapsedSec / 60:00}:{elapsedSec % 60:00}";
        label.color = (budgetSec > 0 && elapsedSec > budgetSec / 2)
            ? AppColors.CandyPinkDeep
            : AppColors.LetterBrown;
    }
}
