using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour {
    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField] AudioClip[] clips; // index matches SoundEvent enum order
    AudioSource[] pool;
    int next;
    bool enabled = true;

    void Awake() {
        pool = new AudioSource[AppConfig.MaxSoundStreams];
        for (int i = 0; i < pool.Length; i++) {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            pool[i] = src;
        }
    }

    public void SetEnabled(bool v) => enabled = v;

    public void Play(SoundEvent e, float pitch = 1f, float volume = 1f) {
        if (!enabled) return;
        var clip = clips[(int)e];
        if (clip == null) return;
        var src = pool[next];
        next = (next + 1) % pool.Length;
        src.clip = clip;
        src.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        src.volume = Mathf.Clamp01(volume);
        src.Play();
    }
}
