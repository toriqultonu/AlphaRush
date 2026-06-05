using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField] AudioClip[] clips; // index matches SoundEvent enum order
    AudioSource[] pool;
    int next;

    // RENAMED: Changed 'enabled' to 'isAudioEnabled' to fix CS0108 warning
    bool isAudioEnabled = true;

    void Awake()
    {
        pool = new AudioSource[AppConfig.MaxSoundStreams];
        for (int i = 0; i < pool.Length; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            pool[i] = src;
        }
    }

    // UPDATED: Now references the new variable name
    public void SetEnabled(bool v) => isAudioEnabled = v;

    public void Play(SoundEvent e, float pitch = 1f, float volume = 1f)
    {
        if (!isAudioEnabled) return;

        int idx = (int)e;
        if (clips == null || idx < 0 || idx >= clips.Length) return;
        var clip = clips[idx];
        if (clip == null) return;

        var src = pool[next];
        next = (next + 1) % pool.Length;
        src.clip = clip;
        src.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        src.volume = Mathf.Clamp01(volume);
        src.Play();
    }
}