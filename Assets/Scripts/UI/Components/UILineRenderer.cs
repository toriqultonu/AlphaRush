using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Placeholder. Replace with Unity-UI-Extensions UILineRenderer or a real Graphic-derived line
// implementation during §11 polish pass. Method signatures here keep GameGridView compiling.
[RequireComponent(typeof(RectTransform))]
public class UILineRenderer : Graphic {
    public List<Vector2> Points = new();
    public float Thickness = 6f;

    public void SetPoints(IEnumerable<Vector2> pts) {
        Points.Clear();
        Points.AddRange(pts);
        SetVerticesDirty();
    }

    public void Clear() {
        Points.Clear();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();
        // No-op stub. Real implementation pending.
    }
}
