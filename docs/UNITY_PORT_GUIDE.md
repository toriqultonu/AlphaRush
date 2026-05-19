# AlphaRush — Unity Port Guide

**Source spec:** `docs/ALPHARUSH_SPEC.md` (authoritative for game design, content, mechanics).
**This doc:** complete instructions to rebuild AlphaRush in Unity with Candy-Crush-style polish.
**Target:** Android only, portrait + landscape, free assets, AI-generated art, word-search mechanic.

---

## 0. Scope & Decisions (locked)

| Decision | Value |
|---|---|
| Engine | Unity 6 LTS (6000.x) |
| Render pipeline | URP 2D |
| Language | C# (.NET Standard 2.1) |
| Platforms | Android only (min API 24, target API 34, IL2CPP, ARM64) |
| Orientation | Portrait + Landscape (both) |
| UI system | UGUI (Canvas + RectTransform) |
| Tweens | DOTween (free) |
| JSON | Newtonsoft.Json (com.unity.nuget.newtonsoft-json) |
| Audio | AudioSource + AudioMixer (built-in) |
| Particles | Shuriken (built-in) |
| Fonts | TextMeshPro (built-in) |
| Haptics | Native `AndroidJavaObject` Vibrator |
| Cost | Free assets only |
| Art workflow | AI gen (SDXL/Flux) → Krita polish → Sprite Atlas |
| Mechanic | Word-search (lift from spec §8) |

---

## 1. Project Bootstrap

### 1.1 Create Project

1. Unity Hub → New Project → Universal 2D template (6 LTS).
2. Project name: `AlphaRushUnity`.
3. Location: outside `alpharush/` directory (Unity creates large `Library/` folder).

### 1.2 Player Settings (Edit → Project Settings → Player)

```
Company Name:           TechnoNext
Product Name:           AlphaRush
Package Name:           online.alpharush
Version:                1.0.0
Bundle Version Code:    1

Resolution and Presentation:
  Default Orientation:  Auto Rotation
  Allowed Orientations: Portrait ✓
                        Portrait Upside Down ✓
                        Landscape Right ✓
                        Landscape Left ✓
  Use 32-bit Display Buffer: ✓

Other Settings:
  Color Space:          Linear
  Auto Graphics API:    OFF
  Graphics APIs:        Vulkan, OpenGLES3
  Scripting Backend:    IL2CPP
  Api Compatibility:    .NET Standard 2.1
  Target Architectures: ARM64 only
  Minimum API Level:    Android 7.0 (API 24)
  Target API Level:     Android 14 (API 34)

Publishing Settings:
  Custom Main Manifest: ✓ (for VIBRATE permission)
  Minify Release:       ✓ (R8)
```

### 1.3 Required Packages (Window → Package Manager)

Install:
- `com.unity.textmeshpro` (TextMeshPro)
- `com.unity.cinemachine` (Cinemachine)
- `com.unity.nuget.newtonsoft-json` (JSON)
- `com.unity.inputsystem` (New Input System) — set Active Input Handling to "Both"

From Asset Store (free):
- **DOTween (HOTween v2)** by Demigiant — import, then run `Tools → Demigiant → DOTween Utility Panel → Setup DOTween`.

### 1.4 URP 2D Renderer

1. Assets → Create → Rendering → URP → 2D Renderer.
2. Project Settings → Graphics → Scriptable Render Pipeline Settings → assign URP 2D asset.
3. Project Settings → Quality → assign for all tiers.
4. Enable Post-processing on Main Camera (for bloom on found-word cells).

### 1.5 Android Manifest (`Assets/Plugins/Android/AndroidManifest.xml`)

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    xmlns:tools="http://schemas.android.com/tools">
    <uses-permission android:name="android.permission.VIBRATE" />
    <application android:allowBackup="true" tools:replace="android:allowBackup">
        <activity android:name="com.unity3d.player.UnityPlayerActivity"
                  android:configChanges="orientation|screenSize|screenLayout|keyboardHidden|keyboard|smallestScreenSize|uiMode"
                  android:screenOrientation="fullSensor"
                  android:exported="true">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>
```

---

## 2. Folder Structure

Create exactly:

```
Assets/
├── Art/
│   ├── Tiles/              (tile sprites, 1 neutral + variants if needed)
│   ├── UI/                 (buttons, cards, frames — 9-slice)
│   ├── Backgrounds/        (gradient bases, sparkle layer)
│   ├── Icons/              (topic icons if not emoji)
│   ├── Badges/             (achievement icons)
│   └── Particles/          (textures for confetti, star burst, sparkle)
├── Audio/
│   ├── SFX/                (.ogg sound effects)
│   └── Music/              (.ogg BGM loops)
├── Fonts/                  (Lilita One, Fredoka, Baloo 2 TMP assets)
├── Prefabs/
│   ├── Tiles/              (TileView.prefab)
│   ├── UI/                 (WordChip, TopicCard, LevelDot, dialogs)
│   └── VFX/                (ParticleSystem prefabs)
├── Scenes/
│   ├── Bootstrap.unity     (entry, loads Main)
│   └── Main.unity          (all gameplay screens via UI panels)
├── Scripts/
│   ├── Constants/
│   │   ├── AppColors.cs
│   │   ├── AppConfig.cs
│   │   ├── AppDimensions.cs
│   │   └── AppStrings.cs
│   ├── Model/
│   │   ├── Difficulty.cs
│   │   ├── WordDirection.cs
│   │   ├── Topic.cs
│   │   ├── Level.cs
│   │   ├── CellSelection.cs
│   │   ├── PlacedWord.cs
│   │   ├── FoundWord.cs
│   │   ├── SavedGameState.cs
│   │   ├── LevelResult.cs
│   │   ├── PlayerProgress.cs
│   │   └── DailyChallenge.cs
│   ├── Data/
│   │   ├── IContentDataSource.cs
│   │   ├── LocalContentDataSource.cs
│   │   ├── RemoteContentDataSource.cs   (stub)
│   │   ├── ContentRepository.cs
│   │   ├── ProgressStorage.cs
│   │   ├── SettingsStorage.cs
│   │   ├── GameStateStorage.cs
│   │   └── DailyChallengeStorage.cs
│   ├── Game/
│   │   ├── GridGenerator.cs
│   │   ├── SelectionEngine.cs
│   │   ├── ScoreCalculator.cs
│   │   └── ComboTracker.cs
│   ├── Audio/
│   │   ├── SoundEvent.cs
│   │   ├── SoundManager.cs
│   │   └── MusicManager.cs
│   ├── Haptic/
│   │   └── HapticManager.cs
│   ├── Effects/
│   │   ├── ScreenShake.cs
│   │   ├── ParticleSpawner.cs
│   │   └── ConfettiBurst.cs
│   ├── UI/
│   │   ├── Core/
│   │   │   ├── SafeAreaFitter.cs
│   │   │   ├── OrientationWatcher.cs
│   │   │   ├── PanelRouter.cs
│   │   │   └── GradientBackground.cs
│   │   ├── Screens/
│   │   │   ├── SplashView.cs
│   │   │   ├── HomeView.cs
│   │   │   ├── TopicListView.cs
│   │   │   ├── LevelSelectView.cs
│   │   │   ├── GameView.cs
│   │   │   ├── LevelCompleteView.cs
│   │   │   ├── DailyChallengeView.cs
│   │   │   ├── ProfileView.cs
│   │   │   └── SettingsView.cs
│   │   └── Components/
│   │       ├── TileView.cs
│   │       ├── GameGridView.cs
│   │       ├── WordChipView.cs
│   │       ├── TopicCardView.cs
│   │       ├── LevelDotView.cs
│   │       ├── StatChipView.cs
│   │       ├── TimerView.cs
│   │       ├── ComboBannerView.cs
│   │       ├── PauseDialog.cs
│   │       ├── ResumeDialog.cs
│   │       └── TutorialOverlay.cs
│   └── Core/
│       ├── AppBootstrap.cs
│       ├── GameEvents.cs
│       └── ServiceLocator.cs
├── StreamingAssets/
│   └── data/
│       └── topics.json     (copy from Android assets verbatim)
└── Settings/
    ├── URP-Renderer2D.asset
    └── AudioMixer.mixer
```

---

## 3. Constants (port from spec §3, §7)

### 3.1 `AppColors.cs`

```csharp
using UnityEngine;

public static class AppColors {
    public static readonly Color FunPurple = Hex("#6200EE");
    public static readonly Color FunBlue   = Hex("#2196F3");
    public static readonly Color FunGreen  = Hex("#4CAF50");
    public static readonly Color FunOrange = Hex("#FF9800");
    public static readonly Color FunPink   = Hex("#E91E63");
    public static readonly Color FunYellow = Hex("#FFEB3B");
    public static readonly Color FunTeal   = Hex("#009688");
    public static readonly Color FunRed    = Hex("#F44336");

    public static readonly Color BackgroundLight  = Hex("#FFF8E1");
    public static readonly Color BackgroundMedium = Hex("#E3F2FD");
    public static readonly Color CardBackground   = Color.white;

    public static readonly Color[] HighlightColors = {
        Hex("#FFEB3B"), Hex("#90CAF9"), Hex("#F48FB1"), Hex("#A5D6A7"),
        Hex("#CE93D8"), Hex("#FFCC80"), Hex("#80DEEA"), Hex("#FFAB91"),
        Hex("#B39DDB"), Hex("#FFF59D")
    };

    static Color Hex(string hex) {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
```

### 3.2 `AppConfig.cs`

```csharp
public static class AppConfig {
    public const int LevelsPerTopic = 30;
    public const int MaxStarsPerLevel = 3;
    public const long ComboWindowMs = 3500;
    public const int MaxActiveParticles = 192;
    public const int MaxSoundStreams = 8;
    public const float HintRevealDurationSec = 1.5f;
    public const bool UseRemoteContent = false; // flip to true when backend ready
}
```

### 3.3 `AppDimensions.cs`

```csharp
public static class AppDimensions {
    public const float CardRadiusSmall = 16f;
    public const float CardRadiusLarge = 20f;
    public const float ButtonRadius    = 12f;
    public const float CardElevation   = 4f;
    public const float TopicPressScale = 0.95f;
}
```

---

## 4. Models (port from spec §6)

All `[Serializable]` for JSON.

### 4.1 `Difficulty.cs`

```csharp
[System.Serializable]
public enum Difficulty { EASY, MEDIUM, HARD, EXPERT }

public static class DifficultyExt {
    public static int GridSize(this Difficulty d) => d switch {
        Difficulty.EASY => 8, Difficulty.MEDIUM => 10,
        Difficulty.HARD => 12, Difficulty.EXPERT => 14, _ => 10
    };
    public static int MaxWords(this Difficulty d) => d switch {
        Difficulty.EASY => 5, Difficulty.MEDIUM => 8,
        Difficulty.HARD => 10, Difficulty.EXPERT => 12, _ => 8
    };
    public static int TimeBonusSec(this Difficulty d) => d switch {
        Difficulty.EASY => 60, Difficulty.MEDIUM => 90,
        Difficulty.HARD => 150, Difficulty.EXPERT => 240, _ => 90
    };
}
```

### 4.2 `WordDirection.cs`

```csharp
[System.Serializable]
public enum WordDirection {
    HORIZONTAL, HORIZONTAL_REVERSE,
    VERTICAL, VERTICAL_REVERSE,
    DIAGONAL_DOWN, DIAGONAL_DOWN_REVERSE,
    DIAGONAL_UP, DIAGONAL_UP_REVERSE
}
```

### 4.3 Other models

```csharp
[System.Serializable]
public class Topic {
    public string id;
    public string name;
    public string icon;           // emoji
    public long accentColor;      // 0xAARRGGBB
    public List<string> wordPool;
    public int unlockStarsRequired;
}

[System.Serializable]
public class Level {
    public int id;
    public string topicId;
    public Difficulty difficulty;
    public int targetWordCount;
    public long seed;
}

[System.Serializable]
public class CellSelection { public int row, col; }

[System.Serializable]
public class PlacedWord {
    public string word;
    public int startRow, startCol;
    public WordDirection direction;
}

[System.Serializable]
public class FoundWord {
    public string word;
    public int startRow, startCol, endRow, endCol;
    public long colorPacked; // store as 0xAARRGGBB
}

[System.Serializable]
public class SavedGameState {
    public string topicId;
    public int levelId;
    public string[] gridRows;       // row strings (CharArray flattened)
    public List<PlacedWord> placedWords;
    public List<string> foundWords;
    public int elapsedSeconds;
    public int hintsUsed;
    public int colorIndex;
}

[System.Serializable]
public class LevelResult {
    public string topicId;
    public int levelId;
    public int stars;
    public int timeSeconds;
    public int xpEarned;
    public int hintsUsed;
    public long completedAt;
}

[System.Serializable]
public class PlayerProgress {
    public int totalStars;
    public int totalXp;
    public int streakDays;
    public long lastPlayedEpochDay;
    public List<string> unlockedTopicIds;
    public Dictionary<string, LevelResult> bestResults;
    public List<string> badges;
}

[System.Serializable]
public class DailyChallenge {
    public string date;
    public string topicId;
    public Difficulty difficulty;
    public long seed;
    public bool completed;
    public int stars;
}
```

---

## 5. Pure-Logic Port (priority — port verbatim from Kotlin)

These are the highest-value files. They're pure math, no Unity dependencies. Port first, unit-test in EditMode.

### 5.1 `GridGenerator.cs` (spec §8.1)

```csharp
using System;
using System.Collections.Generic;

public static class GridGenerator {
    public class Result {
        public char[,] Grid;
        public List<PlacedWord> Placed;
    }

    static readonly WordDirection[] AllDirs = (WordDirection[])Enum.GetValues(typeof(WordDirection));

    public static Result Generate(int size, List<string> words, Difficulty diff, long seed) {
        var rng = new System.Random((int)(seed ^ (seed >> 32)));
        var dirs = AllowedDirections(diff);
        Result best = null;

        var sorted = new List<string>(words);
        sorted.Sort((a, b) => b.Length.CompareTo(a.Length));

        for (int attempt = 0; attempt < 50; attempt++) {
            var grid = new char[size, size];
            for (int r = 0; r < size; r++) for (int c = 0; c < size; c++) grid[r, c] = ' ';
            var placed = new List<PlacedWord>();

            foreach (var word in sorted) {
                if (!TryPlace(grid, word, dirs, size, rng, placed)) continue;
            }

            if (best == null || placed.Count > best.Placed.Count)
                best = new Result { Grid = (char[,])grid.Clone(), Placed = new List<PlacedWord>(placed) };

            if (placed.Count == sorted.Count) break;
        }

        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                if (best.Grid[r, c] == ' ')
                    best.Grid[r, c] = (char)('A' + rng.Next(26));

        return best;
    }

    static bool TryPlace(char[,] grid, string word, WordDirection[] dirs, int size,
                         System.Random rng, List<PlacedWord> placed) {
        for (int i = 0; i < 200; i++) {
            var dir = dirs[rng.Next(dirs.Length)];
            int r = rng.Next(size), c = rng.Next(size);
            if (TryWrite(grid, word, r, c, dir, size)) {
                placed.Add(new PlacedWord { word = word, startRow = r, startCol = c, direction = dir });
                return true;
            }
        }
        // systematic scan fallback
        foreach (var dir in dirs)
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    if (TryWrite(grid, word, r, c, dir, size)) {
                        placed.Add(new PlacedWord { word = word, startRow = r, startCol = c, direction = dir });
                        return true;
                    }
        return false;
    }

    static bool TryWrite(char[,] grid, string word, int r, int c, WordDirection dir, int size) {
        var (dr, dc) = DirToDelta(dir);
        int endR = r + dr * (word.Length - 1);
        int endC = c + dc * (word.Length - 1);
        if (endR < 0 || endR >= size || endC < 0 || endC >= size) return false;
        for (int i = 0; i < word.Length; i++) {
            int rr = r + dr * i, cc = c + dc * i;
            if (grid[rr, cc] != ' ' && grid[rr, cc] != word[i]) return false;
        }
        for (int i = 0; i < word.Length; i++) {
            int rr = r + dr * i, cc = c + dc * i;
            grid[rr, cc] = word[i];
        }
        return true;
    }

    public static (int dr, int dc) DirToDelta(WordDirection dir) => dir switch {
        WordDirection.HORIZONTAL              => (0, 1),
        WordDirection.HORIZONTAL_REVERSE      => (0, -1),
        WordDirection.VERTICAL                => (1, 0),
        WordDirection.VERTICAL_REVERSE        => (-1, 0),
        WordDirection.DIAGONAL_DOWN           => (1, 1),
        WordDirection.DIAGONAL_DOWN_REVERSE   => (-1, -1),
        WordDirection.DIAGONAL_UP             => (-1, 1),
        WordDirection.DIAGONAL_UP_REVERSE     => (1, -1),
        _ => (0, 1)
    };

    static WordDirection[] AllowedDirections(Difficulty d) => d switch {
        Difficulty.EASY => new[] { WordDirection.HORIZONTAL, WordDirection.VERTICAL },
        Difficulty.MEDIUM => new[] {
            WordDirection.HORIZONTAL, WordDirection.VERTICAL,
            WordDirection.DIAGONAL_DOWN, WordDirection.DIAGONAL_UP
        },
        _ => AllDirs
    };
}
```

### 5.2 `SelectionEngine.cs` (spec §8.2)

```csharp
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
```

### 5.3 `ScoreCalculator.cs` (spec §8.4)

```csharp
public static class ScoreCalculator {
    public static int ComputeStars(int elapsedSec, Difficulty diff, int hintsUsed) {
        float budget = diff.TimeBonusSec();
        float ratio = elapsedSec / budget;
        int baseStars = ratio <= 0.5f ? 3 : ratio <= 0.8f ? 2 : 1;
        return System.Math.Max(1, baseStars - hintsUsed);
    }

    public static int ComputeXp(int stars, Difficulty diff, int words) {
        int mult = diff switch {
            Difficulty.EASY => 10, Difficulty.MEDIUM => 18,
            Difficulty.HARD => 28, Difficulty.EXPERT => 42, _ => 18
        };
        return stars * mult + words * 2;
    }
}
```

### 5.4 `ComboTracker.cs` (spec §23.4)

```csharp
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
```

---

## 6. Content Source

### 6.1 Copy `topics.json`

Copy `app/src/main/assets/data/topics.json` from Android project to `Assets/StreamingAssets/data/topics.json`. Same shape, no edits.

### 6.2 `LocalContentDataSource.cs`

```csharp
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LocalContentDataSource : IContentDataSource {
    List<Topic> cached;

    public async System.Threading.Tasks.Task<List<Topic>> LoadTopicsAsync() {
        if (cached != null) return cached;
        string path = Path.Combine(Application.streamingAssetsPath, "data/topics.json");
        string json;
        if (path.Contains("://")) {
            using var req = UnityWebRequest.Get(path);
            var op = req.SendWebRequest();
            while (!op.isDone) await System.Threading.Tasks.Task.Yield();
            json = req.downloadHandler.text;
        } else {
            json = File.ReadAllText(path);
        }
        cached = JsonConvert.DeserializeObject<List<Topic>>(json);
        return cached;
    }

    public List<Level> GenerateLevels(string topicId) {
        var levels = new List<Level>(AppConfig.LevelsPerTopic);
        for (int i = 1; i <= AppConfig.LevelsPerTopic; i++) {
            Difficulty diff = i <= 8 ? Difficulty.EASY
                            : i <= 18 ? Difficulty.MEDIUM
                            : i <= 26 ? Difficulty.HARD
                            : Difficulty.EXPERT;
            int target = TargetWordCount(diff, i);
            long seed = ((long)topicId.GetHashCode() * 31) + i;
            levels.Add(new Level { id = i, topicId = topicId, difficulty = diff, targetWordCount = target, seed = seed });
        }
        return levels;
    }

    static int TargetWordCount(Difficulty d, int levelInTopic) => d switch {
        Difficulty.EASY   => 3 + ((levelInTopic - 1) / 2),         // 3 → 5
        Difficulty.MEDIUM => 5 + ((levelInTopic - 9) / 3),         // 5 → 8
        Difficulty.HARD   => 8 + ((levelInTopic - 19) / 3),        // 8 → 10
        Difficulty.EXPERT => 10 + ((levelInTopic - 27) / 2),       // 10 → 12
        _ => 5
    };
}
```

### 6.3 `ContentRepository.cs`

Thin wrapper exposing `Task<List<Topic>>`, `Task<Level>`, `Task<List<string>>` (words for level).

### 6.4 `RemoteContentDataSource.cs` (stub)

Implement `IContentDataSource` returning `NotImplementedException`. Flip `AppConfig.UseRemoteContent` later to swap.

---

## 7. Persistence (replaces DataStore)

Single approach: JSON files in `Application.persistentDataPath`.

### 7.1 `ProgressStorage.cs`

```csharp
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ProgressStorage {
    string FilePath => Path.Combine(Application.persistentDataPath, "progress.json");
    PlayerProgress cached;

    public PlayerProgress Load() {
        if (cached != null) return cached;
        if (!File.Exists(FilePath)) {
            cached = new PlayerProgress {
                unlockedTopicIds = new() { "animals", "fruits", "colors", "family" },
                bestResults = new(),
                badges = new()
            };
            return cached;
        }
        cached = JsonConvert.DeserializeObject<PlayerProgress>(File.ReadAllText(FilePath));
        return cached;
    }

    public void Save(PlayerProgress p) {
        cached = p;
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(p, Formatting.Indented));
    }

    public void RecordLevelResult(LevelResult r) {
        var p = Load();
        string key = $"{r.topicId}:{r.levelId}";
        if (!p.bestResults.TryGetValue(key, out var prev) || r.stars > prev.stars
            || (r.stars == prev.stars && r.timeSeconds < prev.timeSeconds)) {
            p.bestResults[key] = r;
        }
        p.totalStars = 0;
        foreach (var v in p.bestResults.Values) p.totalStars += v.stars;
        p.totalXp += r.xpEarned;
        Save(p);
    }

    public void UnlockBadge(string id) {
        var p = Load();
        if (!p.badges.Contains(id)) { p.badges.Add(id); Save(p); }
    }

    public void ClearAll() {
        if (File.Exists(FilePath)) File.Delete(FilePath);
        cached = null;
    }
}
```

### 7.2 `SettingsStorage.cs`

```csharp
[System.Serializable]
public class Settings {
    public bool soundEnabled = true;
    public bool hapticsEnabled = true;
    public bool tutorialShown = false;
    public float musicVolume = 0.6f;
    public float sfxVolume = 1.0f;
    public bool reduceMotion = false;
    public bool heartsEnabled = false;
}
```

Same file-based load/save pattern. Path: `Application.persistentDataPath/settings.json`.

### 7.3 `GameStateStorage.cs`

Saves per `(topicId, levelId)` as JSON keyed in single `saved_games.json` map: `Dictionary<string, SavedGameState>`. Key = `"topicId:levelId"`.

### 7.4 `DailyChallengeStorage.cs`

Single file `daily.json` with current `DailyChallenge` + streak counter.

---

## 8. Audio

### 8.1 AudioMixer Setup

1. Assets → Create → Audio Mixer → `AudioMixer.mixer`.
2. Groups: `Master` → `Music`, `SFX`.
3. Expose volumes: right-click each group's Volume → Expose → rename to `MusicVolume`, `SfxVolume`.
4. Use `mixer.SetFloat("MusicVolume", Mathf.Log10(v) * 20)` for slider-to-dB.

### 8.2 `SoundEvent.cs`

```csharp
public enum SoundEvent {
    TAP, SELECT, FOUND, COMBO, MISS, HINT, PAUSE,
    STAR_POP, WIN, LOSE, UNLOCK, BUTTON
}
```

### 8.3 `SoundManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour {
    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField] AudioClip[] clips; // index matches SoundEvent enum order
    AudioSource[] pool;
    int next;
    bool enabled = true;

    void Awake() {
        pool = new AudioSource[AppConfig.MaxSoundStreams];
        for (int i = 0; i < pool.Length; i++) {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            pool[i] = src;
        }
    }

    public void SetEnabled(bool v) => enabled = v;

    public void Play(SoundEvent e, float pitch = 1f, float volume = 1f) {
        if (!enabled) return;
        var clip = clips[(int)e];
        if (clip == null) return;
        var src = pool[next];
        next = (next + 1) % pool.Length;
        src.clip = clip;
        src.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        src.volume = Mathf.Clamp01(volume);
        src.Play();
    }
}
```

Combo pitch escalation: `Play(SoundEvent.FOUND, pitch: 1f + (combo - 1) * 0.08f)`.

### 8.4 `MusicManager.cs`

Two `AudioSource` (A/B) crossfade via `DOTween.To` on `volume`. 400 ms fade. One active at a time.

### 8.5 SFX/BGM Sourcing (free)

- **freesound.org** (filter CC0 license).
- **Pixabay** music + SFX (royalty-free).
- **Kenney.nl** free game audio packs.
- **opengameart.org**.

Convert to `.ogg` (Audacity). Place in `Assets/Audio/SFX/` and `Assets/Audio/Music/`. In Unity Import Settings: Load Type = Decompress on Load (SFX) / Streaming (BGM). Force Mono on SFX.

---

## 9. Haptics

### 9.1 `HapticManager.cs`

```csharp
using UnityEngine;

public static class HapticManager {
#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject vibrator;
    static void EnsureInit() {
        if (vibrator != null) return;
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
    }
#endif

    public static bool Enabled { get; set; } = true;

    public static void Tick()    => Vibrate(8);
    public static void Light()   => Vibrate(12);
    public static void Success() => VibratePattern(new long[] { 0, 20, 40, 30 });
    public static void Win()     => VibratePattern(new long[] { 0, 40, 80, 40, 80, 80 });
    public static void Lose()    => VibratePattern(new long[] { 0, 60, 60, 60 });

    public static void Combo(int level) {
        long[] pattern = level switch {
            1 => new long[] { 0, 15 },
            2 => new long[] { 0, 15, 30, 25 },
            3 => new long[] { 0, 20, 30, 30, 30, 35 },
            4 => new long[] { 0, 25, 30, 35, 30, 40, 30, 45 },
            _ => new long[] { 0, 30, 25, 40, 25, 50, 25, 60, 25, 70 }
        };
        VibratePattern(pattern);
    }

    static void Vibrate(long ms) {
        if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        EnsureInit();
        vibrator?.Call("vibrate", ms);
#endif
    }

    static void VibratePattern(long[] pattern) {
        if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        EnsureInit();
        vibrator?.Call("vibrate", pattern, -1);
#endif
    }
}
```

---

## 10. Scene Setup

### 10.1 `Bootstrap.unity`

Empty scene. Single `GameObject` with `AppBootstrap.cs` that:
1. Spawns `DontDestroyOnLoad` services: `SoundManager`, `MusicManager`, `ProgressStorage`, `SettingsStorage`, `ContentRepository`.
2. Preloads `topics.json`.
3. Loads `Main.unity` additively.

### 10.2 `Main.unity` Hierarchy

```
Main
├── Camera (URP, Orthographic, post-processing on)
├── EventSystem
├── ServicesRoot (DontDestroyOnLoad targets)
└── UICanvas (Screen Space - Overlay)
    ├── SafeArea (RectTransform + SafeAreaFitter.cs)
    │   ├── BackgroundGradient (Image, gradient material)
    │   ├── Panel_Splash
    │   ├── Panel_Home
    │   ├── Panel_TopicList
    │   ├── Panel_LevelSelect
    │   ├── Panel_Game
    │   │   ├── Layout_Portrait
    │   │   └── Layout_Landscape
    │   ├── Panel_LevelComplete  (modal)
    │   ├── Panel_DailyChallenge
    │   ├── Panel_Profile
    │   ├── Panel_Settings
    │   └── Overlays
    │       ├── PauseDialog
    │       ├── ResumeDialog
    │       ├── Tutorial
    │       └── ComboBanner
    └── ParticleStage (world-space layer above UI for VFX)
```

`PanelRouter.cs` toggles `SetActive` on panels — only one main panel active at a time, overlays additive.

### 10.3 `SafeAreaFitter.cs`

```csharp
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour {
    Rect lastSafeArea;
    Vector2Int lastScreen;

    void Update() {
        var safe = Screen.safeArea;
        var screen = new Vector2Int(Screen.width, Screen.height);
        if (safe == lastSafeArea && screen == lastScreen) return;
        lastSafeArea = safe; lastScreen = screen;
        Apply(safe);
    }

    void Apply(Rect safe) {
        var rt = (RectTransform)transform;
        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= Screen.width;  anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;  anchorMax.y /= Screen.height;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
    }
}
```

### 10.4 `OrientationWatcher.cs`

```csharp
using UnityEngine;
using UnityEngine.Events;

public class OrientationWatcher : MonoBehaviour {
    public UnityEvent<bool> OnOrientationChanged; // true = portrait
    bool wasPortrait;

    void Start() { wasPortrait = Screen.height >= Screen.width; OnOrientationChanged?.Invoke(wasPortrait); }

    void Update() {
        bool portrait = Screen.height >= Screen.width;
        if (portrait != wasPortrait) {
            wasPortrait = portrait;
            OnOrientationChanged?.Invoke(portrait);
        }
    }
}
```

Wire to `Panel_Game.GameView` → toggles `Layout_Portrait` / `Layout_Landscape`.

---

## 11. Game Grid UI

### 11.1 `TileView.prefab`

Hierarchy:
```
TileView (RectTransform, ~100x100, TileView.cs)
├── Background (Image, tile_idle sprite)
├── Highlight   (Image, tile_glow sprite, alpha 0)
├── Letter      (TextMeshProUGUI, font Lilita One, outline + drop shadow)
└── FoundOverlay (Image, alpha 0, color = highlight)
```

### 11.2 `TileView.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TileView : MonoBehaviour {
    [SerializeField] Image background;
    [SerializeField] Image highlight;
    [SerializeField] Image foundOverlay;
    [SerializeField] TMP_Text letter;

    public int Row, Col;

    public void Set(char c, int row, int col) {
        Row = row; Col = col;
        letter.text = c.ToString();
    }

    public void SetSelected(bool on) {
        highlight.DOFade(on ? 1f : 0f, 0.1f);
        transform.DOScale(on ? 1.08f : 1f, 0.1f);
    }

    public void PlayFound(Color tint) {
        foundOverlay.color = new Color(tint.r, tint.g, tint.b, 0f);
        foundOverlay.DOFade(0.85f, 0.18f);
        transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 6, 0.5f);
    }

    public void PlayHintPulse() {
        var seq = DOTween.Sequence();
        seq.Append(highlight.DOFade(1f, 0.2f));
        seq.Append(highlight.DOFade(0f, 0.2f));
        seq.SetLoops(3);
    }

    public void Reset() {
        highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0f);
        foundOverlay.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;
    }
}
```

### 11.3 `GameGridView.cs`

Drives input. Uses `GraphicRaycaster` + `IPointerDownHandler` / `IDragHandler` / `IPointerUpHandler` on grid root. On each pointer move, raycast to find current `TileView`, build selection path, validate via `SelectionEngine`. On up, check word match.

```csharp
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
```

Use a `UILineRenderer` (free Unity-UI-Extensions package, or write small `Graphic`-derived line) to draw the selection trail.

---

## 12. ViewModels (replaces Compose VM)

Each screen has a `MonoBehaviour` view + plain C# state class. Keep state in view (Unity-friendly) but separate game state into POCOs.

### 12.1 `GameView.cs` outline

```csharp
public class GameView : MonoBehaviour {
    [SerializeField] GameGridView gridView;
    [SerializeField] WordListView wordList;
    [SerializeField] TimerView timer;
    [SerializeField] TMP_Text headerTopic, progressLine;
    [SerializeField] Button hintBtn, restartBtn, pauseBtn, backBtn;
    [SerializeField] ComboBannerView comboBanner;
    [SerializeField] GameObject layoutPortrait, layoutLandscape;

    string topicId; int levelId;
    Topic topic; Level level;
    List<PlacedWord> placedWords;
    List<FoundWord> foundWords = new();
    char[,] grid;
    int elapsedSec, hintsUsed, colorIndex;
    bool paused, complete;
    ComboTracker combo = new(AppConfig.ComboWindowMs);

    public async void Open(string topicId, int levelId) {
        // load topic, level, generate grid, attach gridView callback
        // check saved game → show ResumeDialog if present
        gridView.OnSelectionComplete = OnSelectionEnd;
        gridView.Build(grid);
        StartCoroutine(TickTimer());
    }

    void OnSelectionEnd(List<CellSelection> cells) {
        string word = BuildString(cells);
        string reversed = Reverse(word);
        foreach (var pw in placedWords) {
            if (foundWords.Exists(f => f.word == pw.word)) continue;
            if (pw.word == word || pw.word == reversed) { MarkFound(pw, cells); return; }
        }
        // miss → SFX + haptic
        ServiceLocator.Sound.Play(SoundEvent.MISS);
        HapticManager.Light();
    }

    void MarkFound(PlacedWord pw, List<CellSelection> cells) {
        var color = AppColors.HighlightColors[colorIndex++ % AppColors.HighlightColors.Length];
        foundWords.Add(new FoundWord { word = pw.word,
            startRow = cells[0].row, startCol = cells[0].col,
            endRow = cells[^1].row, endCol = cells[^1].col,
            colorPacked = PackColor(color)
        });
        foreach (var c in cells) gridView.Tile(c.row, c.col).PlayFound(color);
        int comboLvl = combo.OnWordFound();
        ServiceLocator.Sound.Play(SoundEvent.FOUND, pitch: 1f + (comboLvl - 1) * 0.08f);
        HapticManager.Success();
        if (comboLvl >= 2) comboBanner.Show(comboLvl);
        wordList.MarkFound(pw.word, color);
        UpdateProgress();
        if (foundWords.Count == placedWords.Count) Complete();
    }

    // ...UseHint, Pause, Restart, SaveAndQuit, Complete...
}
```

Fill in remaining methods following spec §8.5–8.7, §9.5.

---

## 13. Screen Specs (port spec §9 to UGUI)

### 13.1 Splash

`Panel_Splash`: centered TMP "AlphaRush" + emoji prefixes/suffixes. 1.2 s `DOTween` scale+fade, then `PanelRouter.Show("Home")`.

### 13.2 Home

White rounded `StatsCard` (4 `StatChipView`: Topics, Levels, Stars, Streak). Three large buttons: Play, Daily, Profile. Settings gear bottom-right.

### 13.3 TopicList

`GridLayoutGroup` 2 columns, `TopicCardView` per topic. Lock overlay + unlock-requirement text on locked.

### 13.4 LevelSelect

30 `LevelDotView` in zig-zag — implement via custom layout: `GridLayoutGroup` 3 cols with alternating-row horizontal offset, or compute positions on a Bezier path. Show 0–3 stars + lock/check ring.

### 13.5 Game

Already covered §11–§12. Two layouts (portrait/landscape) per §1.

### 13.6 LevelComplete (modal sheet)

`Panel_LevelComplete` activated on completion. Three star slots animate sequentially with `DOPunchScale`. Buttons: Next Level, Replay, Topics.

### 13.7 DailyChallenge / Profile / Settings

Standard panels. Profile lists badges as `Badge` cells. Settings = sliders + toggles bound to `SettingsStorage`.

---

## 14. Animations (DOTween)

### 14.1 Topic card press

```csharp
GetComponent<Button>().onClick.AddListener(() => {
    transform.DOScale(0.95f, 0.08f).OnComplete(() =>
        transform.DOScale(1f, 0.12f).SetEase(Ease.OutBack));
});
```

### 14.2 Word chip on found

```csharp
chip.transform.DOScale(1.05f, 0.15f).SetLoops(2, LoopType.Yoyo);
chipImage.DOColor(highlightColor, 0.2f);
```

### 14.3 Star award (Level Complete)

```csharp
for (int i = 0; i < stars; i++) {
    int idx = i;
    DOVirtual.DelayedCall(idx * 0.2f, () => {
        var s = starSlots[idx];
        s.SetActive(true);
        s.transform.localScale = Vector3.zero;
        s.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutBack)
          .OnComplete(() => s.transform.DOScale(1f, 0.1f));
        ServiceLocator.Sound.Play(SoundEvent.STAR_POP);
    });
}
```

### 14.4 Hint reveal

```csharp
tile.PlayHintPulse(); // implemented in TileView.cs
```

### 14.5 Combo banner

```csharp
public void Show(int level) {
    banner.SetActive(true);
    bannerText.text = $"COMBO x{level}!";
    bannerText.color = level >= 5 ? Color.red : Color.yellow;
    var rt = bannerText.rectTransform;
    rt.localScale = Vector3.zero;
    rt.DOScale(1.15f, 0.18f).SetEase(Ease.OutBack)
      .OnComplete(() => rt.DOScale(1f, 0.12f));
    DOVirtual.DelayedCall(1.2f, () => banner.SetActive(false));
}
```

---

## 15. Particles (Shuriken)

Create as prefabs in `Assets/Prefabs/VFX/`.

### 15.1 `ConfettiBurst.prefab`

ParticleSystem settings:
- Duration: 3s, Loop: off
- Start Lifetime: 2–3s
- Start Speed: 6–12
- Start Size: 0.15–0.35
- Start Color: random from palette (use Color over Lifetime gradient cycling palette)
- Gravity Modifier: 0.6
- Emission: Burst 60 particles at t=0
- Shape: Cone, angle 45°, position above screen
- Renderer: Texture from `Art/Particles/confetti_strip.png` (4 frames as Texture Sheet Animation)

### 15.2 `StarBurst.prefab`

- Duration 0.6s, Burst 24, radial, gravity 0.2, star texture, color = injected via script.

### 15.3 `SparkleTrail.prefab`

Continuous trail: low emission rate (15/s), upward drift, small star sprite, alpha-over-lifetime fade. Spawn on selection tile move.

### 15.4 `ParticleSpawner.cs`

```csharp
public class ParticleSpawner : MonoBehaviour {
    [SerializeField] ParticleSystem confettiPrefab;
    [SerializeField] ParticleSystem starBurstPrefab;
    static int active = 0;

    public void SpawnStarBurst(Vector3 worldPos, Color tint) {
        if (active > AppConfig.MaxActiveParticles) return;
        var ps = Instantiate(starBurstPrefab, worldPos, Quaternion.identity);
        var main = ps.main; main.startColor = tint;
        ps.Play();
        active += (int)ps.emission.GetBurst(0).count.constant;
        Destroy(ps.gameObject, 1.5f);
        DOVirtual.DelayedCall(1.5f, () => active -= 24);
    }

    public void SpawnConfetti(Vector3 origin) {
        var ps = Instantiate(confettiPrefab, origin, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, 3.5f);
    }
}
```

Map cell row/col → world position using `RectTransformUtility.ScreenPointToWorldPointInRectangle` or use Canvas overlay particles.

---

## 16. Screen Shake

```csharp
using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour {
    [SerializeField] RectTransform target; // SafeArea
    public void Shake(float amplitude, float duration) {
        target.DOShakeAnchorPos(duration, new Vector2(amplitude, amplitude), 14, 90, false)
              .OnComplete(() => target.anchoredPosition = Vector2.zero);
    }
}
```

Triggers per spec §23.6: combo ≥ 5 → `(6, 0.25)`, level complete → `(4, 0.2)`, time-up → `(10, 0.4)`.

---

## 17. Event → Reaction Wiring (spec §23.8)

Central `GameEvents` static class with C# events. `GameView` raises events; `SoundManager`, `HapticManager`, `ParticleSpawner`, `ScreenShake`, `ComboBannerView` subscribe.

```csharp
public static class GameEvents {
    public static event System.Action<CellSelection> CellTouchDown;
    public static event System.Action<int> CellAddedToSelection; // arg: chain length
    public static event System.Action InvalidRelease;
    public static event System.Action<PlacedWord, int, Color> WordFound; // word, combo, color
    public static event System.Action<int> Combo;
    public static event System.Action HintUsed;
    public static event System.Action PauseOpened;
    public static event System.Action TimeWarning;
    public static event System.Action TimeUp;
    public static event System.Action<LevelResult> LevelComplete;
    public static event System.Action<string> TopicUnlocked;
    public static event System.Action<string> BadgeEarned;
    public static event System.Action DailyStreakIncrement;

    public static void Raise_CellTouchDown(CellSelection c) => CellTouchDown?.Invoke(c);
    // ...etc
}
```

Implement spec §23.8 matrix as subscribers.

---

## 18. Art Pipeline (AI Gen)

### 18.1 Tools

- **ComfyUI** + SDXL 1.0 or Flux.1-schnell (free local).
- **Krita** (free, https://krita.org).
- **rembg** (background removal) — Krita plugin or standalone.

### 18.2 Master Style Prompt

```
"glossy candy-style mobile game UI asset, [thing description],
chunky rounded edges, soft cel-shading, thick dark outline,
vibrant saturated colors, white highlight top-left corner,
clean transparent background, square 1024x1024,
centered subject, no text, no shadow on ground"
```

Append `--style raw` for SDXL or use Flux base.

### 18.3 Asset List

| Asset | Prompt suffix | Output count |
|---|---|---|
| Tile idle | "cream beige glossy square, rounded corners, subtle inner shadow" | 1 |
| Tile selected | "same tile glowing purple #6200EE outer ring" | 1 |
| Tile glow overlay | "soft white circular glow, transparent, additive" | 1 |
| Star empty | "outline gold star, glossy, big chunky" | 1 |
| Star filled | "filled gold star with gradient and highlight" | 1 |
| Board frame | "candy game border frame, repeating side, top-left top-right corners, 9-slice ready" | 4 (corners + edges) |
| Button primary | "purple glossy rounded button, soft 3D, ready for 9-slice" | 1 |
| Button secondary | "yellow glossy rounded button" | 1 |
| Button danger | "red glossy rounded button" | 1 |
| Badge ring | "circular medal frame, gold rope edge" | 1 |
| Confetti pieces | "small confetti strip, primary color, 4 frame sheet" | 1 sheet |
| Sparkle star | "small 4-point sparkle star, white center" | 1 |
| Splash logo | "AlphaRush wordmark, magnifier and target emoji prefix, chunky font" | 1 |
| BG gradient | not gen — use Unity gradient material | 0 |
| BG sparkle layer | "tileable star sparkle overlay, transparent, soft" | 1 |

### 18.4 Polish Steps Per Asset

1. Generate 4 SDXL variants per prompt (same seed for consistency within a set).
2. Pick best → upscale 2× via Real-ESRGAN.
3. Open in Krita → use Color → Color to Alpha, threshold ~10 (remove BG).
4. Manual cleanup: erase fringe with eraser, add inner shadow layer if missing.
5. Export 1024×1024 PNG (or 512×512 for tiles to save APK size).
6. Import in Unity → set Texture Type = Sprite, Compression = High Quality (ASTC 6×6), Generate Mip Maps = OFF for UI.
7. For 9-slice: Sprite Editor → set borders.
8. Add to relevant Sprite Atlas (Assets → Create → 2D → Sprite Atlas).

### 18.5 Color Variants (single sprite + tint)

Don't gen 8 hue variants of tiles. Gen one neutral tile. Tint via `Image.color` at runtime. Saves 8× memory.

For glow/highlight: use additive shader on UI Image → tint freely.

### 18.6 Topic Icons

Spec uses emojis. Two options:
- (a) Keep emoji — render via TMP with emoji-capable font (e.g., **Noto Color Emoji** via SDF). Free, zero AI work.
- (b) AI-gen each topic icon as flat sticker (22 prompts). Use sticker style for consistency.

Recommend (a) for v1.

---

## 19. Fonts

1. Download from Google Fonts (OFL license):
   - **Lilita One** (display, chunky)
   - **Fredoka** Regular + Bold (body)
   - **Baloo 2** Bold (Bangla fallback if needed later)
2. In Unity: Window → TextMeshPro → Font Asset Creator.
3. For each: Source Font File = .ttf, Atlas Resolution = 1024×1024, Character Set = Extended ASCII (or custom for Bangla).
4. Generate → Save in `Assets/Fonts/`.
5. Create TMP Material Presets for outlines and shadows (Lilita_Outline_White, Lilita_Outline_Black).

---

## 20. Build Settings

### 20.1 Build → Player Settings → Optimization

- Managed Stripping Level: Medium
- IL2CPP Code Generation: Faster (smaller) builds
- Strip Engine Code: ON

### 20.2 Texture Compression (Build Settings)

- Texture Compression: ASTC

### 20.3 Build APK

1. File → Build Settings → Android.
2. Add `Bootstrap.unity` (index 0) and `Main.unity` (index 1).
3. Build APK → install via `adb install -r alpharush.apk` for test.

### 20.4 Size Budget

| Component | Budget |
|---|---|
| Unity engine baseline | ~15 MB |
| Art atlases | ≤ 8 MB |
| Audio | ≤ 5 MB |
| Fonts | ≤ 2 MB |
| Scripts | ≤ 1 MB |
| **Total APK** | **≤ 35 MB** |

If over: drop tile resolution to 512×512, re-encode audio at 80 kbps.

---

## 21. Testing

### 21.1 EditMode Tests (`Assets/Tests/EditMode/`)

Window → General → Test Runner → EditMode → Create Test Assembly.

```csharp
using NUnit.Framework;

public class GridGeneratorTests {
    [Test]
    public void SameSeedProducesSameGrid() {
        var words = new List<string> { "CAT", "DOG", "BIRD" };
        var a = GridGenerator.Generate(10, words, Difficulty.MEDIUM, 12345);
        var b = GridGenerator.Generate(10, words, Difficulty.MEDIUM, 12345);
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                Assert.AreEqual(a.Grid[r, c], b.Grid[r, c]);
    }

    [Test]
    public void AllPlacedWordsRetrievable() {
        var words = new List<string> { "APPLE", "MANGO" };
        var res = GridGenerator.Generate(10, words, Difficulty.MEDIUM, 99);
        foreach (var pw in res.Placed) {
            var (dr, dc) = GridGenerator.DirToDelta(pw.direction);
            for (int i = 0; i < pw.word.Length; i++) {
                Assert.AreEqual(pw.word[i], res.Grid[pw.startRow + dr * i, pw.startCol + dc * i]);
            }
        }
    }
}

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

public class ScoreCalculatorTests {
    [Test]
    public void FastFinishGetsThreeStars() {
        Assert.AreEqual(3, ScoreCalculator.ComputeStars(20, Difficulty.MEDIUM, 0));
    }

    [Test]
    public void HintsReduceStars() {
        Assert.AreEqual(2, ScoreCalculator.ComputeStars(20, Difficulty.MEDIUM, 1));
    }

    [Test]
    public void NeverBelowOneStar() {
        Assert.AreEqual(1, ScoreCalculator.ComputeStars(500, Difficulty.EASY, 5));
    }
}
```

### 21.2 PlayMode Tests

UI interaction tests with `InputTestFixture`. Lower priority for v1.

### 21.3 Manual QA Checklist (port from spec §16.3)

- [ ] Tutorial fires only once
- [ ] Pause stops timer; resume continues exact elapsed
- [ ] Backing out mid-level saves state; reopen offers resume
- [ ] All 22 topics render correct emoji + accent
- [ ] Locked topics show 🔒 + unlock once met
- [ ] Daily challenge rolls over at local midnight
- [ ] Reset progress clears stars, XP, badges, saved games
- [ ] Portrait → Landscape mid-game preserves state
- [ ] Notch devices: nothing clipped (SafeArea works)
- [ ] APK size ≤ 35 MB
- [ ] 60 fps on midrange device, 30 fps floor on low-end

---

## 22. Development Milestones

| Day | Deliverable |
|---|---|
| 1 | Project setup, packages, folder structure, AppBootstrap, Bootstrap → Main load |
| 2 | Models + Constants. EditMode tests for `GridGenerator`, `SelectionEngine`, `ScoreCalculator`, `ComboTracker` — all green |
| 3 | `LocalContentDataSource` loads `topics.json`, generates levels. Repository in place |
| 4 | All 4 storage classes (`ProgressStorage`, `SettingsStorage`, `GameStateStorage`, `DailyChallengeStorage`) |
| 5 | Splash → Home → TopicList → LevelSelect screens with placeholder art. PanelRouter working |
| 6 | `TileView` + `GameGridView` build a grid from `GridGenerator.Result`. Selection drag works |
| 7 | Word matching, found-state visuals, word list, timer, hint, restart — full gameplay portrait |
| 8 | Landscape layout variant + `OrientationWatcher` |
| 9 | LevelCompleteSheet, PauseDialog, ResumeDialog, Tutorial |
| 10 | DailyChallenge + Profile + Settings panels |
| 11–13 | AI-gen art pass: all sprites, fonts, atlases. Replace placeholders |
| 14–16 | Juice: DOTween animations, particles, screen shake, combo banner, event matrix wired |
| 17 | Audio (SoundManager + MusicManager) + Haptics. SFX/BGM assets imported |
| 18 | Android build, device QA portrait + landscape on 3+ devices |
| 19–20 | Bugfix, optimize draw calls, APK shrink, release build |

---

## 23. Acceptance Criteria (Unity v1)

From spec §19 adapted to Unity:

1. App launches → splash → home in < 2s on midrange device.
2. All 22 topics render with correct emoji + accent color.
3. Locked topics unlock at correct cumulative star thresholds.
4. Level generation is deterministic — same seed produces identical grid.
5. Selection drag works smoothly in 8 directions per difficulty.
6. Saved game persists across app kill + relaunch.
7. Daily challenge rolls over correctly at local midnight.
8. Reset Progress clears all state.
9. Portrait + Landscape both supported; mid-game rotation preserves state.
10. SafeArea respected on notched devices.
11. APK ≤ 35 MB.
12. 30 fps floor on Pixel 4a or equivalent low-mid Android device.
13. Tutorial fires once on first game launch.
14. Combo system: pitch escalation + visual banner + screen shake at level 5.
15. Level complete: confetti + star sequence + WIN SFX + haptic.
16. Reduce Motion toggle disables shake, halves particles.

---

## 24. Future Work (post-v1)

- Lives system + boosters (spec §23.9).
- World-map LevelSelect with hand-drawn path (Bezier `LineRenderer`).
- Daily reward chain modal.
- Backend integration: flip `AppConfig.UseRemoteContent = true`, implement `RemoteContentDataSource` with `UnityWebRequest` per spec §22.
- Localization (Bangla — fallback font ready).
- Cloud save (Firebase or custom).

---

## 25. References

- **Source spec:** `docs/ALPHARUSH_SPEC.md` (always defer to it for content + mechanics).
- **DOTween docs:** http://dotween.demigiant.com/documentation.php
- **URP 2D:** https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/manual/2d-index.html
- **TextMeshPro:** https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest
- **Newtonsoft.Json for Unity:** https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@latest

---

**End of guide.** Start at §1, port pure-logic (§5) first, get unit tests green, then build UI. Defer all art polish to milestone 11+.
