# AlphaRush — Build Tasklist

A copy-paste prompt list for Claude Code to build the Unity port of AlphaRush.

**Scope (locked):** spec §19 acceptance criteria only — 22 topics, 660 levels, stars/XP/topic unlocks, daily challenge, save/resume, tutorial, settings. **No** boosters/lives/world-map/heavy juice (port guide §24 is out of scope).
**Assets (locked):** placeholders first — Unity primitive rectangles for tiles, emoji glyphs for icons, solid-color sprites for cards, silent SFX stubs. Real art/audio swapped in last.

How to use this doc:
1. Work through phases top-down. Each task is one Claude Code prompt — copy the block under **Prompt** verbatim into a fresh Claude Code session in this repo.
2. Tasks marked **[CLAUDE]** are pure code/file work — paste and let it run.
3. Tasks marked **[USER]** are Unity Editor work that only you can do. Step-by-step instructions are in §C.
4. Each phase ends with an acceptance check (what should compile / what should pass / what you should see). Do not move on until it passes.

The two authoritative docs are `docs/ALPHARUSH_SPEC.md` (what the game does) and `docs/UNITY_PORT_GUIDE.md` (how to assemble it in Unity). Every prompt below points back to specific sections of those docs.

---

## §A. First-Time Setup (do this once before any Claude prompts)

### A.1 Install Unity Hub
1. Download Unity Hub from <https://unity.com/download>. Run the installer.
2. Sign in with a personal Unity ID (free).

### A.2 Install the Unity Editor
1. In Unity Hub → **Installs** → **Install Editor** → pick **6000.4.7f1** (exact version this repo was created with — check `ProjectSettings/ProjectVersion.txt` if a different patch is shown).
2. In the **Add modules** step tick:
   - **Android Build Support**
   - **Android SDK & NDK Tools**
   - **OpenJDK**
3. Click Install. Coffee break — this is ~6 GB.

### A.3 Open the project
1. In Unity Hub → **Projects** → **Add** → browse to `C:\Github\AlphaRush` and select the folder (the one containing `Assets/`, `Packages/`, `ProjectSettings/`).
2. Click the project name to open it. First open takes 5–15 min while Unity rebuilds the `Library/` cache. Lots of red console errors during this first import are normal.

### A.4 Import DOTween (free, from Asset Store)
DOTween is required by the polish layer but is **not** a UPM package — it lives on the Asset Store.

1. In your browser, open <https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676>. Click **Add to My Assets** (free).
2. Back in Unity → **Window → Package Manager** → switch the source dropdown (top-left) to **My Assets** → find **DOTween (HOTween v2)** → click **Download**, then **Import**. Leave all checkboxes ticked in the Import dialog.
3. After import, run **Tools → Demigiant → DOTween Utility Panel → Setup DOTween…** → click **Apply**. Wait for the recompile.

### A.5 Verify the Editor is healthy
- The **Console** (Window → General → Console) should be free of compile errors. Warnings are fine.
- **Window → General → Test Runner** opens — the **EditMode** tab will show empty (no test classes compile yet because `GridGenerator` etc. don't exist — that's expected; you'll fix it in Phase 1).

### A.6 Useful Editor shortcuts (memorise these — you'll use them constantly)

| Action | Shortcut |
|---|---|
| Enter Play Mode (test in Editor) | **Ctrl+P** |
| Save scene | **Ctrl+S** |
| Open Inspector | **Ctrl+3** |
| Open Project window | **Ctrl+5** |
| Open Hierarchy | **Ctrl+4** |
| Open Game view | **Ctrl+2** |
| Open Scene view | **Ctrl+1** |

---

## §B. Phased Task List

Each task block has: **Goal**, **Prompt** (copy verbatim into Claude Code), **Acceptance** (what should be true after).

---

### Phase 0 — Project configuration

#### Task 0.1 [CLAUDE] — Player settings, Android manifest, asmdef references

**Goal:** lock target platform, orientation, package name; add `Assets/Plugins/Android/AndroidManifest.xml`; wire TMP into the runtime assembly so later UI scripts compile.

**Prompt:**
```
Configure Unity project settings for AlphaRush per docs/UNITY_PORT_GUIDE.md §1.2 and §1.5.

1. Edit ProjectSettings/ProjectSettings.asset to set:
   - companyName: TechnoNext
   - productName: AlphaRush
   - applicationIdentifier (Android): online.alpharush
   - bundleVersion: 1.0.0
   - AndroidBundleVersionCode: 1
   - defaultScreenOrientation: 4 (Auto Rotation)
   - allowedAutorotateToPortrait, allowedAutorotateToPortraitUpsideDown, allowedAutorotateToLandscapeRight, allowedAutorotateToLandscapeLeft: all 1
   - colorSpace: 1 (Linear)
   - AndroidMinSdkVersion: 24
   - AndroidTargetSdkVersion: 34
   - AndroidTargetArchitectures: 1 (ARM64 only)
   - scriptingBackend for Android: 1 (IL2CPP)
   - apiCompatibilityLevel: 6 (.NET Standard 2.1)
   - useCustomMainManifest: 1

2. Create Assets/Plugins/Android/AndroidManifest.xml exactly as shown in port guide §1.5 (VIBRATE permission, fullSensor orientation, configChanges including orientation+screenSize).

3. Edit Assets/Scripts/AlphaRush.Runtime.asmdef so its "references" array contains "Unity.TextMeshPro" — on Unity 6 LTS, TMP ships inside com.unity.ugui as the assembly Unity.TextMeshPro, and our asmdef must reference it explicitly or `using TMPro;` will fail in Phase 7+ UI scripts. Leave overrideReferences as false so DOTween.dll (a plugin) remains auto-referenced.

4. Create Assets/Scripts/Effects/DOTweenShortcuts.cs with extension methods for the subset of DOTween's optional UI/Audio module extensions we actually call:
     - Image.DOFade(float, float), Image.DOColor(Color, float)
     - CanvasGroup.DOFade(float, float)
     - AudioSource.DOFade(float, float)
     - TMP_Text.DOFade(float, float), TMP_Text.DOColor(Color, float)
     - RectTransform.DOShakeAnchorPos(float duration, Vector2 strength, int vibrato = 14, float randomness = 90, bool snapping = false, bool fadeOut = true)
   Implement each as a thin DOTween.To / DOTween.Sequence wrapper using DG.Tweening.Core + DG.Tweening.Plugins.Options (these live in DOTween.dll core, no module asmdef needed).
   Why we do this instead of using DOTween's actual modules: those extensions live as .cs source files in Assets/Plugins/Demigiant/DOTween/Modules/ with no asmdef, so they compile into Assembly-CSharp-firstpass — which named asmdefs (like AlphaRush.Runtime) can't reference. The "Create ASMDEF" button in the DOTween Utility Panel is supposed to fix this but its asset-post-processor watchdog deletes any asmdef we add to that folder and doesn't reliably produce a replacement. Our own shim sidesteps the whole fight.

5. Verify Packages/manifest.json already has com.unity.nuget.newtonsoft-json (it does). Do NOT add Cinemachine — out of scope for v1.

Show me a diff summary at the end.
```

**Acceptance:** Open Unity → **Edit → Project Settings → Player** → Android tab → Identification shows `online.alpharush`. **Resolution and Presentation** shows Auto Rotation with all four orientations ticked. `Assets/Plugins/Android/AndroidManifest.xml` exists. `AlphaRush.Runtime.asmdef` lists `Unity.TextMeshPro` in `references`. `Assets/Scripts/Effects/DOTweenShortcuts.cs` exists. **Do not open Tools → Demigiant → DOTween Utility Panel** after this point — its asset-post-processor watchdog will delete the shim if asked to "Setup DOTween".

---

### Phase 1 — Pure logic + EditMode tests (port guide §5)

> The EditMode test files in `Assets/Tests/EditMode/` already exist (`GridGeneratorTests.cs`, `SelectionEngineTests.cs`, `ScoreCalculatorTests.cs`) but reference types that don't exist yet. Phase 1 makes them compile and pass.

#### Task 1.1 [CLAUDE] — Models

**Prompt:**
```
Create C# model files under Assets/Scripts/Model/ per docs/UNITY_PORT_GUIDE.md §4 (also docs/ALPHARUSH_SPEC.md §6 for semantics). One file per type, all in root namespace (no namespace declaration — matches asmdef rootNamespace=""):

- Difficulty.cs (enum + DifficultyExt static class with GridSize/MaxWords/TimeBonusSec extensions per §4.1)
- WordDirection.cs (enum per §4.2)
- Topic.cs (matches §4.3 + topics.json shape — accentColor as long)
- Level.cs
- CellSelection.cs (use a class, not struct — guide passes it as List<CellSelection> with null checks)
- PlacedWord.cs
- FoundWord.cs
- SavedGameState.cs
- LevelResult.cs
- PlayerProgress.cs (Dictionary<string, LevelResult> for bestResults)
- DailyChallenge.cs

All classes [System.Serializable]. No UnityEngine dependencies allowed in this folder — these must remain portable so EditMode tests can use them.
```

**Acceptance:** Files exist, no compile errors in Console. EditMode tests still fail (next task fixes them).

#### Task 1.2 [CLAUDE] — Constants

**Prompt:**
```
Create Assets/Scripts/Constants/ files per docs/UNITY_PORT_GUIDE.md §3 and docs/ALPHARUSH_SPEC.md §3:

- AppColors.cs (full palette + HighlightColors[] per port guide §3.1 — uses UnityEngine.Color)
- AppConfig.cs (per port guide §3.2)
- AppDimensions.cs (per port guide §3.3)
- AppStrings.cs (just create a placeholder static class with const string AppName = "AlphaRush" and const string Tagline = "Find words. Beat the clock. Climb the alphabet." — we'll expand as screens need text)

These can reference UnityEngine.
```

**Acceptance:** No compile errors.

#### Task 1.3 [CLAUDE] — GridGenerator

**Prompt:**
```
Create Assets/Scripts/Game/GridGenerator.cs exactly per docs/UNITY_PORT_GUIDE.md §5.1. Pure logic, no UnityEngine reference. It must satisfy the existing EditMode test at Assets/Tests/EditMode/GridGeneratorTests.cs (SameSeedProducesSameGrid + AllPlacedWordsRetrievable).

Key invariants:
- Deterministic: same seed → identical grid (use a single System.Random(seed) threaded through; never call top-level Random)
- Direction set filtered by Difficulty per port guide §5.1 AllowedDirections + spec §8.1
- Returns the best-placement grid even if not all words fit (50 attempts)
- Empty cells filled with A–Z via the same RNG
```

**Acceptance:** Open Unity → wait for recompile → **Window → General → Test Runner → EditMode → Run All**. `GridGeneratorTests` both pass green.

#### Task 1.4 [CLAUDE] — SelectionEngine

**Prompt:**
```
Create Assets/Scripts/Game/SelectionEngine.cs per docs/UNITY_PORT_GUIDE.md §5.2 (static class with IsValidSelection + GetCellsBetween). Pure logic, no UnityEngine reference. Must pass the existing tests in Assets/Tests/EditMode/SelectionEngineTests.cs.
```

**Acceptance:** SelectionEngineTests pass green in Test Runner.

#### Task 1.5 [CLAUDE] — ScoreCalculator + ComboTracker

**Prompt:**
```
Create two pure-logic files under Assets/Scripts/Game/:

1. ScoreCalculator.cs — static, per docs/UNITY_PORT_GUIDE.md §5.3. Must pass existing tests in Assets/Tests/EditMode/ScoreCalculatorTests.cs.

2. ComboTracker.cs — per docs/UNITY_PORT_GUIDE.md §5.4. Constructor takes optional windowMs (default 3500). Properties Combo + MaxCombo. Method OnWordFound() returns new combo level.

Then add a new EditMode test file Assets/Tests/EditMode/ComboTrackerTests.cs with at least:
- Combo resets after window expires (inject clock via constructor — refactor ComboTracker to take Func<long> clock = null, default to DateTimeOffset.Now.ToUnixTimeMilliseconds)
- Consecutive finds within window increment combo
- MaxCombo tracks the high-water mark
```

**Acceptance:** All four EditMode test classes pass green.

#### Task 1.6 [USER] — Verify EditMode tests green

In Unity Editor:
1. **Window → General → Test Runner**.
2. Click **EditMode** tab.
3. Click **Run All** at top-left.
4. All tests should be green. If any fail, paste the test output into a new Claude Code prompt and ask for a fix.

---

### Phase 2 — Content + Data layer (port guide §6)

#### Task 2.1 [CLAUDE] — Complete topics.json

**Prompt:**
```
The file Assets/StreamingAssets/data/topics.json currently has only a partial topic list. Replace it with the full 22 topics from docs/ALPHARUSH_SPEC.md §7.1 table.

Schema per topic:
{
  "id": "<id>",
  "name": "<name>",
  "icon": "<emoji>",
  "accentColor": <long — UInt32 packed 0xAARRGGBB; use 0xFF prefix for alpha 255; e.g. AppColors.FunYellow #FFEB3B = 0xFFFFEB3B = 4294961979>,
  "wordPool": [ "WORD1", "WORD2", ... ],
  "unlockStarsRequired": <int — see spec §7.3 thresholds: animals/fruits/colors/family = 0; subsequent topics 5,10,15,20,25,30,35,40,45,50,60,70,80,90,100,115,130,150 in the order they appear in §7.1>
}

All words UPPERCASE. Pull the sample words from the spec table; pad to at least 12 words per topic where the spec lists fewer (use kid-appropriate additions). Output strict JSON — no trailing commas, no comments. Preserve UTF-8 encoding so emoji survive.
```

**Acceptance:** `topics.json` has 22 top-level entries. Validate by running `python -c "import json; print(len(json.load(open('Assets/StreamingAssets/data/topics.json'))))"` in PowerShell — should print `22`.

#### Task 2.2 [CLAUDE] — Content data source layer

**Prompt:**
```
Create the content layer per docs/UNITY_PORT_GUIDE.md §6:

1. Assets/Scripts/Data/IContentDataSource.cs — interface with:
   Task<List<Topic>> LoadTopicsAsync();
   List<Level> GenerateLevels(string topicId);

2. Assets/Scripts/Data/LocalContentDataSource.cs — implementation per port guide §6.2. Reads StreamingAssets/data/topics.json using UnityWebRequest on Android (since StreamingAssets is inside the APK) and File.ReadAllText elsewhere. Cache topics in-memory after first load. GenerateLevels per spec §7.2 distribution: levels 1–8 EASY, 9–18 MEDIUM, 19–26 HARD, 27–30 EXPERT; targetWordCount ramps per port guide §6.2 TargetWordCount formula; seed = (long)topicId.GetHashCode() * 31 + i.

3. Assets/Scripts/Data/RemoteContentDataSource.cs — implements IContentDataSource but every method throws NotImplementedException. Wired up later via AppConfig.UseRemoteContent.

4. Assets/Scripts/Data/ContentRepository.cs — thin facade exposing:
   Task<List<Topic>> GetTopicsAsync();
   Task<Topic> GetTopicAsync(string topicId);
   Task<List<Level>> GetLevelsAsync(string topicId);
   Task<Level> GetLevelAsync(string topicId, int levelId);
   Task<List<string>> GetWordsForLevelAsync(Level level);   // picks targetWordCount words from topic.wordPool deterministically using level.seed

   Constructor takes IContentDataSource (instantiate Local or Remote based on AppConfig.UseRemoteContent in the AppBootstrap later).
```

**Acceptance:** No compile errors. Repo compiles.

---

### Phase 3 — Persistence (port guide §7)

#### Task 3.1 [CLAUDE] — Storage classes

**Prompt:**
```
Create four storage classes under Assets/Scripts/Data/ per docs/UNITY_PORT_GUIDE.md §7. Each is a plain C# class (not MonoBehaviour) that the AppBootstrap will instantiate and hand to the ServiceLocator:

1. ProgressStorage.cs — verbatim from port guide §7.1. Path: Application.persistentDataPath/progress.json. Default unlockedTopicIds = { "animals", "fruits", "colors", "family" } per spec §7.3.

2. SettingsStorage.cs — Settings POCO per port guide §7.2 plus a load/save pair like ProgressStorage. Path: settings.json.

3. GameStateStorage.cs — single file saved_games.json containing Dictionary<string, SavedGameState> keyed by "topicId:levelId". Methods: Save(SavedGameState), Load(string topicId, int levelId), Delete(string topicId, int levelId), HasSaved(string topicId, int levelId).

4. DailyChallengeStorage.cs — single file daily.json containing { DailyChallenge Current, int StreakDays, string LastCompletedDate }. Methods: Load, Save, MarkCompleted(int stars).

All use Newtonsoft.Json (already installed). Wrap reads/writes in try/catch — on parse failure, log a warning and return defaults rather than throw (we don't want corrupted save state to crash the app).
```

**Acceptance:** Compiles. We'll verify behavior later via Play mode.

---

### Phase 4 — Core services (port guide §8, §9, §17)

#### Task 4.1 [CLAUDE] — Audio managers (stubs)

**Prompt:**
```
Create audio stubs that compile and run with no actual audio files yet (placeholder phase). Per docs/UNITY_PORT_GUIDE.md §8:

1. Assets/Scripts/Audio/SoundEvent.cs — enum per port guide §8.2.

2. Assets/Scripts/Audio/SoundManager.cs — MonoBehaviour per port guide §8.3. AudioClip[] clips field, AudioMixerGroup sfxGroup field — but tolerate clips being null/empty at runtime (skip play silently). Public Play(SoundEvent, pitch, volume). SetEnabled(bool).

3. Assets/Scripts/Audio/MusicManager.cs — MonoBehaviour per port guide §8.4. Two AudioSource A/B with crossfade via DG.Tweening DOTween.To on volume (400 ms). Public PlayMusic(AudioClip clip), Stop(), SetEnabled(bool), SetVolume(float). Tolerate null clip.

Use `using DG.Tweening;` — DOTween was set up in §A.4.
```

**Acceptance:** Compiles. No runtime errors when Play(SoundEvent.WIN) called with empty clips array (should log a debug warning, not throw).

#### Task 4.2 [CLAUDE] — Haptic manager

**Prompt:**
```
Create Assets/Scripts/Haptic/HapticManager.cs per docs/UNITY_PORT_GUIDE.md §9.1 verbatim — static class, AndroidJavaObject Vibrator behind #if UNITY_ANDROID && !UNITY_EDITOR. In Editor or other platforms, all methods no-op. Methods: Tick, Light, Success, Win, Lose, Combo(int level). Enabled property defaulting to true.
```

**Acceptance:** Compiles in Editor (no Android compile errors).

#### Task 4.3 [CLAUDE] — Event bus + service locator + bootstrap

**Prompt:**
```
Create the application root per docs/UNITY_PORT_GUIDE.md §10 and §17:

1. Assets/Scripts/Core/GameEvents.cs — static class with C# events per port guide §17. Include all listed events: CellTouchDown, CellAddedToSelection (int chainLen), InvalidRelease, WordFound (PlacedWord, int combo, Color), Combo (int), HintUsed, PauseOpened, TimeWarning, TimeUp, LevelComplete (LevelResult), TopicUnlocked (string), BadgeEarned (string), DailyStreakIncrement. Provide Raise_* helper methods so callers don't deal with null event invocations.

2. Assets/Scripts/Core/ServiceLocator.cs — simple static class exposing:
   public static SoundManager Sound { get; set; }
   public static MusicManager Music { get; set; }
   public static ContentRepository Content { get; set; }
   public static ProgressStorage Progress { get; set; }
   public static SettingsStorage Settings { get; set; }
   public static GameStateStorage GameState { get; set; }
   public static DailyChallengeStorage Daily { get; set; }
   public static PanelRouter Router { get; set; }     // forward-ref OK, will be created in Phase 5

3. Assets/Scripts/Core/AppBootstrap.cs — MonoBehaviour. In Awake():
   - DontDestroyOnLoad(this.gameObject)
   - Instantiate ProgressStorage, SettingsStorage, GameStateStorage, DailyChallengeStorage and assign to ServiceLocator
   - Instantiate LocalContentDataSource, wrap in ContentRepository, assign to ServiceLocator.Content
   - Find or create SoundManager + MusicManager components on the same GameObject, assign to ServiceLocator
   In Start():
   - Apply Settings.soundEnabled to SoundManager
   - Preload topics: await ServiceLocator.Content.GetTopicsAsync() (don't block — use async void Start)
   - Load Main scene additively if currently in Bootstrap scene (use SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single))

   Use UnityEngine.SceneManagement.
```

**Acceptance:** Compiles. Cannot Play-test until Phase 5 creates panels for AppBootstrap to drive.

---

### Phase 5 — UI core scripts (port guide §10.3, §10.4, §10.2)

#### Task 5.1 [CLAUDE] — UI core utility scripts

**Prompt:**
```
Create four UI core MonoBehaviours under Assets/Scripts/UI/Core/ per docs/UNITY_PORT_GUIDE.md §10:

1. SafeAreaFitter.cs — port guide §10.3 verbatim.

2. OrientationWatcher.cs — port guide §10.4 verbatim. Use UnityEngine.Events.UnityEvent<bool>.

3. GradientBackground.cs — MonoBehaviour that draws a vertical gradient between two colors on its Image component using Image.material with a tiny generated mesh, OR simpler: just set the Image color from a lerp and add a second Image child for the bottom color with a vertical fade. Simplest acceptable: assign two Color fields topColor + bottomColor, and at Awake set the Image to a 1×2 procedural texture (Texture2D, point filter, clamp wrap) containing those two colors — Image stretches it. Provide SetColors(Color top, Color bottom). Defaults: AppColors.BackgroundLight (top), AppColors.BackgroundMedium (bottom).

4. PanelRouter.cs — MonoBehaviour. Holds:
   [SerializeField] GameObject[] mainPanels;       // Splash, Home, TopicList, LevelSelect, Game, DailyChallenge, Profile, Settings, LevelComplete
   [SerializeField] string[] panelNames;           // parallel array of route names matching mainPanels
   public void Show(string panelName)              // SetActive false on all, true on the match. Warn on miss.
   public string Current { get; private set; }
   public event System.Action<string> OnPanelChanged;
   In Awake, register itself to ServiceLocator.Router.

Also create a tiny helper Assets/Scripts/UI/Core/Routes.cs static class with const string Splash="Splash", Home="Home", TopicList="TopicList", LevelSelect="LevelSelect", Game="Game", Daily="Daily", Profile="Profile", Settings="Settings", LevelComplete="LevelComplete".
```

**Acceptance:** Compiles.

---

### Phase 6 — Editor scene + panel skeleton [USER]

This is the **only big block of pure Editor work** in the build. Take your time. After this, you're back to copy-pasting Claude prompts.

Follow §C.1 below step by step. Don't skip a step — Unity is unforgiving about scene structure mistakes.

**Acceptance:** Two scenes (`Bootstrap.unity`, `Main.unity`) exist in `Assets/Scenes/`. Bootstrap is index 0 in Build Settings, Main is index 1. Press **Ctrl+P** in the Bootstrap scene → it loads Main → you see a blank gradient background + nine empty panel GameObjects in the Hierarchy.

---

### Phase 7 — Tile + grid runtime

#### Task 7.1 [CLAUDE] — TileView + GameGridView scripts

**Prompt:**
```
Create two UI component scripts per docs/UNITY_PORT_GUIDE.md §11:

1. Assets/Scripts/UI/Components/TileView.cs — per port guide §11.2 verbatim. References to Image background, highlight, foundOverlay, TMP_Text letter. Public Row, Col, Set(c, row, col), SetSelected(bool), PlayFound(Color), PlayHintPulse, Reset. Uses DG.Tweening.

2. Assets/Scripts/UI/Components/GameGridView.cs — per port guide §11.3 verbatim BUT drop the UILineRenderer reference (we'll skip drawing the selection trail line for the placeholder pass — selection visuals come from per-tile highlights only). Keep the pointer-event interfaces, the Build(char[,]) method, the OnSelectionComplete callback. Tile(r,c) accessor. Compute cellSize from RectTransform width/height divided by grid size with a small padding.

Use `using TMPro;` for the text component.
```

**Acceptance:** Compiles.

#### Task 7.2 [USER] — Build TileView prefab

Follow §C.2 to build `Assets/Prefabs/Tiles/TileView.prefab` from primitives. ~5 min in the Editor.

---

### Phase 8 — Reusable view components

#### Task 8.1 [CLAUDE] — Small view components

**Prompt:**
```
Create small reusable UI scripts under Assets/Scripts/UI/Components/. Each is a MonoBehaviour wrapping a RectTransform with serialized references. Keep them dumb — no game logic, just data-binding setters.

1. WordChipView.cs — fields: TMP_Text label; Image background. SetWord(string w). MarkFound(Color highlight) — sets background.color, adds strikethrough on label (use letter.fontStyle |= FontStyles.Strikethrough), runs a small DOTween scale punch (1.05, 0.15s, 2 yoyo loops). Reset() restores defaults.

2. TopicCardView.cs — fields: TMP_Text nameLabel, iconLabel (emoji), starsLabel, requirementLabel; Image accentStrip; GameObject lockOverlay. Bind(Topic topic, int starsEarned, int totalStars, bool isLocked). Click event: Button onClick → public UnityEvent<string> OnClicked (topic id).

3. LevelDotView.cs — fields: TMP_Text levelNumberLabel; GameObject lockIcon; Image[] starSlots (size 3); Image accentRing. Bind(int levelId, int starsEarned, bool isUnlocked, Color topicAccent). public UnityEvent<int> OnClicked (level id).

4. StatChipView.cs — fields: TMP_Text iconLabel, valueLabel, captionLabel. Bind(string icon, string value, string caption).

5. TimerView.cs — field: TMP_Text label. SetSeconds(int s) formats m:ss. PulseWarning(bool over) — when true, DOTween.To pulses label.color between default and AppColors.FunRed.

6. ComboBannerView.cs — port guide §14.5 verbatim — Show(int comboLevel) with scale-bounce animation, auto-hide after 1.2s. Banner is the same gameObject; bannerText is a TMP_Text child.

7. PauseDialog.cs — fields: Button resumeBtn, restartBtn, quitBtn. public UnityEvent OnResume, OnRestart, OnQuit. Hook buttons in Start.

8. ResumeDialog.cs — fields: Button resumeBtn, restartBtn. public UnityEvent OnResume, OnRestart.

9. TutorialOverlay.cs — array of step panels (4 GameObjects), step index, Next button, Skip button. Show() resets to step 0. Hide() disables. public UnityEvent OnFinished.
```

**Acceptance:** Compiles.

#### Task 8.2 [USER] — Build prefabs for each component

Follow §C.3 — build minimal prefab per component using Unity primitives. ~30 min total.

---

### Phase 9 — Screen views

Each screen is a MonoBehaviour on its respective `Panel_*` GameObject. They read from `ServiceLocator`, populate child UI, and trigger `ServiceLocator.Router.Show(...)` on navigation.

#### Task 9.1 [CLAUDE] — SplashView + HomeView + SettingsView

**Prompt:**
```
Create three view scripts per docs/UNITY_PORT_GUIDE.md §13 and docs/ALPHARUSH_SPEC.md §9.1, §9.2, §9.9:

1. Assets/Scripts/UI/Screens/SplashView.cs — OnEnable: 1.2 s DOTween scale-in on a centered TMP_Text "AlphaRush", then ServiceLocator.Router.Show(Routes.Home).

2. Assets/Scripts/UI/Screens/HomeView.cs — OnEnable: populate four StatChipViews (Topics found/total, Levels played, Stars total, Streak days) from ServiceLocator.Progress.Load(). Three big buttons wired to Show(Routes.TopicList | Routes.Daily | Routes.Profile). Settings gear button wired to Show(Routes.Settings).
   Serialize: StatChipView topicsChip, levelsChip, starsChip, streakChip; Button playBtn, dailyBtn, profileBtn, settingsBtn.

3. Assets/Scripts/UI/Screens/SettingsView.cs — fields:
   Toggle soundToggle, hapticsToggle, reduceMotionToggle;
   Slider musicSlider, sfxSlider;
   Button resetTutorialBtn, resetProgressBtn, backBtn;
   OnEnable: read SettingsStorage, set control values. Add listeners that write back on change and apply to SoundManager/MusicManager/HapticManager immediately.
   resetTutorial: settings.tutorialShown = false; save.
   resetProgress: show confirmation via a child GameObject containing Yes/No buttons; on Yes, ProgressStorage.ClearAll() + GameStateStorage.ClearAll().
```

**Acceptance:** Compiles.

#### Task 9.2 [CLAUDE] — TopicListView + LevelSelectView

**Prompt:**
```
Create two list screens per docs/UNITY_PORT_GUIDE.md §13.3, §13.4 and spec §9.3, §9.4:

1. Assets/Scripts/UI/Screens/TopicListView.cs — references:
   GridLayoutGroup gridContainer (2 columns);
   TopicCardView cardPrefab;
   Button backBtn.
   OnEnable: clear children, await ServiceLocator.Content.GetTopicsAsync(), instantiate one TopicCardView per topic. For each card compute: starsEarned = sum of best results for that topic; isLocked = (totalStars < topic.unlockStarsRequired) && !progress.unlockedTopicIds.Contains(topic.id). When a topic gains enough stars, add it to unlockedTopicIds and save.
   Card OnClicked → ServiceLocator.Router.Show(Routes.LevelSelect); also stash selected topicId in a static SelectedTopicId field on TopicListView so LevelSelectView can read it.

2. Assets/Scripts/UI/Screens/LevelSelectView.cs — references:
   GridLayoutGroup gridContainer (3 columns);
   LevelDotView dotPrefab;
   TMP_Text headerLabel; Button backBtn.
   OnEnable: read TopicListView.SelectedTopicId, fetch topic + 30 levels via ServiceLocator.Content. Instantiate one LevelDotView per level. Unlock rule per spec §7.3: level 1 always unlocked; level N>1 unlocked iff level N-1 has best result with stars >= 1.
   Dot OnClicked → stash selectedLevelId in a static SelectedLevelId field, then Router.Show(Routes.Game).
```

**Acceptance:** Compiles.

#### Task 9.3 [CLAUDE] — GameView (the big one)

**Prompt:**
```
Create Assets/Scripts/UI/Screens/GameView.cs per docs/UNITY_PORT_GUIDE.md §12.1 outline and docs/ALPHARUSH_SPEC.md §9.5, §8.3, §8.5, §8.6, §8.7.

Public Open(string topicId, int levelId) — entry point. LevelSelectView calls this in its onClick handler before showing Game, OR GameView reads TopicListView.SelectedTopicId + LevelSelectView.SelectedLevelId on OnEnable.

Responsibilities:
- Serialized refs: GameGridView gridView; Transform wordListContainer; WordChipView wordChipPrefab; TimerView timer; TMP_Text headerTopic, progressLine; Button hintBtn, restartBtn, pauseBtn, backBtn; ComboBannerView comboBanner; PauseDialog pauseDialog; ResumeDialog resumeDialog; TutorialOverlay tutorialOverlay; GameObject layoutPortrait, layoutLandscape; OrientationWatcher orientationWatcher.

- On Open:
  1. Load topic + level via ServiceLocator.Content.
  2. Check ServiceLocator.GameState.HasSaved → if yes, show ResumeDialog (Resume vs Restart). On Restart, clear saved state and StartFresh. On Resume, RestoreFrom(savedState).
  3. Otherwise StartFresh.
  4. If ServiceLocator.Settings.Load().tutorialShown == false → show tutorialOverlay, set tutorialShown = true after dismiss.

- StartFresh:
  Pick targetWordCount words from topic.wordPool via deterministic shuffle seeded with level.seed.
  Generate grid via GridGenerator.
  Build gridView, instantiate WordChipViews into wordListContainer, reset timer + hintsUsed + colorIndex + comboTracker.
  StartCoroutine(TickTimer).

- TickTimer: every 1s while !paused && !complete, elapsedSec++, timer.SetSeconds. After difficulty.TimeBonusSec, timer.PulseWarning(true).

- gridView.OnSelectionComplete = OnSelectionEnd: build word from cells, check forward + reversed against unfound placedWords. If match → MarkFound. Else → SoundManager.Play(MISS) + HapticManager.Light().

- MarkFound: append to foundWords. Pick highlight color from AppColors.HighlightColors[colorIndex++ % length]. For each cell, gridView.Tile(r,c).PlayFound(color). comboTracker.OnWordFound() → if >=2 comboBanner.Show. Find matching WordChipView in wordListContainer.GetComponentsInChildren and call MarkFound(color). SoundManager.Play(FOUND, pitch = 1 + (combo-1)*0.08f). HapticManager.Success(). Update progressLine "Found: X/Y". If foundWords.Count == placedWords.Count → Complete().

- UseHint: pick a random unfound placedWord, call gridView.Tile(pw.startRow, pw.startCol).PlayHintPulse(). hintsUsed++. SoundManager.Play(HINT).

- Pause: pauseDialog.gameObject.SetActive(true), paused=true. OnResume hides + paused=false. OnRestart hides + StartFresh. OnQuit → SaveAndQuit + back.

- SaveAndQuit: build a SavedGameState (gridRows = char[,] flattened to string[]), call ServiceLocator.GameState.Save, then ServiceLocator.Router.Show(Routes.LevelSelect).

- BackBtn click → SaveAndQuit.

- Complete: stop timer, compute stars via ScoreCalculator.ComputeStars, xp via ComputeXp. Build LevelResult. ServiceLocator.Progress.RecordLevelResult. Clear saved state. ServiceLocator.GameState.Delete. Show LevelCompleteView (stashes the result in a static LevelCompleteView.PendingResult and calls Router.Show(Routes.LevelComplete)). SoundManager.Play(WIN). HapticManager.Win().

- OrientationWatcher hook: subscribe to OnOrientationChanged in OnEnable, unsubscribe in OnDisable. true → layoutPortrait.SetActive(true), layoutLandscape.SetActive(false); false → vice versa. Both layouts have their own copy of the GameGridView/word list refs? Simplest: don't dual-mount; the layout containers hold layout-specific anchor configs only, and the gridView lives in one shared parent that re-parents itself. Acceptable shortcut: one layout group active at a time, content is just re-anchored. Document the choice in code comments.
```

**Acceptance:** Compiles. You won't be able to fully play-test until LevelComplete and prefab wiring is done.

#### Task 9.4 [CLAUDE] — LevelCompleteView + DailyChallengeView + ProfileView

**Prompt:**
```
Three remaining screens per docs/UNITY_PORT_GUIDE.md §13.6, §13.7 and spec §9.6, §9.7, §9.8:

1. Assets/Scripts/UI/Screens/LevelCompleteView.cs — static LevelResult PendingResult. OnEnable consumes PendingResult: animate three star slots with stagger per port guide §14.3 (DOVirtual.DelayedCall). Buttons: NextLevel → GameView.Open(topicId, levelId+1) and Router.Show(Game); Replay → GameView.Open(topicId, levelId) and Router.Show(Game); Topics → Router.Show(TopicList). If levelId == 30, hide NextLevel button.

2. Assets/Scripts/UI/Screens/DailyChallengeView.cs — OnEnable: read DailyChallengeStorage. If no challenge for today (today's date != stored.date), generate one: pick random unlocked topic, difficulty=MEDIUM, seed = long.Parse(today.ToString("yyyyMMdd")). Display topic name + date + streak. PlayBtn → set GameView.OverrideDifficulty = MEDIUM and GameView.OverrideSeed = challenge.seed before Open. Backbtn → Home.
   (You'll need to add OverrideDifficulty/OverrideSeed nullable fields to GameView for the daily flow — apply in StartFresh when set.)

3. Assets/Scripts/UI/Screens/ProfileView.cs — read ProgressStorage. Display: avatar emoji (pick from ⭐🦊🐼🦄🐯 cycled by totalXp/500), totalStars, totalXp, levels completed, topics mastered (count of topics where every level has stars==3), streakDays. Reset Progress button shows confirm child → on confirm: ServiceLocator.Progress.ClearAll(). BackBtn → Home.
```

**Acceptance:** Compiles. Full screen set in place.

#### Task 9.5 [USER] — Wire references in the Editor

Follow §C.4 — drag prefab refs into each Panel's view component via the Inspector. ~45 min — but it's the gate to actually playing the game.

---

### Phase 10 — Verify acceptance in Editor

#### Task 10.1 [USER] — Editor playtest

Run the manual QA checklist from `docs/UNITY_PORT_GUIDE.md` §21.3 (mirrors spec §16.3). For each item, check ✅ in this doc or open a Claude prompt to fix.

- [ ] Tutorial fires only on first launch (then `settings.tutorialShown == true`)
- [ ] Pause stops timer; Resume picks up at the same elapsed
- [ ] Quit mid-level → return to level → ResumeDialog shows
- [ ] All 22 topics render with correct emoji + accent
- [ ] Locked topics show 🔒 + requirement text
- [ ] Reset progress → home stats zero out
- [ ] Portrait ↔ Landscape mid-game preserves grid + found words
- [ ] APK builds and installs (Task 11.1)

#### Task 10.2 [CLAUDE] — Fix what's broken

When a checklist item fails, run:
```
In the Editor I observed the following bug: <paste behavior, console errors, screenshots if available>.
Investigate the relevant code in Assets/Scripts/<area>/ and propose a fix. Do not refactor unrelated code.
```

---

### Phase 11 — Android build

#### Task 11.1 [USER] — Build APK

Follow §C.5 step by step.

#### Task 11.2 [USER] — Install + smoke test

1. Connect Android device with USB debugging on (Developer Options → USB Debugging).
2. In a PowerShell window: `adb install -r C:\Github\AlphaRush\Builds\AlphaRush.apk`. If `adb` isn't on PATH, full path is `C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`.
3. Open AlphaRush on the device. Tutorial → first level of Animals → drag-select CAT → see it found.

---

## §C. Editor Walkthroughs (for [USER] tasks)

### §C.1 Build the two scenes (Phase 6)

#### C.1.1 Delete the SampleScene

1. In the Project window, navigate to `Assets/Scenes/`.
2. Right-click `SampleScene.unity` → **Delete**. Confirm.

#### C.1.2 Create Bootstrap.unity

1. **File → New Scene** → pick **Basic (Built-in)** template → click Create.
2. **File → Save As** → save to `Assets/Scenes/Bootstrap.unity`.
3. In the Hierarchy: right-click → **Create Empty** → rename to **ServicesRoot**.
4. With ServicesRoot selected, in the Inspector click **Add Component** → search **AppBootstrap** → add.
5. Add Component again → **SoundManager**.
6. Add Component again → **MusicManager**.

That's it for Bootstrap. Save (**Ctrl+S**).

#### C.1.3 Create Main.unity

1. **File → New Scene** → **Basic (Built-in)** → Create.
2. **File → Save As** → `Assets/Scenes/Main.unity`.
3. In Hierarchy, ensure there's a **Main Camera** (default) and an **EventSystem** (if not, right-click → UI → Event System).
4. Right-click in Hierarchy → **UI → Canvas**. Select the Canvas:
   - Render Mode = **Screen Space - Overlay**
   - UI Scale Mode (in Canvas Scaler) = **Scale With Screen Size**
   - Reference Resolution = **1080 × 1920**
   - Match = **0.5**
5. Inside Canvas, right-click → **Create Empty** → rename **SafeArea**. Add Component → **SafeAreaFitter**. Set its RectTransform anchors to stretch (anchorMin 0,0; anchorMax 1,1; offsets 0).
6. Inside SafeArea, right-click → **UI → Image** → rename **BackgroundGradient**. Stretch its RectTransform to fill SafeArea. Add Component → **GradientBackground**.
7. Inside SafeArea, create nine empty child GameObjects (one per main panel), each named **Panel_Splash**, **Panel_Home**, **Panel_TopicList**, **Panel_LevelSelect**, **Panel_Game**, **Panel_DailyChallenge**, **Panel_Profile**, **Panel_Settings**, **Panel_LevelComplete**. Each should be a full-screen RectTransform (stretch anchors, zero offsets). Disable (uncheck the tickbox at top of Inspector) all of them except **Panel_Splash**.
8. On the Canvas GameObject itself, Add Component → **PanelRouter**. In the Inspector:
   - **mainPanels** → set size 9 → drag each Panel_* in order
   - **panelNames** → set size 9 → match the const strings in Routes.cs: "Splash", "Home", "TopicList", "LevelSelect", "Game", "Daily", "Profile", "Settings", "LevelComplete"
9. Save (Ctrl+S).

#### C.1.4 Build Settings

1. **File → Build Settings**.
2. Click **Add Open Scenes** with Bootstrap open — it lands at index 0.
3. Open Main scene from Project window, then click **Add Open Scenes** again — index 1.
4. Switch Platform to **Android** (one-time, ~3 min).
5. Close.

#### C.1.5 Smoke test

Press **Ctrl+P** in the Bootstrap scene. The Console should not show errors. After a moment Main loads, you see a gradient background and Panel_Splash GameObject — but no SplashView script is on it yet, so nothing animates. That's fine — Phase 9.1 fixes that.

Press Ctrl+P again to exit Play mode.

---

### §C.2 Build TileView prefab (Task 7.2)

1. In Project window, navigate to `Assets/Prefabs/Tiles/`.
2. Open **Main** scene. In Hierarchy under Canvas → SafeArea → Panel_Game (temporarily enable Panel_Game for this), right-click → **UI → Image** → rename **TileView**. RectTransform: width 100, height 100.
3. Set its Image **Source Image** to **UISprite** (default knob sprite is fine), **Color** to white.
4. Add Component → **TileView**.
5. As children of TileView, create:
   - **Highlight** — UI → Image, white sprite, color = yellow with alpha 0. Stretch anchors.
   - **FoundOverlay** — UI → Image, white sprite, color = white alpha 0. Stretch anchors. Above Highlight in sibling order.
   - **Letter** — UI → Text - TextMeshPro (Unity will prompt to import TMP Essentials the first time — click Import). Alignment center, font size 48, default font.
6. On the TileView GameObject, drag the children into the script slots:
   - **Background** → the TileView's own Image
   - **Highlight** → child Highlight's Image
   - **FoundOverlay** → child FoundOverlay's Image
   - **Letter** → child Letter's TMP_Text
7. Drag the TileView from Hierarchy into `Assets/Prefabs/Tiles/`. Choose **Original Prefab**. Then delete the in-scene instance from the Hierarchy.
8. Re-disable Panel_Game.

---

### §C.3 Build small prefabs (Task 8.2)

For each script in Task 8.1, build a similar minimal prefab under `Assets/Prefabs/UI/`. Keep them ugly — placeholders.

Recipe per prefab:
1. Create a Canvas-child Image as the root (stretch + tinted color so you can see it).
2. Add Component → the matching script (e.g. WordChipView).
3. Add child TMP_Text(s) and child Images to match the script's serialized fields.
4. Drag the children into the script's Inspector slots.
5. Drag the root into `Assets/Prefabs/UI/<Name>.prefab`.
6. Delete the scene instance.

Specifics:

- **WordChip** — root Image (100x32, rounded if you have a sprite). Child: TMP_Text label, center-aligned, font 18.
- **TopicCard** — root Image (160x120). Children: TMP_Text iconLabel (top, font 48), TMP_Text nameLabel (font 18), TMP_Text starsLabel (font 14), TMP_Text requirementLabel (font 12), Image accentStrip (top edge, 6px), GameObject lockOverlay (full-overlay Image with low alpha and a 🔒 TMP_Text). Add Component → Button (root).
- **LevelDot** — root Image circle (UISprite knob, color tinted). Children: TMP_Text levelNumberLabel (center, font 18), GameObject lockIcon (TMP_Text 🔒), three small Image starSlots horizontally below the dot, Image accentRing as a stretched child behind the main circle. Add Button.
- **StatChip** — root Image (rounded). Children: TMP_Text iconLabel (font 24), valueLabel (20), captionLabel (12).
- **TimerView** — root empty. Child: TMP_Text label (font 24).
- **ComboBanner** — root Image full-width strip at top, anchor top-center. Child: TMP_Text bannerText (font 32, bold).
- **PauseDialog** — root Image overlay full-screen (semi-transparent black). Child Panel (centered card 400x300) with three Buttons: Resume, Restart, Quit. Each Button has child TMP_Text.
- **ResumeDialog** — same pattern, two Buttons.
- **TutorialOverlay** — root Image overlay. Four child Step panels each with a TMP_Text title + body and a Next Button. Skip Button always visible.

---

### §C.4 Wire references in the Editor (Task 9.5)

For each `Panel_*` under Canvas/SafeArea:

1. Select the panel.
2. Add Component → the matching View script (e.g. `Panel_Home` gets `HomeView`).
3. In the Inspector, drag prefab instances under the panel for each required UI element, then drag those instances into the script's serialized slots.

The pattern for **Panel_Game** is the biggest:
- Inside Panel_Game, create two empty children **Layout_Portrait** and **Layout_Landscape**.
- Inside Layout_Portrait, create child empty objects for: GridContainer (will hold GameGridView), WordListContainer (with a `FlowLayoutGroup` or a manual horizontal+vertical group), Header, ActionRow.
  - On GridContainer, add Component → **GameGridView**. Drag the `TileView` prefab into its `tilePrefab` slot. Set its `GridLayoutGroup` cell size leave default (the script overrides at runtime).
- Drop a `WordChipView` prefab into WordListContainer as the template — Claude's `wordChipPrefab` field on GameView wants the *prefab asset* (drag from Project, not from Hierarchy).
- Instantiate one **TimerView** prefab under Header. Drag into GameView's `timer` field.
- Add Buttons for hint/restart/pause/back as direct children. Drag into GameView's fields.
- Drop **ComboBanner**, **PauseDialog**, **ResumeDialog**, **TutorialOverlay** prefabs as children of Panel_Game; disable them. Drag into GameView's fields.
- On Panel_Game itself, Add Component → **OrientationWatcher**. Drag Panel_Game into GameView's `orientationWatcher` field, plus Layout_Portrait and Layout_Landscape into the two layout-group fields.

Repeat the simpler pattern for HomeView, TopicListView, LevelSelectView, etc.

---

### §C.5 Android build (Task 11.1)

1. **File → Build Settings → Android**. Ensure Bootstrap is index 0, Main is index 1.
2. Click **Player Settings…** at bottom — re-verify the values from Task 0.1.
3. Back in Build Settings, set **Texture Compression** = **ASTC**.
4. Click **Build** → choose `C:\Github\AlphaRush\Builds\` (create folder if needed) → filename `AlphaRush.apk`. Hit Save.
5. Wait 5–15 min for first build (IL2CPP). Subsequent builds are faster.
6. APK lands at `C:\Github\AlphaRush\Builds\AlphaRush.apk`. Check size — target ≤ 35 MB per spec.

---

## §D. After v1

Once §B Phase 11 passes, the game meets spec §19 acceptance criteria. Out-of-scope follow-ons (port guide §24):

- Real art via the AI pipeline in port guide §18.
- SFX/BGM from freesound.org → drop into `Assets/Audio/`, assign to `SoundManager.clips` array in the Inspector.
- TMP font assets (Lilita One, Fredoka) baked via `Window → TextMeshPro → Font Asset Creator`.
- Polish §23 from spec: combo banners with screen shake, full particle juice, Lottie animations.
- Badges (spec §11) — wire `GameEvents.BadgeEarned` to the matching conditions, render in `ProfileView`.
- Backend integration: flip `AppConfig.UseRemoteContent = true` and implement `RemoteContentDataSource` per spec §22.

---

**End of tasklist.** Work top-down. Each prompt under **Prompt:** is self-contained — Claude Code, running fresh in this repo, has the two docs + this tasklist + the prompt to do its job.
