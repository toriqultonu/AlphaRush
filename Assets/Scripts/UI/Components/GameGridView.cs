using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameGridView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] TileView tilePrefab;
    [SerializeField] RectTransform lineLayer;
    [SerializeField] UILineRenderer lineRenderer;

    TileView[,] tiles;
    int size;
    CellSelection startCell, currentCell;
    bool dragging;

    public System.Action<List<CellSelection>> OnSelectionComplete;

    public void Build(char[,] chars) {
        size = chars.GetLength(0);
        // Compute cell size from rect
        var rt = (RectTransform)transform;
        float minSide = Mathf.Min(rt.rect.width, rt.rect.height) - 16f;
        grid.cellSize = new Vector2(minSide / size, minSide / size);
        grid.constraintCount = size;

        foreach (Transform c in grid.transform) Destroy(c.gameObject);
        tiles = new TileView[size, size];
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++) {
                var t = Instantiate(tilePrefab, grid.transform);
                t.Set(chars[r, c], r, c);
                tiles[r, c] = t;
            }
    }

    public void OnPointerDown(PointerEventData e) {
        if (!TryHitTile(e, out var cell)) return;
        dragging = true;
        startCell = cell; currentCell = cell;
        ApplySelectionVisuals();
    }

    public void OnDrag(PointerEventData e) {
        if (!dragging) return;
        if (!TryHitTile(e, out var cell)) return;
        var candidate = cell;
        if (SelectionEngine.IsValidSelection(startCell, candidate)) {
            currentCell = candidate;
            ApplySelectionVisuals();
        }
    }

    public void OnPointerUp(PointerEventData e) {
        if (!dragging) return;
        dragging = false;
        var cells = SelectionEngine.GetCellsBetween(startCell, currentCell);
        ClearSelectionVisuals();
        OnSelectionComplete?.Invoke(cells);
    }

    bool TryHitTile(PointerEventData e, out CellSelection cell) {
        cell = null;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(e, results);
        foreach (var r in results) {
            var tv = r.gameObject.GetComponentInParent<TileView>();
            if (tv != null) { cell = new CellSelection { row = tv.Row, col = tv.Col }; return true; }
        }
        return false;
    }

    void ApplySelectionVisuals() {
        ClearSelectionVisuals();
        foreach (var c in SelectionEngine.GetCellsBetween(startCell, currentCell))
            tiles[c.row, c.col].SetSelected(true);
        // Update line renderer with positions of startCell + currentCell tile centers
    }

    void ClearSelectionVisuals() {
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                tiles[r, c].SetSelected(false);
    }

    public TileView Tile(int r, int c) => tiles[r, c];
}
