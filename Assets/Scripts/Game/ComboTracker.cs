public class ComboTracker {
    readonly long windowMs;
    long lastFindAt;
    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    public ComboTracker(long windowMs = 3500) { this.windowMs = windowMs; }

    public int OnWordFound() {
        long now = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Combo = (now - lastFindAt <= windowMs) ? Combo + 1 : 1;
        lastFindAt = now;
        if (Combo > MaxCombo) MaxCombo = Combo;
        return Combo;
    }

    public void Reset() { Combo = 0; lastFindAt = 0; }
}
