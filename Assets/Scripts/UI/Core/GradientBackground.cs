using UnityEngine;
using UnityEngine.UI;

// Cheap two-stop vertical gradient via a 1×2 procedural texture stretched by the
// Image. Bilinear filter (not point) so the stretch produces a smooth gradient
// rather than two hard bands. Defaults to AppColors.BackgroundLight → BackgroundMedium.
[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class GradientBackground : MonoBehaviour {
    [SerializeField] Color topColor;
    [SerializeField] Color bottomColor;

    Image image;
    Texture2D tex;
    Sprite sprite;

    void Reset() {
        topColor    = AppColors.BackgroundLight;
        bottomColor = AppColors.BackgroundMedium;
    }

    void Awake() {
        if (topColor == default && bottomColor == default) {
            topColor    = AppColors.BackgroundLight;
            bottomColor = AppColors.BackgroundMedium;
        }
        Build();
    }

    public void SetColors(Color top, Color bottom) {
        topColor = top;
        bottomColor = bottom;
        Build();
    }

    void Build() {
        if (image == null) image = GetComponent<Image>();
        if (image == null) return;

        if (tex == null) {
            tex = new Texture2D(1, 2, TextureFormat.RGBA32, false) {
                filterMode = FilterMode.Bilinear,
                wrapMode   = TextureWrapMode.Clamp,
                hideFlags  = HideFlags.DontSave
            };
        }
        tex.SetPixel(0, 0, bottomColor); // v=0 → bottom of UV
        tex.SetPixel(0, 1, topColor);    // v=1 → top of UV
        tex.Apply(false, false);

        if (sprite == null) {
            sprite = Sprite.Create(tex, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.DontSave;
            image.sprite = sprite;
        }
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = Color.white;
        image.SetMaterialDirty();
    }

    void OnDestroy() {
        if (sprite != null) DestroyImmediate(sprite);
        if (tex    != null) DestroyImmediate(tex);
    }
}
