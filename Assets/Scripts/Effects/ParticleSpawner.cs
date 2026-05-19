using UnityEngine;
using DG.Tweening;

public class ParticleSpawner : MonoBehaviour {
    [SerializeField] ParticleSystem confettiPrefab;
    [SerializeField] ParticleSystem starBurstPrefab;
    static int active = 0;

    public void SpawnStarBurst(Vector3 worldPos, Color tint) {
        if (active > AppConfig.MaxActiveParticles) return;
        var ps = Instantiate(starBurstPrefab, worldPos, Quaternion.identity);
        var main = ps.main; main.startColor = tint;
        ps.Play();
        active += (int)ps.emission.GetBurst(0).count.constant;
        Destroy(ps.gameObject, 1.5f);
        DOVirtual.DelayedCall(1.5f, () => active -= 24);
    }

    public void SpawnConfetti(Vector3 origin) {
        var ps = Instantiate(confettiPrefab, origin, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, 3.5f);
    }
}
