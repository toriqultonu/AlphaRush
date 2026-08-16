using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// One-shot scene/prefab generator for AlphaRush.
// Run via  AlphaRush → Build All (Scenes + Prefabs)  or headless:
//   Unity -batchmode -projectPath <path> -executeMethod AlphaRushSceneBuilder.BuildAll -quit
//
// Styling follows docs/new_demo.jpeg (candy theme): peach sparkle background,
// glossy pink banners, biscuit letter tiles with brown letters, maroon board
// with magenta frame, candy-pill word chips, white cards.
public static class AlphaRushSceneBuilder {

    // ── candy palette (mirrors AppColors, editor-side literals) ────────────
    static readonly Color PinkDeep  = Hex("#E2447E");
    static readonly Color Pink      = Hex("#F782B4");
    static readonly Color PinkSoft  = Hex("#FFEFF6");
    static readonly Color Maroon    = Hex("#5C2438");
    static readonly Color Biscuit   = Hex("#F7E8C9");
    static readonly Color Brown     = Hex("#5C3A21");
    static readonly Color CardWhite = Color.white;
    static readonly Color Teal      = Hex("#35C3C1");
    static readonly Color Purple    = Hex("#C05BD4");
    static readonly Color Blue      = Hex("#4FA8F5");
    static readonly Color Green     = Hex("#7DC942");
    static readonly Color Gold      = Hex("#FFC93C");
    static readonly Color Danger    = Hex("#E2536A");
    static readonly Color BgTop     = Hex("#FDE8C3");
    static readonly Color BgBottom  = Hex("#F6D19B");

    static Sprite SprUI, SprBg, SprKnob, SprCheck;

    [MenuItem("AlphaRush/Build All (Scenes + Prefabs)")]
    public static void BuildAll() {
        AssetDatabase.Refresh();
        LoadSprites();
        EnsureFolders();

        // Prefabs first — scenes reference them.
        var tilePrefab  = BuildTilePrefab();
        var chipPrefab  = BuildWordChipPrefab();
        var cardPrefab  = BuildTopicCardPrefab();
        var dotPrefab   = BuildLevelDotPrefab();
        var badgePrefab = BuildBadgeCellPrefab();

        BuildMainScene(tilePrefab, chipPrefab, cardPrefab, dotPrefab, badgePrefab);
        BuildBootstrapScene();

        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/Scenes/Bootstrap.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
        };

        AssetDatabase.SaveAssets();
        Debug.Log("[AlphaRushSceneBuilder] Build complete — Bootstrap + Main scenes, all prefabs, build settings.");
    }

    // ═══════════════════════════ helpers ═══════════════════════════════════

    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

    static void LoadSprites() {
        SprUI    = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        SprBg    = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        SprKnob  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        SprCheck = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
    }

    static void EnsureFolders() {
        foreach (var p in new[] { "Assets/Prefabs", "Assets/Prefabs/Tiles", "Assets/Prefabs/UI", "Assets/Scenes" }) {
            if (!AssetDatabase.IsValidFolder(p)) {
                var parent = System.IO.Path.GetDirectoryName(p).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(p));
            }
        }
    }

    static GameObject NewUI(string name, Transform parent) {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; // UI
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    static RectTransform Rt(GameObject go) => (RectTransform)go.transform;

    static void Stretch(RectTransform rt, float left = 0, float right = 0, float top = 0, float bottom = 0) {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    // Anchor min==max point placement.
    static void Place(RectTransform rt, float ax, float ay, float px, float py, float x, float y, float w, float h) {
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(px, py);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    static Image AddImg(GameObject go, Color c, Sprite s, bool raycast = false, bool sliced = true) {
        var img = go.AddComponent<Image>();
        img.sprite = s;
        img.color = c;
        img.raycastTarget = raycast;
        if (s != null && sliced && s.border.sqrMagnitude > 0) img.type = Image.Type.Sliced;
        return img;
    }

    static TextMeshProUGUI AddText(Transform parent, string name, string text, float size, Color color,
                                   FontStyles style = FontStyles.Bold,
                                   TextAlignmentOptions align = TextAlignmentOptions.Center) {
        var go = NewUI(name, parent);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    static Button MakeButton(Transform parent, string name, string label, Color bg, Color labelCol, float fontSize = 36) {
        var go = NewUI(name, parent);
        var img = AddImg(go, bg, SprUI, raycast: true);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        btn.colors = colors;
        go.AddComponent<ButtonBouncer>(); // candy squash-and-bounce
        var t = AddText(go.transform, "Label", label, fontSize, labelCol);
        Stretch(Rt(t.gameObject));
        return btn;
    }

    // Bordered card: outer frame color + inset white (demo's rounded frames).
    static GameObject FramedCard(Transform parent, string name, Color frame, Color inner, float inset = 10f) {
        var outer = NewUI(name, parent);
        AddImg(outer, frame, SprUI);
        var innerGo = NewUI("Inner", outer.transform);
        Stretch(Rt(innerGo), inset, inset, inset, inset);
        AddImg(innerGo, inner, SprUI);
        return outer;
    }

    static void SetRef(Component comp, string field, Object value) {
        var so = new SerializedObject(comp);
        var p = so.FindProperty(field);
        if (p == null) { Debug.LogError($"[Builder] no field '{field}' on {comp.GetType().Name}"); return; }
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetRefs(Component comp, params (string field, Object value)[] pairs) {
        var so = new SerializedObject(comp);
        foreach (var (field, value) in pairs) {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError($"[Builder] no field '{field}' on {comp.GetType().Name}"); continue; }
            p.objectReferenceValue = value;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetObjArray(Component comp, string field, Object[] values) {
        var so = new SerializedObject(comp);
        var p = so.FindProperty(field);
        if (p == null) { Debug.LogError($"[Builder] no array field '{field}' on {comp.GetType().Name}"); return; }
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetStrArray(Component comp, string field, string[] values) {
        var so = new SerializedObject(comp);
        var p = so.FindProperty(field);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            p.GetArrayElementAtIndex(i).stringValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetFloat(Component comp, string field, float value) {
        var so = new SerializedObject(comp);
        var p = so.FindProperty(field);
        if (p != null) { p.floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    static GameObject SavePrefab(GameObject go, string path) {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // Loads a generated icon PNG as a Sprite, forcing sprite import if needed.
    static Sprite LoadIcon(string path) {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null && AssetImporter.GetAtPath(path) is TextureImporter ti) {
            ti.textureType = TextureImporterType.Sprite;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        if (sprite == null) Debug.LogError($"[Builder] icon not importable as sprite: {path}");
        return sprite;
    }

    // Rounded candy button with a centered white icon instead of a text label.
    static Button MakeIconButton(Transform parent, string name, string iconPath, Color bg) {
        var go = NewUI(name, parent);
        var img = AddImg(go, bg, SprUI, raycast: true);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        btn.colors = colors;
        go.AddComponent<ButtonBouncer>();

        var iconGo = NewUI("Icon", go.transform);
        var irt = Rt(iconGo);
        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
        irt.pivot = new Vector2(0.5f, 0.5f);
        irt.anchoredPosition = Vector2.zero;
        irt.sizeDelta = new Vector2(58, 58);
        var icon = iconGo.AddComponent<Image>();
        icon.sprite = LoadIcon(iconPath);
        icon.color = Color.white;
        icon.raycastTarget = false;
        icon.preserveAspect = true;
        return btn;
    }

    // ═══════════════════════════ prefabs ═══════════════════════════════════

    static GameObject BuildTilePrefab() {
        var root = NewUI("TileView", null);
        Rt(root).sizeDelta = new Vector2(100, 100);
        var bg = AddImg(root, Biscuit, SprUI, raycast: true);

        var hi = NewUI("Highlight", root.transform);
        Stretch(Rt(hi), -4, -4, -4, -4);
        var hiImg = AddImg(hi, new Color(1f, 0.62f, 0.8f, 0f), SprUI); // pink jelly glow

        var fo = NewUI("FoundOverlay", root.transform);
        Stretch(Rt(fo));
        var foImg = AddImg(fo, new Color(1, 1, 1, 0f), SprUI);

        var letter = AddText(root.transform, "Letter", "A", 60, Brown);
        Stretch(Rt(letter.gameObject), 4, 4, 4, 4);
        letter.enableAutoSizing = true;
        letter.fontSizeMin = 10; letter.fontSizeMax = 72;

        var view = root.AddComponent<TileView>();
        SetRefs(view, ("background", bg), ("highlight", hiImg), ("foundOverlay", foImg), ("letter", letter));
        return SavePrefab(root, "Assets/Prefabs/Tiles/TileView.prefab");
    }

    static GameObject BuildWordChipPrefab() {
        var root = NewUI("WordChip", null);
        Rt(root).sizeDelta = new Vector2(320, 70);
        var bg = AddImg(root, Teal, SprUI); // runtime cycles AppColors.ChipColors

        var label = AddText(root.transform, "Label", "WORD", 30, Brown);
        Stretch(Rt(label.gameObject), 8, 8, 4, 4);
        label.enableAutoSizing = true;
        label.fontSizeMin = 12; label.fontSizeMax = 32;

        var view = root.AddComponent<WordChipView>();
        SetRefs(view, ("label", label), ("background", bg));
        return SavePrefab(root, "Assets/Prefabs/UI/WordChip.prefab");
    }

    static GameObject BuildTopicCardPrefab() {
        var root = NewUI("TopicCard", null);
        Rt(root).sizeDelta = new Vector2(480, 360);
        var bg = AddImg(root, CardWhite, SprUI, raycast: true);
        var btn = root.AddComponent<Button>();
        btn.targetGraphic = bg;

        var stripe = NewUI("AccentStripe", root.transform);
        var stripeRt = Rt(stripe);
        stripeRt.anchorMin = new Vector2(0, 1); stripeRt.anchorMax = Vector2.one;
        stripeRt.pivot = new Vector2(0.5f, 1);
        stripeRt.offsetMin = new Vector2(14, -28); stripeRt.offsetMax = new Vector2(-14, -12);
        var stripeImg = AddImg(stripe, Pink, SprUI);

        var icon = AddText(root.transform, "Icon", "?", 72, PinkDeep);
        Place(Rt(icon.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -36, 400, 110);

        // Sprite icon slot — TopicCardView prefers this over the text glyph
        // (loads Resources/TopicIcons/<topicId> at bind time).
        var iconImgGo = NewUI("IconImage", root.transform);
        Place(Rt(iconImgGo), 0.5f, 1f, 0.5f, 1f, 0, -38, 104, 104);
        var iconImg = iconImgGo.AddComponent<Image>();
        iconImg.raycastTarget = false;
        iconImg.preserveAspect = true;
        iconImgGo.SetActive(false);

        var title = AddText(root.transform, "Title", "Topic", 36, Brown);
        Place(Rt(title.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -152, 440, 60);

        var stats = AddText(root.transform, "Stats", "0 ★", 28, PinkDeep);
        Place(Rt(stats.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 26, 440, 50);

        var lockOverlay = NewUI("LockOverlay", root.transform);
        Stretch(Rt(lockOverlay));
        AddImg(lockOverlay, new Color(Maroon.r, Maroon.g, Maroon.b, 0.78f), SprUI, raycast: true);
        var lockText = AddText(lockOverlay.transform, "LockText", "Need 5 ★", 26, Color.white);
        Stretch(Rt(lockText.gameObject), 20, 20, 20, 20);

        var view = root.AddComponent<TopicCardView>();
        SetRefs(view,
            ("titleText", title), ("iconText", icon), ("statsText", stats), ("lockText", lockText),
            ("accentStripe", stripeImg), ("iconImage", iconImg),
            ("lockOverlay", lockOverlay), ("pressArea", btn));
        return SavePrefab(root, "Assets/Prefabs/UI/TopicCard.prefab");
    }

    static GameObject BuildLevelDotPrefab() {
        var root = NewUI("LevelDot", null);
        Rt(root).sizeDelta = new Vector2(280, 280);
        var bg = AddImg(root, Pink, SprKnob, sliced: false, raycast: true);
        bg.preserveAspect = true;
        var btn = root.AddComponent<Button>();
        btn.targetGraphic = bg;

        var num = AddText(root.transform, "Number", "1", 64, Color.white);
        Place(Rt(num.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, 18, 200, 90);

        // Three little gold gems (rotated squares) along the bottom of the dot.
        var stars = new GameObject[3];
        for (int i = 0; i < 3; i++) {
            var star = NewUI($"Star{i + 1}", root.transform);
            Place(Rt(star), 0.5f, 0.5f, 0.5f, 0.5f, (i - 1) * 44, -62, 30, 30);
            star.transform.localRotation = Quaternion.Euler(0, 0, 45);
            AddImg(star, Gold, SprUI);
            star.SetActive(false);
            stars[i] = star;
        }

        // Completed badge — green circle at top-right.
        var ring = NewUI("CheckRing", root.transform);
        Place(Rt(ring), 1f, 1f, 0.5f, 0.5f, -34, -34, 56, 56);
        AddImg(ring, Green, SprKnob, sliced: false);
        ring.SetActive(false);

        var lockIcon = NewUI("LockShade", root.transform);
        Stretch(Rt(lockIcon));
        var lockImg = AddImg(lockIcon, new Color(Maroon.r, Maroon.g, Maroon.b, 0.72f), SprKnob, sliced: false);
        lockImg.preserveAspect = true;
        lockIcon.SetActive(false);

        var view = root.AddComponent<LevelDotView>();
        SetRefs(view, ("numberLabel", num), ("lockIcon", lockIcon), ("checkRing", ring), ("pressArea", btn));
        SetObjArray(view, "starIcons", stars);
        return SavePrefab(root, "Assets/Prefabs/UI/LevelDot.prefab");
    }

    static GameObject BuildBadgeCellPrefab() {
        var root = NewUI("BadgeCell", null);
        Rt(root).sizeDelta = new Vector2(220, 90);
        AddImg(root, Purple, SprUI);
        var label = AddText(root.transform, "Label", "badge", 24, Color.white);
        Stretch(Rt(label.gameObject), 8, 8, 6, 6);
        label.enableAutoSizing = true; label.fontSizeMin = 12; label.fontSizeMax = 26;
        return SavePrefab(root, "Assets/Prefabs/UI/BadgeCell.prefab");
    }

    // ═══════════════════════════ Main scene ════════════════════════════════

    static void BuildMainScene(GameObject tilePrefab, GameObject chipPrefab, GameObject cardPrefab,
                               GameObject dotPrefab, GameObject badgePrefab) {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera (UGUI overlay does the visuals; camera just clears to peach).
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgTop;
        camGo.AddComponent<AudioListener>();
        camGo.tag = "MainCamera";

        // Event system (new Input System module).
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // Canvas.
        var canvasGo = NewUI("UICanvas", null);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // Match WIDTH: canvas is always exactly 1080 units wide, so nothing
        // clips horizontally on tall 20:9 phones — extra height just adds air.
        scaler.matchWidthOrHeight = 0f;
        canvasGo.AddComponent<GraphicRaycaster>();
        var router = canvasGo.AddComponent<PanelRouter>();

        // SafeArea.
        var safe = NewUI("SafeArea", canvasGo.transform);
        Stretch(Rt(safe));
        safe.AddComponent<SafeAreaFitter>();

        // Background gradient.
        var bgGo = NewUI("BackgroundGradient", safe.transform);
        Stretch(Rt(bgGo));
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.raycastTarget = false;
        var grad = bgGo.AddComponent<GradientBackground>();
        var gradSo = new SerializedObject(grad);
        gradSo.FindProperty("topColor").colorValue = BgTop;
        gradSo.FindProperty("bottomColor").colorValue = BgBottom;
        gradSo.ApplyModifiedPropertiesWithoutUndo();

        AddSparkles(safe.transform);

        // Panels.
        var pSplash   = Panel(safe, "Panel_Splash", true);
        var pHome     = Panel(safe, "Panel_Home", false);
        var pTopics   = Panel(safe, "Panel_TopicList", false);
        var pLevels   = Panel(safe, "Panel_LevelSelect", false);
        var pGame     = Panel(safe, "Panel_Game", false);
        var pDaily    = Panel(safe, "Panel_DailyChallenge", false);
        var pProfile  = Panel(safe, "Panel_Profile", false);
        var pSettings = Panel(safe, "Panel_Settings", false);
        var pComplete = Panel(safe, "Panel_LevelComplete", false);

        BuildSplash(pSplash);
        BuildHome(pHome);
        BuildTopicList(pTopics, cardPrefab);
        BuildLevelSelect(pLevels, dotPrefab);
        BuildGame(pGame, tilePrefab, chipPrefab);
        BuildDaily(pDaily);
        BuildProfile(pProfile, badgePrefab);
        BuildSettings(pSettings);
        BuildLevelComplete(pComplete);

        SetObjArray(router, "mainPanels",
            new Object[] { pSplash, pHome, pTopics, pLevels, pGame, pDaily, pProfile, pSettings, pComplete });
        SetStrArray(router, "panelNames",
            new[] { Routes.Splash, Routes.Home, Routes.TopicList, Routes.LevelSelect, Routes.Game,
                    Routes.Daily, Routes.Profile, Routes.Settings, Routes.LevelComplete });

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
    }

    // Scattered twinkling sparkle dots over the background (candy demo look).
    static void AddSparkles(Transform safe) {
        var holder = NewUI("Sparkles", safe);
        Stretch(Rt(holder));
        // (x, y, size, duration, delay) in normalized anchor space.
        (float x, float y, float s, float dur, float del)[] spots = {
            (0.08f, 0.92f, 26, 1.7f, 0.0f), (0.90f, 0.88f, 34, 2.1f, 0.5f),
            (0.15f, 0.72f, 20, 1.5f, 0.9f), (0.84f, 0.66f, 24, 1.9f, 0.2f),
            (0.06f, 0.45f, 30, 2.3f, 1.1f), (0.93f, 0.40f, 20, 1.6f, 0.7f),
            (0.12f, 0.20f, 24, 2.0f, 0.4f), (0.88f, 0.15f, 28, 1.8f, 1.3f),
            (0.50f, 0.96f, 20, 2.2f, 0.8f), (0.45f, 0.05f, 22, 1.7f, 1.6f),
        };
        foreach (var (x, y, s, dur, del) in spots) {
            var dot = NewUI("Sparkle", holder.transform);
            var rt = Rt(dot);
            rt.anchorMin = rt.anchorMax = new Vector2(x, y);
            rt.sizeDelta = new Vector2(s, s);
            AddImg(dot, new Color(1f, 1f, 1f, 0f), SprKnob, sliced: false);
            var tw = dot.AddComponent<SparkleTwinkle>();
            SetFloat(tw, "duration", dur);
            SetFloat(tw, "delay", del);
        }
    }

    static GameObject Panel(GameObject safe, string name, bool active) {
        var p = NewUI(name, safe.transform);
        Stretch(Rt(p));
        p.AddComponent<CanvasGroup>(); // PanelRouter fades this on entrance
        p.SetActive(active);
        return p;
    }

    // Candy banner across the top: deep-pink frame, pink face, white title.
    static TextMeshProUGUI Header(GameObject panel, string title) {
        var banner = FramedCard(panel.transform, "HeaderBanner", PinkDeep, Pink, 10f);
        var rt = Rt(banner);
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(32, -150); rt.offsetMax = new Vector2(-32, -24);
        var t = AddText(banner.transform, "Title", title, 56, Color.white);
        Stretch(Rt(t.gameObject), 120, 120, 8, 8);
        t.enableAutoSizing = true; t.fontSizeMin = 24; t.fontSizeMax = 60;
        return t;
    }

    static Button BackButton(GameObject panel) {
        var b = MakeButton(panel.transform, "BackBtn", "<", PinkDeep, Color.white, 48);
        Place(Rt(b.gameObject), 0, 1, 0, 1, 36, -32, 100, 106);
        return b;
    }

    // ── Splash ──────────────────────────────────────────────────────────────
    static void BuildSplash(GameObject panel) {
        var cg = panel.GetComponent<CanvasGroup>();

        var logoRoot = NewUI("LogoRoot", panel.transform);
        Place(Rt(logoRoot), 0.5f, 0.5f, 0.5f, 0.5f, 0, 80, 900, 420);

        var plaque = FramedCard(logoRoot.transform, "Plaque", PinkDeep, Pink, 12f);
        Stretch(Rt(plaque), 0, 0, 60, 140);

        var title = AddText(plaque.transform, "Title", "AlphaRush", 110, Color.white);
        Stretch(Rt(title.gameObject), 20, 20, 10, 10);
        title.enableAutoSizing = true; title.fontSizeMin = 40; title.fontSizeMax = 120;

        var tagline = AddText(logoRoot.transform, "Tagline", "Find words. Beat the clock.", 38, Brown);
        Place(Rt(tagline.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 40, 860, 70);

        var view = panel.AddComponent<SplashView>();
        SetRefs(view, ("canvasGroup", cg), ("logoRoot", Rt(logoRoot.gameObject)), ("title", title));
    }

    // ── Home ────────────────────────────────────────────────────────────────
    static void BuildHome(GameObject panel) {
        Header(panel, "AlphaRush");

        var statsCard = FramedCard(panel.transform, "StatsCard", Pink, CardWhite, 8f);
        Place(Rt(statsCard), 0.5f, 1f, 0.5f, 1f, 0, -210, 980, 400);
        var inner = statsCard.transform.Find("Inner");
        var gridGo = NewUI("Grid", inner);
        Stretch(Rt(gridGo), 14, 14, 14, 14);
        var grid = gridGo.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(455, 165);
        grid.spacing = new Vector2(18, 18);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.MiddleCenter;

        StatChipView Chip(string name) {
            var chipGo = NewUI(name, gridGo.transform);
            AddImg(chipGo, PinkSoft, SprUI);
            var value = AddText(chipGo.transform, "Value", "0", 52, PinkDeep);
            Place(Rt(value.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, 22, 420, 70);
            var label = AddText(chipGo.transform, "Label", name, 28, Brown);
            Place(Rt(label.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -42, 420, 46);
            var chip = chipGo.AddComponent<StatChipView>();
            SetRefs(chip, ("labelText", label), ("valueText", value));
            return chip;
        }

        var topicsChip = Chip("Topics");
        var levelsChip = Chip("Levels");
        var starsChip  = Chip("Stars");
        var streakChip = Chip("Streak");

        var play = MakeButton(panel.transform, "PlayBtn", "PLAY", Pink, Color.white, 58);
        Place(Rt(play.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -40, 760, 170);
        var daily = MakeButton(panel.transform, "DailyBtn", "Daily Challenge", Teal, Color.white, 40);
        Place(Rt(daily.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -250, 760, 130);
        var profile = MakeButton(panel.transform, "ProfileBtn", "Profile", Purple, Color.white, 40);
        Place(Rt(profile.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -420, 760, 130);
        var settings = MakeButton(panel.transform, "SettingsBtn", "Settings", Blue, Color.white, 32);
        Place(Rt(settings.gameObject), 1f, 0f, 1f, 0f, -40, 50, 280, 100);

        var view = panel.AddComponent<HomeView>();
        SetRefs(view,
            ("topicsChip", topicsChip), ("levelsChip", levelsChip),
            ("starsChip", starsChip), ("streakChip", streakChip),
            ("playBtn", play), ("dailyBtn", daily), ("profileBtn", profile), ("settingsBtn", settings));
    }

    // Scrollable grid shell shared by TopicList/LevelSelect. Returns content root.
    static RectTransform ScrollGrid(GameObject panel, int columns, Vector2 cell, Vector2 spacing) {
        var scrollGo = NewUI("Scroll", panel.transform);
        Stretch(Rt(scrollGo), 28, 28, 174, 36);
        var scroll = scrollGo.AddComponent<ScrollRect>();

        var viewport = NewUI("Viewport", scrollGo.transform);
        Stretch(Rt(viewport));
        viewport.AddComponent<RectMask2D>();

        var content = NewUI("Content", viewport.transform);
        var crt = Rt(content);
        crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = cell;
        grid.spacing = spacing;
        grid.padding = new RectOffset(12, 12, 12, 40);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.childAlignment = TextAnchor.UpperCenter;

        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = Rt(viewport);
        scroll.content = crt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30;
        return crt;
    }

    // ── TopicList ───────────────────────────────────────────────────────────
    static void BuildTopicList(GameObject panel, GameObject cardPrefab) {
        Header(panel, "Topics");
        var back = BackButton(panel);
        var content = ScrollGrid(panel, 2, new Vector2(480, 360), new Vector2(28, 28));

        var view = panel.AddComponent<TopicListView>();
        SetRefs(view,
            ("gridContainer", content),
            ("cardPrefab", cardPrefab.GetComponent<TopicCardView>()),
            ("backBtn", back));
    }

    // ── LevelSelect ─────────────────────────────────────────────────────────
    static void BuildLevelSelect(GameObject panel, GameObject dotPrefab) {
        Header(panel, "Levels");
        var back = BackButton(panel);
        var content = ScrollGrid(panel, 3, new Vector2(310, 310), new Vector2(20, 24));

        var view = panel.AddComponent<LevelSelectView>();
        SetRefs(view,
            ("gridContainer", content),
            ("dotPrefab", dotPrefab.GetComponent<LevelDotView>()),
            ("backBtn", back));
    }

    // ── Game ────────────────────────────────────────────────────────────────
    static void BuildGame(GameObject panel, GameObject tilePrefab, GameObject chipPrefab) {
        // Header.
        var headerTopic = Header(panel, "Topic");
        var back = BackButton(panel);

        // Timer (white pill, brown text) + progress (pink pill, white text) row.
        var timerChip = FramedCard(panel.transform, "TimerChip", CardWhite, CardWhite, 0f);
        Place(Rt(timerChip), 0f, 1f, 0f, 1f, 32, -172, 300, 74);
        var timerLabel = AddText(timerChip.transform, "Time", "01:30", 38, Brown);
        Stretch(Rt(timerLabel.gameObject), 8, 8, 4, 4);
        var timerView = timerChip.AddComponent<TimerView>();
        SetRef(timerView, "label", timerLabel);

        var progChip = FramedCard(panel.transform, "ProgressChip", Pink, Pink, 0f);
        Place(Rt(progChip), 1f, 1f, 1f, 1f, -32, -172, 560, 74);
        var progress = AddText(progChip.transform, "Progress", "FOUND: 0 / 0", 32, Color.white);
        Stretch(Rt(progress.gameObject), 10, 10, 4, 4);
        // Gold star gem on the right of the progress pill.
        var starGem = NewUI("StarGem", progChip.transform);
        Place(Rt(starGem), 1f, 0.5f, 0.5f, 0.5f, -8, 0, 54, 54);
        starGem.transform.localRotation = Quaternion.Euler(0, 0, 45);
        AddImg(starGem, Gold, SprUI);

        // Word list — white card in pink frame with candy-pill chips,
        // ABOVE the board (demo layout).
        var wordCard = FramedCard(panel.transform, "WordListCard", Pink, CardWhite, 8f);
        var wrt = Rt(wordCard);
        wrt.anchorMin = new Vector2(0, 1); wrt.anchorMax = new Vector2(1, 1);
        wrt.pivot = new Vector2(0.5f, 1);
        wrt.offsetMin = new Vector2(32, -620); wrt.offsetMax = new Vector2(-32, -264);
        var wordInner = wordCard.transform.Find("Inner");
        var wlTitle = AddText(wordInner, "Title", "WORD LIST", 34, PinkDeep);
        Place(Rt(wlTitle.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -4, 500, 50);
        var chips = NewUI("Chips", wordInner);
        Stretch(Rt(chips), 12, 12, 56, 8);
        var chipGrid = chips.AddComponent<GridLayoutGroup>();
        chipGrid.cellSize = new Vector2(320, 62);
        chipGrid.spacing = new Vector2(12, 8);
        chipGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        chipGrid.constraintCount = 3;
        chipGrid.childAlignment = TextAnchor.UpperCenter;
        var wordListView = chips.AddComponent<WordListView>();
        SetRefs(wordListView, ("container", chips.transform), ("chipPrefab", chipPrefab.GetComponent<WordChipView>()));

        // Board: magenta frame around maroon backing (demo look), below the words.
        var board = FramedCard(panel.transform, "Board", PinkDeep, Maroon, 12f);
        Place(Rt(board), 0.5f, 1f, 0.5f, 1f, 0, -644, 1000, 1000);
        var boardInner = board.transform.Find("Inner");

        var gridRoot = NewUI("GridRoot", boardInner);
        Stretch(Rt(gridRoot), 12, 12, 12, 12);
        // Transparent catcher so drags between tiles still hit the grid view.
        AddImg(gridRoot, new Color(1, 1, 1, 0f), null, raycast: true, sliced: false);
        var gridLayout = gridRoot.AddComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 10;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.spacing = new Vector2(5, 5);
        var gridView = gridRoot.AddComponent<GameGridView>();
        SetRefs(gridView, ("grid", gridLayout), ("tilePrefab", tilePrefab.GetComponent<TileView>()));

        // Bottom action bar — floating rounded pill, inset from every edge.
        var bar = NewUI("ActionBar", panel.transform);
        var brt = Rt(bar);
        brt.anchorMin = Vector2.zero; brt.anchorMax = new Vector2(1, 0);
        brt.pivot = new Vector2(0.5f, 0);
        brt.offsetMin = new Vector2(32, 24); brt.offsetMax = new Vector2(-32, 148);
        AddImg(bar, PinkDeep, SprUI);
        var hint = MakeButton(bar.transform, "HintBtn", "Hint", Teal, Color.white, 34);
        Place(Rt(hint.gameObject), 0.17f, 0.5f, 0.5f, 0.5f, 0, 0, 280, 92);
        var restart = MakeButton(bar.transform, "RestartBtn", "Restart", Purple, Color.white, 34);
        Place(Rt(restart.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, 0, 280, 92);
        var pause = MakeButton(bar.transform, "PauseBtn", "Pause", Pink, Color.white, 34);
        Place(Rt(pause.gameObject), 0.83f, 0.5f, 0.5f, 0.5f, 0, 0, 280, 92);

        // Combo banner (view active, banner child hidden) — over the words/board seam.
        var comboRoot = NewUI("ComboBanner", panel.transform);
        Place(Rt(comboRoot), 0.5f, 1f, 0.5f, 1f, 0, -604, 700, 90);
        var comboView = comboRoot.AddComponent<ComboBannerView>();
        var bannerGo = NewUI("Banner", comboRoot.transform);
        Stretch(Rt(bannerGo));
        AddImg(bannerGo, PinkDeep, SprUI);
        var bannerText = AddText(bannerGo.transform, "Text", "COMBO x2!", 44, Color.white);
        Stretch(Rt(bannerText.gameObject));
        bannerGo.SetActive(false);
        SetRefs(comboView, ("banner", bannerGo), ("bannerText", bannerText));

        // Dialogs.
        var pauseDialog = BuildPauseDialog(panel);
        var resumeDialog = BuildResumeDialog(panel);
        var tutorial = BuildTutorialOverlay(panel);

        var view = panel.AddComponent<GameView>();
        SetRefs(view,
            ("gridView", gridView), ("wordList", wordListView), ("timer", timerView),
            ("headerTopic", headerTopic), ("progressLine", progress),
            ("hintBtn", hint), ("restartBtn", restart), ("pauseBtn", pause), ("backBtn", back),
            ("comboBanner", comboView),
            ("pauseDialog", pauseDialog), ("resumeDialog", resumeDialog), ("tutorial", tutorial));
    }

    static (GameObject overlay, GameObject card) Overlay(GameObject panel, string name, float cardW, float cardH) {
        var overlay = NewUI(name, panel.transform);
        Stretch(Rt(overlay));
        AddImg(overlay, new Color(Maroon.r, Maroon.g, Maroon.b, 0.62f), SprUI, raycast: true);
        var frame = FramedCard(overlay.transform, "Card", Pink, CardWhite, 10f);
        Place(Rt(frame), 0.5f, 0.5f, 0.5f, 0.5f, 0, 0, cardW, cardH);
        return (overlay, frame.transform.Find("Inner").gameObject);
    }

    static PauseDialog BuildPauseDialog(GameObject panel) {
        var (overlay, card) = Overlay(panel, "PauseDialog", 720, 700);
        var title = AddText(card.transform, "Title", "Paused", 54, PinkDeep);
        Place(Rt(title.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -30, 600, 80);
        var resume  = MakeButton(card.transform, "ResumeBtn", "Resume", Pink, Color.white, 40);
        Place(Rt(resume.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, 80, 560, 116);
        var restart = MakeButton(card.transform, "RestartBtn", "Restart", Teal, Color.white, 40);
        Place(Rt(restart.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -70, 560, 116);
        var quit    = MakeButton(card.transform, "QuitBtn", "Save & Quit", Danger, Color.white, 40);
        Place(Rt(quit.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -220, 560, 116);

        var dlg = overlay.AddComponent<PauseDialog>();
        SetRefs(dlg, ("resumeBtn", resume), ("restartBtn", restart), ("quitBtn", quit));
        overlay.SetActive(false);
        return dlg;
    }

    static ResumeDialog BuildResumeDialog(GameObject panel) {
        var (overlay, card) = Overlay(panel, "ResumeDialog", 720, 560);
        var title = AddText(card.transform, "Title", "Welcome back!", 48, PinkDeep);
        Place(Rt(title.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -34, 640, 70);
        var body = AddText(card.transform, "Body", "You left this level unfinished.", 32, Brown, FontStyles.Normal);
        Place(Rt(body.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -120, 620, 90);
        var resume  = MakeButton(card.transform, "ResumeBtn", "Resume", Pink, Color.white, 40);
        Place(Rt(resume.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 190, 560, 116);
        var restart = MakeButton(card.transform, "RestartBtn", "Start Over", Teal, Color.white, 40);
        Place(Rt(restart.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 44, 560, 116);

        var dlg = overlay.AddComponent<ResumeDialog>();
        SetRefs(dlg, ("resumeBtn", resume), ("restartBtn", restart));
        overlay.SetActive(false);
        return dlg;
    }

    static TutorialOverlay BuildTutorialOverlay(GameObject panel) {
        var (overlay, card) = Overlay(panel, "TutorialOverlay", 820, 640);
        var title = AddText(card.transform, "Title", "Find the Words", 48, PinkDeep);
        Place(Rt(title.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -36, 720, 70);
        var body = AddText(card.transform, "Body", "…", 34, Brown, FontStyles.Normal);
        Place(Rt(body.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -130, 700, 240);
        var next = MakeButton(card.transform, "NextBtn", "Next", Pink, Color.white, 40);
        Place(Rt(next.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 44, 460, 112);
        var nextLabel = next.GetComponentInChildren<TextMeshProUGUI>();
        var skip = MakeButton(card.transform, "SkipBtn", "Skip", CardWhite, PinkDeep, 30);
        Place(Rt(skip.gameObject), 1f, 1f, 1f, 1f, -20, -20, 140, 70);

        var tut = overlay.AddComponent<TutorialOverlay>();
        SetRefs(tut, ("titleText", title), ("bodyText", body),
                     ("nextBtn", next), ("skipBtn", skip), ("nextBtnLabel", nextLabel));
        overlay.SetActive(false);
        return tut;
    }

    // ── Daily ───────────────────────────────────────────────────────────────
    static void BuildDaily(GameObject panel) {
        Header(panel, "Daily Challenge");
        var back = BackButton(panel);

        var frame = FramedCard(panel.transform, "Card", Pink, CardWhite, 8f);
        Place(Rt(frame), 0.5f, 0.5f, 0.5f, 0.5f, 0, 140, 900, 620);
        var card = frame.transform.Find("Inner");

        var date = AddText(card, "Date", "2026-01-01", 34, Pink);
        Place(Rt(date.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -36, 800, 60);
        var topic = AddText(card, "Topic", "animals", 64, Brown);
        Place(Rt(topic.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -136, 820, 100);
        var status = AddText(card, "Status", "Tap Play", 34, PinkDeep);
        Place(Rt(status.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -276, 800, 60);
        var streak = AddText(card, "Streak", "Streak: 0d", 34, Green);
        Place(Rt(streak.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -356, 800, 60);
        var play = MakeButton(card, "PlayBtn", "Play Today's Puzzle", Pink, Color.white, 40);
        Place(Rt(play.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 44, 700, 130);

        var view = panel.AddComponent<DailyChallengeView>();
        SetRefs(view, ("dateText", date), ("topicText", topic), ("statusText", status),
                      ("streakText", streak), ("playBtn", play), ("backBtn", back));
    }

    // ── Profile ─────────────────────────────────────────────────────────────
    static void BuildProfile(GameObject panel, GameObject badgePrefab) {
        Header(panel, "Profile");
        var back = BackButton(panel);

        var frame = FramedCard(panel.transform, "StatsCard", Pink, CardWhite, 8f);
        Place(Rt(frame), 0.5f, 1f, 0.5f, 1f, 0, -210, 980, 460);
        var card = frame.transform.Find("Inner");

        TextMeshProUGUI Row(string caption, string init, float y, Color valCol) {
            var cap = AddText(card, caption + "Cap", caption, 32, Brown, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(Rt(cap.gameObject), 0f, 1f, 0f, 1f, 44, y, 420, 60);
            var val = AddText(card, caption + "Val", init, 40, valCol, FontStyles.Bold, TextAlignmentOptions.Right);
            Place(Rt(val.gameObject), 1f, 1f, 1f, 1f, -44, y, 420, 60);
            return val;
        }

        var stars  = Row("Total Stars", "0", -34, Gold);
        var xp     = Row("Total XP", "0", -134, PinkDeep);
        var streak = Row("Streak", "0d", -234, Green);
        var badges = Row("Badges", "0 badges", -334, Purple);

        var badgeLabel = AddText(panel.transform, "BadgesTitle", "BADGES", 34, PinkDeep);
        Place(Rt(badgeLabel.gameObject), 0.5f, 0.5f, 0.5f, 0.5f, 0, -30, 500, 60);
        var badgeGrid = NewUI("BadgeGrid", panel.transform);
        Place(Rt(badgeGrid), 0.5f, 0.5f, 0.5f, 1f, 0, -80, 980, 320);
        var bg = badgeGrid.AddComponent<GridLayoutGroup>();
        bg.cellSize = new Vector2(228, 90);
        bg.spacing = new Vector2(16, 14);
        bg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        bg.constraintCount = 4;
        bg.childAlignment = TextAnchor.UpperCenter;

        var reset = MakeButton(panel.transform, "ResetBtn", "Reset Progress", Danger, Color.white, 36);
        Place(Rt(reset.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 60, 620, 116);

        var view = panel.AddComponent<ProfileView>();
        SetRefs(view,
            ("badgeContainer", badgeGrid.transform), ("badgeCellPrefab", badgePrefab),
            ("totalStarsText", stars), ("totalXpText", xp), ("streakText", streak), ("badgeCountText", badges),
            ("backBtn", back), ("resetBtn", reset));
    }

    // ── Settings ────────────────────────────────────────────────────────────
    static void BuildSettings(GameObject panel) {
        Header(panel, "Settings");
        var back = BackButton(panel);

        var frame = FramedCard(panel.transform, "Card", Pink, CardWhite, 8f);
        Place(Rt(frame), 0.5f, 1f, 0.5f, 1f, 0, -210, 980, 900);
        var card = frame.transform.Find("Inner");

        var res = new DefaultControls.Resources {
            standard = SprUI, background = SprBg, knob = SprKnob, checkmark = SprCheck
        };

        Toggle MakeToggle(string label, float y) {
            var t = DefaultControls.CreateToggle(res);
            t.name = label.Replace(" ", "") + "Toggle";
            t.transform.SetParent(card, false);
            Place(Rt(t), 0f, 1f, 0f, 1f, 60, y, 700, 70);
            var toggle = t.GetComponent<Toggle>();
            // Swap uGUI Text label for TMP.
            var oldLabel = t.transform.Find("Label");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
            var tmp = AddText(t.transform, "Label", label, 34, Brown, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(Rt(tmp.gameObject), 0f, 0.5f, 0f, 0.5f, 60, 0, 620, 60);
            // Enlarge the checkbox.
            var bg = t.transform.Find("Background");
            if (bg != null) {
                Place((RectTransform)bg, 0f, 0.5f, 0f, 0.5f, 0, 0, 44, 44);
                var check = bg.Find("Checkmark");
                if (check != null) {
                    Stretch((RectTransform)check, 4, 4, 4, 4);
                    ((RectTransform)check).anchorMin = Vector2.zero;
                    ((RectTransform)check).anchorMax = Vector2.one;
                    var ci = check.GetComponent<Image>();
                    if (ci != null) ci.color = PinkDeep;
                }
            }
            return toggle;
        }

        Slider MakeSlider(string label, float y) {
            var caption = AddText(card, label + "Cap", label, 34, Brown, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(Rt(caption.gameObject), 0f, 1f, 0f, 1f, 60, y, 500, 60);
            var s = DefaultControls.CreateSlider(res);
            s.name = label.Replace(" ", "") + "Slider";
            s.transform.SetParent(card, false);
            Place(Rt(s), 0f, 1f, 0f, 1f, 60, y - 64, 840, 44);
            var slider = s.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0.8f;
            var fill = s.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (fill != null) fill.color = Pink;
            var handle = s.transform.Find("Handle Slide Area/Handle")?.GetComponent<Image>();
            if (handle != null) handle.color = PinkDeep;
            var sBg = s.transform.Find("Background")?.GetComponent<Image>();
            if (sBg != null) sBg.color = PinkSoft;
            return slider;
        }

        var sound   = MakeToggle("Sound Effects", -60);
        var haptics = MakeToggle("Vibration", -160);
        var motion  = MakeToggle("Reduce Motion", -260);
        var music   = MakeSlider("Music Volume", -390);
        var sfx     = MakeSlider("SFX Volume", -550);

        var view = panel.AddComponent<SettingsView>();
        SetRefs(view,
            ("soundToggle", sound), ("hapticsToggle", haptics), ("reduceMotionToggle", motion),
            ("musicSlider", music), ("sfxSlider", sfx), ("backBtn", back));
    }

    // ── LevelComplete ───────────────────────────────────────────────────────
    static void BuildLevelComplete(GameObject panel) {
        var dim = NewUI("Dim", panel.transform);
        Stretch(Rt(dim));
        AddImg(dim, new Color(Maroon.r, Maroon.g, Maroon.b, 0.58f), SprUI, raycast: true);

        var frame = FramedCard(panel.transform, "Card", Pink, CardWhite, 10f);
        Place(Rt(frame), 0.5f, 0.5f, 0.5f, 0.5f, 0, 40, 880, 1060);
        var card = frame.transform.Find("Inner");

        var header = AddText(card, "Header", "Level Complete!", 56, PinkDeep);
        Place(Rt(header.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -40, 800, 80);

        // Star slots — gold gems (rotated squares) that LevelCompleteView reveals.
        var slots = new GameObject[3];
        for (int i = 0; i < 3; i++) {
            var slot = NewUI($"Star{i + 1}", card.transform);
            Place(Rt(slot), 0.5f, 1f, 0.5f, 1f, (i - 1) * 190, -206, 130, 130);
            slot.transform.localRotation = Quaternion.Euler(0, 0, 45);
            AddImg(slot, Gold, SprUI);
            var inner = NewUI("Inner", slot.transform);
            Stretch(Rt(inner), 18, 18, 18, 18);
            AddImg(inner, Hex("#FFE08A"), SprUI);
            slot.SetActive(false);
            slots[i] = slot;
        }

        var xp = AddText(card, "Xp", "+0 XP", 46, Green);
        Place(Rt(xp.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -366, 700, 70);
        var time = AddText(card, "Time", "00:00", 36, Brown);
        Place(Rt(time.gameObject), 0.5f, 1f, 0.5f, 1f, 0, -436, 700, 60);

        // "NEW BEST!" stamp — slightly tilted gold pill over the star row.
        var badge = NewUI("NewBestBadge", card.gameObject.transform);
        Place(Rt(badge), 0.5f, 1f, 0.5f, 1f, 250, -150, 300, 86);
        badge.transform.localRotation = Quaternion.Euler(0, 0, 12);
        AddImg(badge, Gold, SprUI);
        var badgeText = AddText(badge.transform, "Text", "NEW BEST!", 34, Maroon);
        Stretch(Rt(badgeText.gameObject), 6, 6, 4, 4);
        badge.SetActive(false);

        // Victory confetti rains over the whole panel, above the card.
        var confettiGo = NewUI("Confetti", panel.transform);
        Stretch(Rt(confettiGo));
        var confetti = confettiGo.AddComponent<ConfettiRain>();

        var next   = MakeButton(card, "NextBtn", "Next Level", Pink, Color.white, 40);
        Place(Rt(next.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 344, 620, 120);
        var replay = MakeButton(card, "ReplayBtn", "Replay", Teal, Color.white, 40);
        Place(Rt(replay.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 196, 620, 120);
        var topics = MakeButton(card, "TopicsBtn", "Topics", Purple, Color.white, 40);
        Place(Rt(topics.gameObject), 0.5f, 0f, 0.5f, 0f, 0, 48, 620, 120);

        var view = panel.AddComponent<LevelCompleteView>();
        SetObjArray(view, "starSlots", slots);
        SetRefs(view, ("xpText", xp), ("timeText", time), ("headerText", header),
                      ("nextBtn", next), ("replayBtn", replay), ("topicsBtn", topics),
                      ("newBestBadge", badge), ("confetti", confetti));
    }

    // ═══════════════════════════ Bootstrap scene ═══════════════════════════

    static void BuildBootstrapScene() {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgTop;
        camGo.AddComponent<AudioListener>();
        camGo.tag = "MainCamera";

        var services = new GameObject("ServicesRoot");
        services.AddComponent<SoundManager>();
        services.AddComponent<MusicManager>();
        services.AddComponent<AppBootstrap>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
    }
}
