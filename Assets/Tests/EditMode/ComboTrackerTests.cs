using NUnit.Framework;

public class ComboTrackerTests {
    [Test]
    public void ConsecutiveFindsWithinWindowIncrementCombo() {
        long now = 1000;
        var tracker = new ComboTracker(windowMs: 3500, clock: () => now);

        Assert.AreEqual(1, tracker.OnWordFound());
        now += 500;
        Assert.AreEqual(2, tracker.OnWordFound());
        now += 1000;
        Assert.AreEqual(3, tracker.OnWordFound());
    }

    [Test]
    public void ComboResetsAfterWindowExpires() {
        long now = 1000;
        var tracker = new ComboTracker(windowMs: 3500, clock: () => now);

        tracker.OnWordFound();
        now += 1000;
        tracker.OnWordFound();
        Assert.AreEqual(2, tracker.Combo);

        now += 4000; // gap > windowMs
        Assert.AreEqual(1, tracker.OnWordFound());
    }

    [Test]
    public void MaxComboTracksHighWaterMark() {
        long now = 1000;
        var tracker = new ComboTracker(windowMs: 3500, clock: () => now);

        tracker.OnWordFound();
        now += 500; tracker.OnWordFound();
        now += 500; tracker.OnWordFound();
        now += 500; tracker.OnWordFound();
        Assert.AreEqual(4, tracker.MaxCombo);

        now += 10000; // window expires, chain breaks
        tracker.OnWordFound();
        Assert.AreEqual(1, tracker.Combo);
        Assert.AreEqual(4, tracker.MaxCombo);

        now += 500; tracker.OnWordFound();
        now += 500; tracker.OnWordFound();
        Assert.AreEqual(3, tracker.Combo);
        Assert.AreEqual(4, tracker.MaxCombo);
    }

    [Test]
    public void ResetClearsComboButPreservesMaxCombo() {
        long now = 1000;
        var tracker = new ComboTracker(windowMs: 3500, clock: () => now);

        tracker.OnWordFound();
        now += 500; tracker.OnWordFound();
        now += 500; tracker.OnWordFound();
        Assert.AreEqual(3, tracker.MaxCombo);

        tracker.Reset();
        Assert.AreEqual(0, tracker.Combo);
        Assert.AreEqual(3, tracker.MaxCombo);

        now += 100;
        Assert.AreEqual(1, tracker.OnWordFound());
    }
}
