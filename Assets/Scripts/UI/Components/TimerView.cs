using UnityEngine;
using TMPro;

// Stub. Real impl renders mm:ss + warning color when remaining < 10s.
public class TimerView : MonoBehaviour {
    [SerializeField] TMP_Text label;

    public virtual void SetTime(int elapsedSec, int budgetSec) {
        int remaining = Mathf.Max(0, budgetSec - elapsedSec);
        if (label != null) label.text = $"{remaining / 60:00}:{remaining % 60:00}";
    }
}
