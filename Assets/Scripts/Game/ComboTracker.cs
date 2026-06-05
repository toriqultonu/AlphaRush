using System;

public class ComboTracker {
    readonly long windowMs;
    readonly Func<long> clock;
    long lastFindAt;
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    public ComboTracker(long windowMs = 3500, Func<long> clock = null) {
        this.windowMs = windowMs;
        this.clock = clock ?? (() => DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }

    public int OnWordFound() {
        long now = clock();
        Combo = (lastFindAt != 0 && now - lastFindAt <= windowMs) ? Combo + 1 : 1;
        lastFindAt = now;
        if (Combo > MaxCombo) MaxCombo = Combo;
        return Combo;
    }

    public void Reset() { Combo = 0; lastFindAt = 0; }
}
