using NUnit.Framework;

public class SelectionEngineTests {
    [Test]
    public void HorizontalIsValid() {
        var a = new CellSelection { row = 3, col = 1 };
        var b = new CellSelection { row = 3, col = 7 };
        Assert.IsTrue(SelectionEngine.IsValidSelection(a, b));
    }

    [Test]
    public void DiagonalIsValid() {
        var a = new CellSelection { row = 0, col = 0 };
        var b = new CellSelection { row = 4, col = 4 };
        Assert.IsTrue(SelectionEngine.IsValidSelection(a, b));
    }

    [Test]
    public void OffAxisIsInvalid() {
        var a = new CellSelection { row = 0, col = 0 };
        var b = new CellSelection { row = 2, col = 5 };
        Assert.IsFalse(SelectionEngine.IsValidSelection(a, b));
    }
}
