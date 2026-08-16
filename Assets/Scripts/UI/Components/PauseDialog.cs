using UnityEngine;
using UnityEngine.UI;

// Modal pause sheet. GameView assigns the callbacks and toggles activation.
public class PauseDialog : MonoBehaviour {
    [SerializeField] Button resumeBtn, restartBtn, quitBtn;

    public System.Action OnResume, OnRestart, OnQuit;

    void OnEnable() {
        if (resumeBtn  != null) resumeBtn.onClick.AddListener(HandleResume);
        if (restartBtn != null) restartBtn.onClick.AddListener(HandleRestart);
        if (quitBtn    != null) quitBtn.onClick.AddListener(HandleQuit);
    }

    void OnDisable() {
        if (resumeBtn  != null) resumeBtn.onClick.RemoveListener(HandleResume);
        if (restartBtn != null) restartBtn.onClick.RemoveListener(HandleRestart);
        if (quitBtn    != null) quitBtn.onClick.RemoveListener(HandleQuit);
    }

    void HandleResume()  { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); OnResume?.Invoke(); }
    void HandleRestart() { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); OnRestart?.Invoke(); }
    void HandleQuit()    { ServiceLocator.Sound?.Play(SoundEvent.BUTTON); OnQuit?.Invoke(); }
}
