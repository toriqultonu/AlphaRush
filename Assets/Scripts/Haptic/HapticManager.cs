using UnityEngine;

public static class HapticManager {
#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject vibrator;
    static void EnsureInit() {
        if (vibrator != null) return;
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
    }
#endif

    public static bool Enabled { get; set; } = true;

    public static void Tick()    => Vibrate(8);
    public static void Light()   => Vibrate(12);
    public static void Success() => VibratePattern(new long[] { 0, 20, 40, 30 });
    public static void Win()     => VibratePattern(new long[] { 0, 40, 80, 40, 80, 80 });
    public static void Lose()    => VibratePattern(new long[] { 0, 60, 60, 60 });

    public static void Combo(int level) {
        long[] pattern = level switch {
            1 => new long[] { 0, 15 },
            2 => new long[] { 0, 15, 30, 25 },
            3 => new long[] { 0, 20, 30, 30, 30, 35 },
            4 => new long[] { 0, 25, 30, 35, 30, 40, 30, 45 },
            _ => new long[] { 0, 30, 25, 40, 25, 50, 25, 60, 25, 70 }
        };
        VibratePattern(pattern);
    }

    static void Vibrate(long ms) {
        if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        EnsureInit();
        vibrator?.Call("vibrate", ms);
#endif
    }

    static void VibratePattern(long[] pattern) {
        if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        EnsureInit();
        vibrator?.Call("vibrate", pattern, -1);
#endif
    }
}
