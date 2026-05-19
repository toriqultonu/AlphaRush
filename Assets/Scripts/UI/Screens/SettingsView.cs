using UnityEngine;
using UnityEngine.UI;

public class SettingsView : MonoBehaviour {
    [SerializeField] Toggle soundToggle, hapticsToggle, reduceMotionToggle;
    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] Button backBtn;

    Settings settings;

    void OnEnable() {
        settings = ServiceLocator.Settings?.Load() ?? new Settings();

        if (soundToggle != null) {
            soundToggle.SetIsOnWithoutNotify(settings.soundEnabled);
            soundToggle.onValueChanged.AddListener(OnSound);
        }
        if (hapticsToggle != null) {
            hapticsToggle.SetIsOnWithoutNotify(settings.hapticsEnabled);
            hapticsToggle.onValueChanged.AddListener(OnHaptics);
        }
        if (reduceMotionToggle != null) {
            reduceMotionToggle.SetIsOnWithoutNotify(settings.reduceMotion);
            reduceMotionToggle.onValueChanged.AddListener(OnReduceMotion);
        }
        if (musicSlider != null) {
            musicSlider.SetValueWithoutNotify(settings.musicVolume);
            musicSlider.onValueChanged.AddListener(OnMusicVolume);
        }
        if (sfxSlider != null) {
            sfxSlider.SetValueWithoutNotify(settings.sfxVolume);
            sfxSlider.onValueChanged.AddListener(OnSfxVolume);
        }
        if (backBtn != null) backBtn.onClick.AddListener(OnBack);
    }

    void OnDisable() {
        if (soundToggle        != null) soundToggle.onValueChanged.RemoveListener(OnSound);
        if (hapticsToggle      != null) hapticsToggle.onValueChanged.RemoveListener(OnHaptics);
        if (reduceMotionToggle != null) reduceMotionToggle.onValueChanged.RemoveListener(OnReduceMotion);
        if (musicSlider        != null) musicSlider.onValueChanged.RemoveListener(OnMusicVolume);
        if (sfxSlider          != null) sfxSlider.onValueChanged.RemoveListener(OnSfxVolume);
        if (backBtn            != null) backBtn.onClick.RemoveListener(OnBack);
    }

    void OnSound(bool v)        { settings.soundEnabled   = v; ServiceLocator.Sound?.SetEnabled(v); Persist(); }
    void OnHaptics(bool v)      { settings.hapticsEnabled = v; HapticManager.Enabled = v; Persist(); }
    void OnReduceMotion(bool v) { settings.reduceMotion   = v; Persist(); }
    void OnMusicVolume(float v) { settings.musicVolume    = v; ServiceLocator.Music?.SetVolume(v); Persist(); }
    void OnSfxVolume(float v)   { settings.sfxVolume      = v; Persist(); }

    void Persist() => ServiceLocator.Settings?.Save(settings);

    void OnBack() {
        ServiceLocator.Sound?.Play(SoundEvent.BUTTON);
        PanelRouter.Show("Home");
    }
}
