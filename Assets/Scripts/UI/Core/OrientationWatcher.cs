using UnityEngine;
using UnityEngine.Events;

public class OrientationWatcher : MonoBehaviour {
    public UnityEvent<bool> OnOrientationChanged; // true = portrait
    bool wasPortrait;

    void Start() { wasPortrait = Screen.height >= Screen.width; OnOrientationChanged?.Invoke(wasPortrait); }

    void Update() {
        bool portrait = Screen.height >= Screen.width;
        if (portrait != wasPortrait) {
            wasPortrait = portrait;
            OnOrientationChanged?.Invoke(portrait);
        }
    }
}
