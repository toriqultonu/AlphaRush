using System.Collections.Generic;

public static class SelectionEngine {
    public static bool IsValidSelection(CellSelection a, CellSelection b) {
        int dr = b.row - a.row, dc = b.col - a.col;
        if (dr == 0 && dc == 0) return true;
        if (dr == 0 || dc == 0) return true;
        return System.Math.Abs(dr) == System.Math.Abs(dc);
    }

    public static List<CellSelection> GetCellsBetween(CellSelection a, CellSelection b) {
        var list = new List<CellSelection>();
        int dr = System.Math.Sign(b.row - a.row);
        int dc = System.Math.Sign(b.col - a.col);
        int steps = System.Math.Max(System.Math.Abs(b.row - a.row), System.Math.Abs(b.col - a.col));
        for (int i = 0; i <= steps; i++)
            list.Add(new CellSelection { row = a.row + dr * i, col = a.col + dc * i });
        return list;
    }
}
