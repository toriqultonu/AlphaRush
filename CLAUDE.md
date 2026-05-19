# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository status

This is a **fresh Unity 6 LTS (6000.4.7f1) bootstrap** — no game code exists yet. `Assets/` contains only the URP 2D template (default `SampleScene`, URP settings, Input System actions). All work is greenfield against the two design docs in `docs/`.

> Note: the global `~/.claude/CLAUDE.md` describes an Android-Kotlin port of AlphaRush living at `~/AndroidStudioProjects/alpharush`. **That is a different repo.** This repo is the Unity port. Treat the Kotlin project as a reference implementation only — code here is C# / Unity.

## Source-of-truth docs

Read these before touching code. Order of authority:

1. **`docs/ALPHARUSH_SPEC.md`** — game design, content, mechanics, palette, content JSON shape, score/combo rules, acceptance criteria. Authoritative for *what* the game does.
2. **`docs/UNITY_PORT_GUIDE.md`** — Unity-specific build plan (package list, folder layout, target script structure, scene plan, build settings, milestone list). Authoritative for *how* to assemble it in Unity 6 + URP 2D.

If the two conflict, the spec wins on design and content; the port guide wins on Unity mechanics.

## Target architecture (per port guide §2, §5–§13)

Two scenes — `Bootstrap.unity` (entry, loads Main) and `Main.unity` (all gameplay via swapped UI panels under a single Canvas managed by a `PanelRouter`). No additive scene loading.

Layering, target:

- **Pure logic** (`Scripts/Game/`) — `GridGenerator`, `SelectionEngine`, `ScoreCalculator`, `ComboTracker`. No `UnityEngine` dependencies; portable from the Kotlin reference; covered by EditMode tests. **Port these first.**
- **Models + Constants** (`Scripts/Model/`, `Scripts/Constants/`) — plain C# data; `AppColors`, `AppConfig`, `AppDimensions`, `AppStrings`.
- **Data** (`Scripts/Data/`) — `ContentDataSource` interface; `LocalContentDataSource` reads `Assets/StreamingAssets/topics.json`; `RemoteContentDataSource` is a stub gated by `AppConfig.UseRemoteContent`. `ContentRepository` is the only consumer the UI sees.
- **Persistence** (`Scripts/Data/Storage/`) — `ProgressStorage`, `SettingsStorage`, `GameStateStorage`, `DailyChallengeStorage`. Replaces Android DataStore with `PlayerPrefs` + JSON blobs (Newtonsoft.Json).
- **Audio/Haptics** (`Scripts/Audio/`, `Scripts/Haptic/`) — `SoundManager` (AudioSource pool indexed by `SoundEvent` enum) + `MusicManager` (AudioMixer with exposed `MusicVolume` / `SfxVolume`). Haptics via `AndroidJavaObject` Vibrator (no-op in editor).
- **UI** (`Scripts/UI/`) — UGUI panels (`Panel_Splash`, `Panel_Home`, `Panel_TopicList`, `Panel_LevelSelect`, `Panel_Game`, …) plus reusable views (`TileView`, `GameGridView`, `WordChip`, `TopicCard`, `LevelDot`). DOTween for tweens; Shuriken for particles.

DI: manual composition root (`AppBootstrap` MonoBehaviour in Bootstrap scene). No Zenject / VContainer.

## Platform & build constraints (locked)

| | |
|---|---|
| Engine | Unity 6 LTS, URP 2D |
| Platform | Android only, min API 24, target API 34 |
| Scripting | IL2CPP, .NET Standard 2.1, ARM64 |
| Orientation | Portrait + Landscape (both); mid-game rotation must preserve state |
| Connectivity | Fully offline. No ads, IAP, or analytics in v1 |
| APK budget | ≤ 35 MB total (art ≤ 8, audio ≤ 5, fonts ≤ 2) |
| Perf floor | 30 fps on Pixel 4a-class device |

Package name: `online.alpharush`. Company: TechnoNext.

## Required packages not yet installed

`Packages/manifest.json` currently has only the Unity 6 URP 2D template defaults. The port guide §1.3 requires these additions before substantive work:

- `com.unity.nuget.newtonsoft-json` (JSON serialization — required by storage layer)
- `com.unity.cinemachine`
- `com.unity.textmeshpro` (UGUI text)
- **DOTween** (HOTween v2) from the Asset Store — not a UPM package; import then run `Tools → Demigiant → DOTween Utility Panel → Setup DOTween`.

Flag this to the user before adding code that imports `Newtonsoft.Json`, `DG.Tweening`, etc.

## Common commands

This is a Unity project — most operations run in the Editor GUI, not on the CLI. Useful commands:

```bash
# Open the project in the installed Editor (adjust path)
/opt/Unity/Hub/Editor/6000.4.7f1/Editor/Unity -projectPath /home/technonext/AlphaRush

# Headless EditMode test run (after Test Assembly exists at Assets/Tests/EditMode)
Unity -batchmode -nographics -projectPath /home/technonext/AlphaRush \
  -runTests -testPlatform EditMode \
  -testResults /tmp/editmode-results.xml -logFile /tmp/unity.log -quit

# Install a built APK
adb install -r /path/to/alpharush.apk
```

Single-test filter for EditMode: append `-testFilter "Namespace.ClassName.MethodName"` to the `-runTests` invocation.

There is no Gradle wrapper in-repo; Android builds go through Unity's Build Settings (port guide §20).

## Working with this repo

- **Port pure logic first**, with EditMode tests, before any UI work. The port guide milestone table (§22) is the intended ordering; deviate only with reason.
- **Content lives in JSON**, not code. Copy `topics.json` (spec §7.1, 22 topics) into `Assets/StreamingAssets/` — do **not** hardcode topics into C#.
- **Determinism matters.** Grid generation uses `seed = topicId.GetHashCode() * 31 + levelId`. Acceptance criterion #4 — same seed must produce identical grid. Don't introduce `Random.Range` without an explicit seeded `System.Random`.
- **Two orientations, one scene.** Landscape isn't a separate scene — it's a layout variant driven by an `OrientationWatcher` swapping `RectTransform` anchors. Don't design portrait-only layouts.
- **Don't commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`** — already in `.gitignore` (standard Unity template). Do commit `.meta` files alongside their assets; missing meta files corrupt the Asset Database.
- **Currently dirty files** (`Assets/Settings/UniversalRP.asset`, `ProjectSettings/*.asset`) are baseline Unity-generated churn from first project open; safe to commit as the initial state.
