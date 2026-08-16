using UnityEngine;
using UnityEngine.UI;

// Shown when a saved game exists for the opened level: Resume or Restart.
public class ResumeDialog : MonoBehaviour {
    [SerializeField] Button resumeBtn, restartBtn;

    public System.Action OnResume, OnRestart;

    void OnEnable() {
        if (resumeBtn  != null) resumeBtn.onClick.AddListener(HandleResume);
        if (restartBtn != null) restartBtn.onClick.AddListener(HandleRestart);
    }

    void OnDisable() {
        if (resumeBtn  != null) resumeBtn.onClick.RemoveListener(HandleResume);
        if (restartBtn != null) restartBtn.onClick.RemoveListener(HandleRestart);
    }

    void HandleResume()  { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); OnResume?.Invoke(); }
    void HandleRestart() { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); OnRestart?.Invoke(); }
}
