using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

// Two AudioSource (A/B) crossfade via DOTween. 400 ms default fade. One active at a time.
public class MusicManager : MonoBehaviour {
    const float CrossfadeSec = 0.4f;

    [SerializeField] AudioMixerGroup musicGroup;
    AudioSource a, b;
    AudioSource active;
    float targetVolume = 0.6f;
    bool isMusicEnabled = true;

    void Awake() {
        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        foreach (var src in new[] { a, b }) {
            src.outputAudioMixerGroup = musicGroup;
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
        }
        active = a;
    }

    public void SetEnabled(bool v) {
        isMusicEnabled = v;
        if (!isMusicEnabled) Stop();
    }

    public void SetVolume(float v) {
        targetVolume = Mathf.Clamp01(v);
        if (isMusicEnabled && active != null && active.isPlaying)
            active.DOFade(targetVolume, 0.2f);
    }

    public void PlayMusic(AudioClip clip) {
        if (clip == null) return;
        if (!isMusicEnabled) return;
        if (active != null && active.clip == clip && active.isPlaying) return;

        AudioSource next = (active == a) ? b : a;
        next.clip = clip;
        next.volume = 0f;
        next.Play();
        next.DOFade(targetVolume, CrossfadeSec);

        var outgoing = active;
        if (outgoing != null && outgoing.isPlaying)
            outgoing.DOFade(0f, CrossfadeSec).OnComplete(() => outgoing.Stop());

        active = next;
    }

    public void Stop() {
        if (active == null || !active.isPlaying) return;
        var outgoing = active;
        outgoing.DOFade(0f, CrossfadeSec).OnComplete(() => outgoing.Stop());
    }
}
