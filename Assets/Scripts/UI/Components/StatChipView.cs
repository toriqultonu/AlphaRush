using UnityEngine;
using TMPro;

// Small two-line stat tile used in HomeView (Topics / Levels / Stars / Streak).
public class StatChipView : MonoBehaviour {
    [SerializeField] TMP_Text labelText;
    [SerializeField] TMP_Text valueText;

    public void Set(string label, string value) {
        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value;
    }
}
