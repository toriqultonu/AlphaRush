# AlphaRush — Word Search Game (Standalone Android App)

A standalone, kid-friendly word search puzzle game derived from the HaateKhori `WordSearch` feature. Same visual identity, more topics, more levels, structured difficulty progression, and richer meta-game (stars, XP, world map, daily challenge).

This document is a build spec — hand it to Claude Code (or any engineer) and the app should be implementable end-to-end without further questions.

---

## 1. Product Overview

- **Name:** AlphaRush
- **Tagline:** Find words. Beat the clock. Climb the alphabet.
- **Audience:** Kids 5–12 (and casual word-puzzle players of any age)
- **Genre:** Word Search / Educational Puzzle
- **Platform:** Android (Kotlin + Jetpack Compose, Material 3)
- **Min SDK:** 26 (Android 8.0) — broader reach than parent app
- **Target SDK:** 36 (Android 15)
- **Compile SDK:** 36
- **Orientation:** Portrait only (locked in manifest)
- **Connectivity:** Fully offline. No ads, no IAP, no analytics SDKs in v1.
- **Package name:** `online.alpharush`
- **Application ID:** `online.alpharush`

### Goals
1. Preserve the look & feel of the existing WordSearch screens (gradient bg, purple accents, rounded cards, emoji icons).
2. Add a real progression system: difficulty tiers, levels, stars, XP, unlocks.
3. Expand content: 20+ topics, 100+ levels across 4 difficulty tiers, daily challenge.
4. Keep the implementation small, single-module, no Room/Hilt — `SharedPreferences` + `kotlinx.serialization` (or `org.json`) for state.

### Non-goals (v1)
- No multiplayer.
- No cloud sync / accounts.
- No paid content.
- No localization beyond English (architecture should allow it later).

---

## 2. Tech Stack

| Concern | Choice |
|---|---|
| Language | Kotlin 2.0 |
| UI | Jetpack Compose (BOM latest stable) |
| Design system | Material 3 |
| Navigation | `androidx.navigation:navigation-compose` |
| Persistence | `DataStore Preferences` (reactive `Flow`s) + `org.json` for blob payloads (saved game state). Existing `SharedPreferences` data migrated via `SharedPreferencesMigration`. |
| Async | Kotlin Coroutines + `Flow` |
| Drawing | Compose `Canvas` (grid + selection trail + custom particle effects) |
| Animations | `androidx.compose.animation`, `animateFloatAsState`, `Animatable`, `Transition`, `rememberInfiniteTransition`, spring physics |
| Vector animation | **Lottie Compose** (`com.airbnb.android:lottie-compose`) — star bursts, badge unlocks, level intros, loading state |
| Image loading | **Coil** (`io.coil-kt:coil-compose`) — async topic art, profile avatars, future seasonal backgrounds |
| Audio | **SoundPool** for SFX (low latency, pooled, pitch/rate control); `MediaPlayer` for background music loops |
| Haptics | Compose `HapticFeedback` + `Vibrator` / `VibratorManager` with `VibrationEffect` for richer combo escalation patterns |
| Build | Gradle Kotlin DSL, version catalog (`libs.versions.toml`) |
| Lint/Format | Android Lint defaults; ktlint optional |

No Hilt, no Room, no Retrofit — keep dependencies minimal. The polish stack (Lottie, Coil, DataStore) is the only deliberate expansion beyond the original "minimal deps" goal because Candy Crush-grade juice is a v1 requirement.

---

## 3. Visual Design

The visual language must match the source WordSearch screens. Do not invent new colors or shapes — reuse the palette and shape tokens below.

### 3.1 Color Palette

```kotlin
// Primary / brand
val FunPurple = Color(0xFF6200EE)    // primary brand, used for headings & active states
val FunBlue   = Color(0xFF2196F3)
val FunGreen  = Color(0xFF4CAF50)    // success
val FunOrange = Color(0xFFFF9800)
val FunPink   = Color(0xFFE91E63)
val FunYellow = Color(0xFFFFEB3B)
val FunTeal   = Color(0xFF009688)
val FunRed    = Color(0xFFF44336)

// Backgrounds
val BackgroundLight  = Color(0xFFFFF8E1)  // warm cream
val BackgroundMedium = Color(0xFFE3F2FD)  // light blue
val CardBackground   = Color(0xFFFFFFFF)

// Word highlight palette (cycled as words are found)
val highlightColors = listOf(
    Color(0xFFFFEB3B), // Yellow
    Color(0xFF90CAF9), // Light Blue
    Color(0xFFF48FB1), // Pink
    Color(0xFFA5D6A7), // Light Green
    Color(0xFFCE93D8), // Purple
    Color(0xFFFFCC80), // Orange
    Color(0xFF80DEEA), // Cyan
    Color(0xFFFFAB91), // Deep Orange
    Color(0xFFB39DDB), // Lavender
    Color(0xFFFFF59D)  // Pale Yellow
)
```

### 3.2 Background Gradient

Every full-screen route uses the same vertical gradient as `WordSearchTopicScreen`:

```kotlin
Brush.verticalGradient(
    colors = listOf(BackgroundLight, BackgroundMedium)
)
```

### 3.3 Shapes & Elevation

- Cards: `RoundedCornerShape(16.dp)` (small), `RoundedCornerShape(20.dp)` (large)
- Chips: `RoundedCornerShape(20.dp)`
- Buttons: `RoundedCornerShape(12.dp)` or `24.dp` (pill)
- Card elevation: `4.dp` (resting), `6.dp` (interactive)
- Topic-card press scale: `0.95f` via `animateFloatAsState` + `MutableInteractionSource`

### 3.4 Typography

Reuse Compose defaults. Sizes used in source:

| Use | Size | Weight |
|---|---|---|
| Screen title | 26 sp | Bold |
| Topic name | 16 sp | Bold |
| Stat value | 20 sp | Bold |
| Body / info | 16 sp | Medium |
| Caption | 12 sp | Normal |
| Hand pointer emoji | 50 sp | — |
| Title emoji | 28–48 sp | — |

### 3.5 Iconography

Emojis (no custom drawables for topic icons). Map names → emoji at render time (`getTopicIcon`). Add new mappings as topics grow.

---

## 4. Module / Package Structure

Single `:app` module. Mirror the existing parent package layout, scoped to AlphaRush:

```
online.alpharush/
├── MainActivity.kt
├── AlphaRushApp.kt                     // root composable, hosts NavHost
├── constants/
│   ├── AppColors.kt
│   ├── AppDimensions.kt
│   ├── AppStrings.kt
│   └── AppConfig.kt                    // grid sizes, timer defaults, etc.
├── model/
│   ├── Difficulty.kt
│   ├── Topic.kt
│   ├── Level.kt
│   ├── WordDirection.kt
│   ├── CellSelection.kt
│   ├── FoundWord.kt
│   ├── PlacedWord.kt
│   ├── SavedGameState.kt
│   ├── PlayerProgress.kt
│   └── DailyChallenge.kt
├── content/
│   ├── Topics.kt                       // master topic list (20+)
│   ├── Levels.kt                       // level generator config per topic
│   └── Hints.kt                        // hint texts per topic (optional)
├── data/
│   ├── ProgressStorage.kt              // stars, XP, unlocks
│   ├── GameStateStorage.kt             // saved in-progress games per level
│   ├── SettingsStorage.kt              // sound, tutorial flag, theme prefs
│   └── DailyChallengeStorage.kt
├── game/
│   ├── GridGenerator.kt                // word placement engine
│   ├── SelectionEngine.kt              // valid-line check, cells-between
│   ├── ScoreCalculator.kt              // XP/star formula
│   └── ComboTracker.kt                 // streaks, multipliers, juice triggers
├── audio/
│   ├── SoundManager.kt                 // SoundPool wrapper, preload + play
│   ├── MusicManager.kt                 // MediaPlayer BGM with fade in/out
│   └── SoundEvent.kt                   // enum of SFX (TAP, SELECT, FOUND, COMBO, WIN, LOSE, STAR, UNLOCK)
├── haptic/
│   └── HapticManager.kt                // wraps HapticFeedback + Vibrator for rich patterns
├── effects/
│   ├── ParticleSystem.kt               // pooled particles for confetti, sparkles, trails
│   ├── ScreenShake.kt                  // Modifier-based shake controller
│   └── GlowOverlay.kt                  // edge glow / vignette pulses
├── navigation/
│   ├── Routes.kt
│   └── AppNavHost.kt
└── ui/
    ├── theme/
    │   ├── Color.kt
    │   ├── Theme.kt
    │   └── Type.kt
    ├── screens/
    │   ├── SplashScreen.kt
    │   ├── HomeScreen.kt
    │   ├── TopicListScreen.kt
    │   ├── LevelSelectScreen.kt        // parallax + animated path
    │   ├── GameScreen.kt
    │   ├── LevelIntroScreen.kt         // pre-game title card slide-in
    │   ├── LevelCompleteScreen.kt      // (dialog/sheet, not full screen)
    │   ├── DailyChallengeScreen.kt
    │   ├── DailyRewardScreen.kt        // 7-day rotating gift chain
    │   ├── ProfileScreen.kt
    │   └── SettingsScreen.kt
    └── components/
        ├── GradientBackground.kt
        ├── StatChip.kt
        ├── TopicCard.kt
        ├── LevelDot.kt                 // numbered, locked/unlocked/starred
        ├── WordChip.kt
        ├── GameGrid.kt                 // the Canvas + pointerInput
        ├── Timer.kt
        ├── HintButton.kt
        ├── PauseDialog.kt
        ├── ConfettiAnimation.kt
        ├── SparkleTrail.kt             // selection-drag sparkle particles
        ├── ComboBanner.kt              // "Combo x3!" floating text
        ├── StarBurstLottie.kt          // Lottie wrapper for star pop
        ├── BadgeUnlockLottie.kt        // Lottie wrapper for achievement reveal
        ├── LevelIntroCard.kt           // animated topic name + emoji on level start
        └── AlphaRushTutorial.kt        // rebrand of WordSearchTutorial
```

---

## 5. Navigation

```kotlin
sealed class Route(val path: String) {
    data object Splash      : Route("splash")
    data object Home        : Route("home")
    data object Topics      : Route("topics")
    data object Levels      : Route("levels/{topicId}") {
        fun build(topicId: String) = "levels/$topicId"
    }
    data object Game        : Route("game/{topicId}/{levelId}") {
        fun build(topicId: String, levelId: Int) = "game/$topicId/$levelId"
    }
    data object Daily       : Route("daily")
    data object Profile     : Route("profile")
    data object Settings    : Route("settings")
}
```

Use `composable(...)` with default fade-through transitions (`fadeIn(200) + fadeOut(200)`).

---

## 6. Data Models

```kotlin
enum class Difficulty(val gridSize: Int, val maxWords: Int, val timeBonusSec: Int) {
    EASY  (gridSize = 8,  maxWords = 5,  timeBonusSec = 60),
    MEDIUM(gridSize = 10, maxWords = 8,  timeBonusSec = 90),
    HARD  (gridSize = 12, maxWords = 10, timeBonusSec = 150),
    EXPERT(gridSize = 14, maxWords = 12, timeBonusSec = 240)
}

enum class WordDirection {
    HORIZONTAL,
    HORIZONTAL_REVERSE,
    VERTICAL,
    VERTICAL_REVERSE,
    DIAGONAL_DOWN,
    DIAGONAL_DOWN_REVERSE,
    DIAGONAL_UP,
    DIAGONAL_UP_REVERSE
}
// EASY uses only HORIZONTAL + VERTICAL.
// MEDIUM adds DIAGONAL_DOWN + DIAGONAL_UP.
// HARD adds all reverses.
// EXPERT adds reverses + smaller cell size + shorter timer.

data class Topic(
    val id: String,
    val name: String,
    val icon: String,             // emoji key, resolved via getTopicIcon
    val accentColor: Long,        // 0xAARRGGBB so it can be stored
    val wordPool: List<String>,   // ALL words available for this topic
    val unlockStarsRequired: Int  // 0 = free, otherwise total stars needed
)

data class Level(
    val id: Int,                  // 1..N within topic
    val topicId: String,
    val difficulty: Difficulty,
    val targetWordCount: Int,     // how many words to pick from pool
    val seed: Long                // deterministic word + placement selection
)

data class CellSelection(val row: Int, val col: Int)

data class PlacedWord(
    val word: String,
    val startRow: Int,
    val startCol: Int,
    val direction: WordDirection
)

data class FoundWord(
    val word: String,
    val startRow: Int,
    val startCol: Int,
    val endRow: Int,
    val endCol: Int,
    val color: Color
)

data class SavedGameState(
    val topicId: String,
    val levelId: Int,
    val grid: Array<CharArray>,
    val placedWords: List<PlacedWord>,
    val foundWords: List<String>,
    val elapsedSeconds: Int,
    val hintsUsed: Int,
    val colorIndex: Int
)

data class LevelResult(
    val topicId: String,
    val levelId: Int,
    val stars: Int,               // 0..3
    val timeSeconds: Int,
    val xpEarned: Int,
    val hintsUsed: Int,
    val completedAt: Long         // epoch ms
)

data class PlayerProgress(
    val totalStars: Int,
    val totalXp: Int,
    val streakDays: Int,
    val lastPlayedEpochDay: Long,
    val unlockedTopicIds: Set<String>,
    val bestResults: Map<String, LevelResult> // key = "$topicId:$levelId"
)

data class DailyChallenge(
    val date: String,             // ISO yyyy-MM-dd
    val topicId: String,
    val difficulty: Difficulty,
    val seed: Long,
    val completed: Boolean,
    val stars: Int
)
```

---

## 7. Content

### 7.1 Topics (≥ 20)

Each topic has a 12–25 word pool. Levels draw a subset based on difficulty.

| ID | Name | Icon | Accent | Sample words |
|---|---|---|---|---|
| `animals` | Animals | 🐾 | FunYellow | DOG, CAT, LION, BEAR, TIGER, ZEBRA, MONKEY, ELEPHANT, GIRAFFE, RABBIT, HORSE, COW, GOAT, FOX, WOLF, DEER, FROG, DUCK, OWL, BIRD |
| `fruits` | Fruits | 🍎 | FunRed | APPLE, MANGO, BANANA, GRAPE, ORANGE, PEACH, PLUM, PEAR, KIWI, LEMON, MELON, CHERRY, PAPAYA, GUAVA, BERRY |
| `vegetables` | Vegetables | 🥦 | FunGreen | CARROT, POTATO, ONION, TOMATO, GARLIC, GINGER, PEPPER, CABBAGE, LETTUCE, SPINACH, PEAS, BEAN, CORN, OKRA |
| `colors` | Colors | 🎨 | FunPurple | RED, BLUE, GREEN, YELLOW, PINK, BLACK, WHITE, ORANGE, BROWN, PURPLE, GRAY, CYAN |
| `body` | Body Parts | 🧍 | FunPink | HEAD, HAND, FOOT, NOSE, EYE, EAR, LEG, ARM, NECK, FACE, KNEE, LIP, CHIN, HAIR, TOE |
| `family` | Family | 👨‍👩‍👧 | FunTeal | MOM, DAD, BROTHER, SISTER, GRANDMA, GRANDPA, UNCLE, AUNT, COUSIN, BABY, FATHER, MOTHER |
| `vehicles` | Vehicles | 🚗 | FunOrange | CAR, BUS, BIKE, BOAT, TRAIN, PLANE, SHIP, TRUCK, JEEP, VAN, TAXI, SCOOTER, TRAM |
| `food` | Food | 🍕 | FunGreen | RICE, BREAD, FISH, MEAT, CAKE, PIZZA, SOUP, PASTA, BURGER, SALAD, NOODLE, EGG |
| `countries` | Countries | 🌍 | FunBlue | INDIA, CHINA, JAPAN, EGYPT, FRANCE, SPAIN, ITALY, BRAZIL, CANADA, KENYA, NEPAL, PERU |
| `cities` | Cities | 🏙️ | FunBlue | TOKYO, PARIS, ROME, LONDON, CAIRO, DELHI, DHAKA, LAGOS, LIMA, MIAMI, OSAKA |
| `sports` | Sports | ⚽ | FunOrange | SOCCER, TENNIS, GOLF, CRICKET, HOCKEY, RUGBY, BASEBALL, BOXING, CHESS, KARATE |
| `school` | School | 🎒 | FunPurple | BOOK, PEN, PENCIL, BAG, RULER, ERASER, DESK, CHAIR, BOARD, CLASS, TEACHER, STUDENT |
| `weather` | Weather | ⛅ | FunBlue | SUN, RAIN, SNOW, WIND, CLOUD, STORM, FOG, HEAT, COLD, ICE, FROST |
| `space` | Space | 🚀 | FunPurple | MOON, STAR, SUN, EARTH, MARS, VENUS, COMET, PLANET, ROCKET, ORBIT, GALAXY |
| `ocean` | Ocean | 🐠 | FunTeal | FISH, SHARK, WHALE, CORAL, CRAB, OCTOPUS, SHELL, WAVE, SEA, REEF, EEL, TUNA |
| `farm` | Farm | 🚜 | FunGreen | COW, HEN, GOAT, SHEEP, HORSE, PIG, DUCK, BARN, FARMER, TRACTOR, CROP, HAY |
| `clothes` | Clothes | 👕 | FunPink | SHIRT, PANTS, HAT, SOCKS, SHOES, COAT, SCARF, GLOVES, DRESS, SKIRT, JEANS |
| `instruments` | Instruments | 🎸 | FunOrange | DRUM, GUITAR, PIANO, FLUTE, VIOLIN, HARP, TRUMPET, CELLO, BANJO |
| `tools` | Tools | 🔧 | FunYellow | HAMMER, SAW, NAIL, DRILL, WRENCH, SCREW, RULER, TAPE, KNIFE, BRUSH |
| `bugs` | Bugs | 🐞 | FunGreen | ANT, BEE, FLY, MOTH, WASP, BEETLE, SPIDER, CRICKET, LADYBUG, MOSQUITO |
| `shapes` | Shapes | 🔷 | FunBlue | CIRCLE, SQUARE, TRIANGLE, OVAL, STAR, HEART, DIAMOND, HEXAGON, CUBE |
| `numbers` | Numbers (spelled) | 🔢 | FunPurple | ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN, ELEVEN, TWELVE |

Implementation: define each as a `Topic` in `content/Topics.kt`. Accent colors store as `Long` (0xAARRGGBB) so they survive serialization, then wrap with `Color(value)` at render time.

### 7.2 Levels

Generate **30 levels per topic = 660 total** for the v1 launch. Distribution:

| Levels | Difficulty |
|---|---|
| 1–8 | EASY |
| 9–18 | MEDIUM |
| 19–26 | HARD |
| 27–30 | EXPERT |

Per-level `targetWordCount` ramps inside each difficulty band (e.g. EASY: 3 → 5; MEDIUM: 5 → 8; HARD: 8 → 10; EXPERT: 10 → 12). Use `seed = (topicId.hashCode().toLong() * 31) + levelId` so word selection + placement is deterministic and replays are identical.

### 7.3 Topic Unlock Gating

- First 4 topics free (`unlockStarsRequired = 0`): animals, fruits, colors, family.
- Subsequent topics require cumulative stars: 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 115, 130, 150.
- Within a topic: level N+1 unlocks when level N is completed with ≥ 1 star.

### 7.4 Daily Challenge

- One per calendar day (local device date).
- Random topic from the player's unlocked set.
- Difficulty = MEDIUM (fixed for v1).
- Seed derived from date (`yyyyMMdd.toLong()`).
- Reward: 2× XP and 1 bonus star toward unlock budget (does not count toward per-level best results).
- Streak counter increments on consecutive days; resets on miss.

---

## 8. Game Mechanics

### 8.1 Grid Generation (`GridGenerator.kt`)

Lift directly from `WordSearchGameScreen.generateWordSearchGrid` and harden:

1. Sort words by length descending.
2. For each word, try 200 random `(direction, row, col)` placements; if all fail, fall back to systematic scan over all `(direction, row, col)`.
3. A placement is valid when every target cell is either empty or already holds the same letter (allow overlap, encourages crossings).
4. After placing all words, fill remaining `' '` cells with random `A`–`Z`.
5. If after 50 grid attempts not every word fits, return the best grid achieved (placed subset).
6. Direction set is filtered by `Difficulty`:
   - EASY → `HORIZONTAL`, `VERTICAL` only
   - MEDIUM → adds `DIAGONAL_DOWN`, `DIAGONAL_UP`
   - HARD/EXPERT → adds all `*_REVERSE` variants

Determinism: take a `Random(seed)` instance and thread it through. Do not call top-level `Random` inside the generator.

### 8.2 Selection (`SelectionEngine.kt`)

Reuse algorithms from `WordSearchGameScreen.kt`:

- `isValidSelection(start, current)`: same row, same col, or `|dRow| == |dCol|`.
- `getCellsBetween(start, end)`: walk one step at a time using sign-of-diff.
- Render a `Canvas` with `pointerInput`. On `awaitFirstDown` consume the event so the parent scroll cannot steal it (the source file already does this — keep that behavior).

### 8.3 Word Matching

On selection end:
1. Build `selectedWord` from `grid[r][c]` over the selection.
2. Also compute `reversedWord`.
3. If any placed word in `placedWords` (and not yet found) equals either, record a `FoundWord` with cycled `highlightColors[colorIndex % size]` and increment `colorIndex`.

### 8.4 Scoring (`ScoreCalculator.kt`)

```kotlin
fun computeStars(elapsedSec: Int, difficulty: Difficulty, hintsUsed: Int): Int {
    val budget = difficulty.timeBonusSec
    val timeRatio = elapsedSec.toFloat() / budget
    val baseStars = when {
        timeRatio <= 0.5f -> 3
        timeRatio <= 0.8f -> 2
        timeRatio <= 1.2f -> 1
        else              -> 1     // completing always earns at least 1
    }
    return (baseStars - hintsUsed).coerceAtLeast(1)
}

fun computeXp(stars: Int, difficulty: Difficulty, words: Int): Int {
    val multiplier = when (difficulty) {
        Difficulty.EASY   -> 10
        Difficulty.MEDIUM -> 18
        Difficulty.HARD   -> 28
        Difficulty.EXPERT -> 42
    }
    return stars * multiplier + words * 2
}
```

`PlayerProgress.totalStars` = sum of `max(stars)` across `bestResults`. Replays only overwrite if star count or time improves.

### 8.5 Hints

- Player has unlimited hints, each costs 1 potential star (floor at 1).
- Pressing the 💡 button reveals the first letter of any unfound word by briefly tinting that cell `FunYellow` for 1500 ms.
- Increments `hintsUsed`.

### 8.6 Pause & Resume

- Header pause icon → modal `PauseDialog` (Resume / Restart / Quit).
- Timer stops while paused.
- Quit serializes `SavedGameState` to prefs and exits to Level Select.
- Returning to the same level offers Resume vs Restart.

### 8.7 Completion Flow

- All target words found → `gameCompleted = true`.
- Stop timer, persist `LevelResult`, clear saved game, play confetti, then show `LevelCompleteScreen` bottom sheet with stars, time, XP, and "Next Level" / "Replay" / "Topics" buttons.

---

## 9. Screens — Detailed Specs

All screens use `GradientBackground` and standard 16 dp padding unless noted.

### 9.1 SplashScreen

- Centered logo (text "AlphaRush" in `FunPurple`, 48 sp Bold, with 🔍 left + 🎯 right).
- `LaunchedEffect` delays 1200 ms then navigates to Home.

### 9.2 HomeScreen

- Header row: 🔍 "AlphaRush" 🎯 (matches `WordSearchTopicScreen` header).
- Stats card (white, rounded 16 dp, 4 dp elevation): "Topics X/Y", "Levels A/B", "Stars ⭐ N", "Streak 🔥 D".
- Three primary cards (large, rounded 20 dp, 6 dp elevation):
  1. **Play** → Topics
  2. **Daily Challenge** → Daily (badge with today's date)
  3. **Profile** → Profile
- Footer mini row: Settings ⚙️ button.

### 9.3 TopicListScreen

- Header same pattern as `WordSearchTopicScreen`.
- Stats card showing total topics completed and total levels completed.
- `LazyVerticalGrid(columns = Fixed(2))`, spacing 12 dp.
- Each cell uses `TopicCard` (see §10.3). Locked topics show a 🔒 emoji overlay and the requirement text ("Earn 10⭐").

### 9.4 LevelSelectScreen

- Header with back arrow, topic emoji + name in topic accent color, total stars earned for this topic.
- A vertical scroll with a "world-map" feel: 30 `LevelDot`s arranged in a zig-zag using `LazyVerticalGrid(columns = Fixed(3))` with alternating row offsets, or a custom layout — pick whichever is simpler.
- Each `LevelDot` shows: level number, stars earned (0–3 small ⭐), and a state:
  - Locked → gray, 🔒 overlay, not clickable
  - Unlocked → topic accent color
  - Completed → green ring + stars
- Tapping a level → Game route.

### 9.5 GameScreen

Root: `Box` (fills screen, wraps `screenShake(ScreenShakeController)` modifier) containing a vertically scrollable `Column` over a `BackgroundLight → BackgroundMedium` vertical gradient. Overlays (combo banner, dialogs, confetti, complete sheet) sit on top of the column inside the same `Box`.

**Header `Row`** (vertically centered):
- `←` back `TextButton` (24.sp, `FunPurple`). On press calls `viewModel.saveAndQuit()` then `onBack()` — saves in-progress state so player can resume.
- Topic header `Column` (`weight(1f)`):
  - Line 1: `${topic.icon ?: "🔍"} ${topic.name}` — 22.sp bold, `FunPurple`.
  - Line 2: `"Level ${level.id} · Find all ${placedWords.size} words!"` — 13.sp gray.
- Timer `Card` (white, `RoundedCornerShape(12.dp)`): `⏱️` + `formatTime(elapsedSeconds)` as `m:ss`, 16.sp bold `FunPurple`.
- Pause `IconButton`: `⏸` glyph 20.sp → `viewModel.pause()`.

**Progress line:** `"Found: ${foundWords.size}/${placedWords.size}"` — 16.sp medium `FunGreen`.

**Word list `Card`** (white, `RoundedCornerShape(16.dp)`, 4.dp elevation): `FlowRow` of `WordChip`s, 8.dp horizontal + vertical spacing. Each chip rendered as `WordChip(word, found = foundWord != null, highlightColor = foundWord?.color ?: Color.LightGray)`. Found chips tinted with the highlight color assigned at find time.

**Game grid `Card`** (only when `state.grid.isNotEmpty()`): same white card style, inner `Box` with `aspectRatio(1f)` + 8.dp padding wrapping `GameGrid(grid, gridSize, foundWords, hintCell, onWordSelected)`. `gridSize` taken from `state.level?.difficulty?.gridSize ?: 10`. Cell font/stroke scaling lives inside `GameGrid`:
- 8 → fontSize = cellSize * 0.65f
- 10 → 0.60f (default)
- 12 → 0.50f
- 14 → 0.42f

When grid empty and `state.isLoading`: 320.dp-tall `Box` with centered `CircularProgressIndicator`.

**Action `Row`** (`SpaceEvenly`):
- `💡 Hint (${hintsUsed})` — `FunYellow`, `RoundedCornerShape(12.dp)`, black text → `viewModel.useHint()`.
- `🔄 Restart` — `FunOrange` with `Icons.Default.Refresh` + label → `viewModel.startFresh()`.

(No separate pause button in this row — pause lives in header.)

**Overlays inside the root `Box`:**
- `ComboBanner(combo = currentCombo, visible = showComboBanner)` — top-centered.
- Resume `AlertDialog` when `state.hasSavedGame`: title "Resume?" / text "You have a saved game for this level." / confirm → `resumeSavedGame()` / dismiss → `startFresh()`.
- `PauseDialog` when `state.isPaused` → `resume()` / `startFresh()` / `saveAndQuit()+onBack()`.
- `AlphaRushTutorial` when `state.showTutorial` → `viewModel.onTutorialDismissed()`.
- `LevelCompleteSheet` when `state.isComplete && state.levelResult != null` — params: `levelId`, `result`, `hasNextLevel = (level.id < 30)`, `onNextLevel`, `onReplay = startFresh()`, `onTopics`.
- `ConfettiAnimation(fillMaxSize)` while `state.isComplete`.

**Reactive effects (`LaunchedEffect`):**
- `currentCombo` ≥ 5 → `shakeController.shake(6f, 250ms)`.
- `isComplete` true → `shakeController.shake(4f, 200ms)`.

### 9.6 LevelCompleteScreen (bottom sheet)

- Big 🎉 (or 🌟) at top.
- Title "Level N Complete!" in `FunPurple`.
- Star row: three star slots, fill animated one by one with `animateFloatAsState` (scale 0 → 1, 250 ms staggered).
- Stats: time, XP gained, hints used.
- Buttons: **Next Level** (primary, `FunPurple`), **Replay**, **Topics**.

### 9.7 DailyChallengeScreen

- Header: 📅 "Daily Challenge", today's date, streak 🔥.
- Card showing today's topic + difficulty + reward.
- "Play" button → Game route with daily seed.
- After completion: show result with "Come back tomorrow!" hint.

### 9.8 ProfileScreen

- Avatar emoji (cycle ⭐🦊🐼🦄🐯 by total XP tier).
- Total Stars, Total XP, Levels Completed, Topics Mastered (all-3-star levels), Streak.
- Trophy row (badges from §11).
- "Reset Progress" link → confirm dialog → `ProgressStorage.clearAll()`.

### 9.9 SettingsScreen

Toggles:
- Sound effects (default on)
- Haptic feedback (default on)
- Reset tutorial flag
- Reset all progress (confirmation)
- About — version, link to source repo (placeholder)

---

## 10. Reusable Components

### 10.1 GradientBackground

```kotlin
@Composable
fun GradientBackground(content: @Composable ColumnScope.() -> Unit) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Brush.verticalGradient(listOf(BackgroundLight, BackgroundMedium)))
            .padding(16.dp),
        content = content
    )
}
```

### 10.2 StatChip

Icon + value + label, vertical layout. Mirrors `StatItem` in `WordSearchTopicScreen`.

### 10.3 TopicCard

Reuse `TopicCard` from `WordSearchTopicScreen.kt` verbatim, plus:
- `isLocked: Boolean` overlay (semi-transparent gray + 🔒 + requirement text).
- Show "⭐ 0/30" mini progress under name once unlocked.

### 10.4 LevelDot

- 64 dp `Box`, `CircleShape`, colored by state.
- Center: level number, 18 sp Bold.
- Below the dot: 0–3 small star emojis.
- Locked state: gray fill + 🔒 instead of number.

### 10.5 WordChip

Reuse `WordChip` from `WordSearchGameScreen.kt` unchanged.

### 10.6 GameGrid

Reuse `WordSearchGrid` from `WordSearchGameScreen.kt`. Parameterize `gridSize` and adapt font scaling as in §9.5.

### 10.7 AlphaRushTutorial

Rebrand `WordSearchTutorial`. Four steps (same flow): Intro, Touch & Drag, Any Direction, Ready. Replace title color references with `FunPurple` (already `0xFF6200EE`).

### 10.8 ConfettiAnimation

Port the parent app's component (80 particles, 3 s duration, falling + wobble + rotation). Trigger on level complete.

### 10.9 Timer

Display `mm:ss`. Pulses `FunRed` when over the difficulty's `timeBonusSec`.

---

## 11. Achievements / Badges

Optional but recommended. Stored in `ProgressStorage` as a `Set<String>`.

| ID | Title | Trigger |
|---|---|---|
| `first_word` | First Word! | Find your first word |
| `first_level` | Level Up | Complete level 1 of any topic |
| `topic_starter` | Topic Starter | Complete any 5 levels in one topic |
| `topic_master` | Topic Master | 3-star every level in a topic |
| `speedster` | Speedster | Finish a level in under 50% of its time budget |
| `no_hints` | Pure Mind | Complete 10 levels without using hints |
| `daily_3` | Daily Trio | 3-day daily challenge streak |
| `daily_7` | Week Warrior | 7-day daily challenge streak |
| `hundred_stars` | Centurion | Earn 100 stars |
| `polyglot` | Polyglot | Unlock 10 topics |

Show as 🏆 emoji chips in Profile.

---

## 12. Storage

All values via `SharedPreferences("alpharush_prefs", MODE_PRIVATE)`.

### 12.1 Keys

```
progress.total_stars            : Int
progress.total_xp               : Int
progress.streak_days            : Int
progress.last_played_epoch_day  : Long
progress.unlocked_topics        : String (CSV of topic IDs)
progress.best_results           : String (JSON map)
progress.badges                 : String (CSV)

settings.sound_enabled          : Boolean (default true)
settings.haptics_enabled        : Boolean (default true)
settings.tutorial_shown         : Boolean (default false)

daily.last_date                 : String (yyyy-MM-dd)
daily.completed                 : Boolean
daily.stars                     : Int

game.saved.<topicId>.<levelId>  : String (JSON SavedGameState)
```

### 12.2 Serialization

Use `org.json` (already a transitive dep) to keep things simple — no need for `kotlinx.serialization`. Encode arrays as JSONArray, char grids as array of strings (one row per string).

---

## 13. Tutorial

Show `AlphaRushTutorial` overlay the **first time** `GameScreen` is opened (any topic, any level). Persist via `settings.tutorial_shown`. Mirror the existing component:
- Step 1: 🔍 "Find Hidden Words!"
- Step 2: 👆 "Touch & Drag" with horizontal hand sweep
- Step 3: ↗️ "Any Direction!" with diagonal hand sweep
- Step 4: 🎯 "You're Ready!" with "Let's Play" button

Tap anywhere advances; ✖ in the card skips.

---

## 14. Animations

Baseline Compose animations:

- Topic card press: scale 1f ↔ 0.95f via `animateFloatAsState` + spring.
- Word chip on found: scale to 1.05f, color tween, strikethrough.
- Star award sequence: 3 stars pop in (alpha 0→1, scale 0.6→1.2→1.0) staggered 200 ms.
- Hint reveal: cell tint pulse — `infiniteRepeatable` for 1500 ms then fade.
- Timer warning: when `elapsed > timeBudget`, color pulses between text default and `FunRed`.

See **§23 Polish & Juice Layer** for the full juice spec (particles, screen shake, combo system, audio, haptics, Lottie hooks).

---

## 15. Build / Project Setup

### 15.1 Gradle (module `:app`)

- `compileSdk = 36`, `minSdk = 26`, `targetSdk = 36`.
- `applicationId = "online.alpharush"`.
- `versionCode = 1`, `versionName = "1.0.0"`.
- Compose enabled, Kotlin compiler extension matching Compose BOM.
- Lock orientation in `AndroidManifest.xml`: `android:screenOrientation="portrait"`.
- `MainActivity` uses `enableEdgeToEdge()` and `WindowCompat.setDecorFitsSystemWindows(window, false)` for status-bar tinting (match parent app behavior).

### 15.2 `libs.versions.toml` (suggested versions, pin latest stable when building)

```toml
[versions]
kotlin           = "2.0.21"
agp              = "8.7.0"
compose-bom      = "2024.12.01"
nav-compose      = "2.8.4"
core-ktx         = "1.15.0"
lifecycle        = "2.8.7"
activity-compose = "1.9.3"
datastore        = "1.1.1"
lottie-compose   = "6.6.0"
coil-compose     = "2.7.0"

[libraries]
androidx-core-ktx           = { module = "androidx.core:core-ktx",                          version.ref = "core-ktx" }
androidx-lifecycle-runtime  = { module = "androidx.lifecycle:lifecycle-runtime-ktx",        version.ref = "lifecycle" }
androidx-activity-compose   = { module = "androidx.activity:activity-compose",              version.ref = "activity-compose" }
androidx-compose-bom        = { module = "androidx.compose:compose-bom",                    version.ref = "compose-bom" }
androidx-compose-ui         = { module = "androidx.compose.ui:ui" }
androidx-compose-ui-graphics= { module = "androidx.compose.ui:ui-graphics" }
androidx-compose-ui-tooling = { module = "androidx.compose.ui:ui-tooling" }
androidx-compose-ui-preview = { module = "androidx.compose.ui:ui-tooling-preview" }
androidx-compose-material3  = { module = "androidx.compose.material3:material3" }
androidx-navigation-compose = { module = "androidx.navigation:navigation-compose",          version.ref = "nav-compose" }
androidx-datastore-prefs    = { module = "androidx.datastore:datastore-preferences",        version.ref = "datastore" }
lottie-compose              = { module = "com.airbnb.android:lottie-compose",               version.ref = "lottie-compose" }
coil-compose                = { module = "io.coil-kt:coil-compose",                         version.ref = "coil-compose" }
```

App-module `build.gradle.kts` additions:

```kotlin
dependencies {
    implementation(libs.androidx.datastore.prefs)
    implementation(libs.lottie.compose)
    implementation(libs.coil.compose)
    // SoundPool, Vibrator, HapticFeedback are platform APIs — no extra deps.
}
```

### 15.3 ProGuard / R8

Default `proguard-android-optimize.txt`. No reflection-heavy libs in v1, so minimal rules needed.

---

## 16. Testing Plan

### 16.1 Unit Tests (`app/src/test`)
- `GridGeneratorTest`: same seed produces same grid; every placed word is retrievable; no out-of-bounds; respects difficulty direction set.
- `SelectionEngineTest`: `isValidSelection` truth table; `getCellsBetween` for all 8 directions.
- `ScoreCalculatorTest`: time/hint matrix maps to expected star counts and XP.

### 16.2 Instrumentation (`app/src/androidTest`)
- Compose UI test: tap a topic → level select shows correct level count.
- Game flow: launch level 1 of `animals`, drag through a known seeded word, assert word marked found.
- Tutorial test: first game launch shows tutorial; second does not.

### 16.3 Manual QA Checklist
- [ ] Tutorial fires only once.
- [ ] Pause stops timer; resume continues exact elapsed.
- [ ] Backing out mid-level saves state; reopen offers resume.
- [ ] All 22 topics render with correct emoji + accent.
- [ ] Locked topics show 🔒 and unlock once threshold is met.
- [ ] Daily challenge updates at local midnight rollover.
- [ ] Reset progress clears stars, XP, badges, saved games.

---

## 17. Development Milestones

| Phase | Deliverable |
|---|---|
| **M0** | Project skeleton: module, theme, splash, home, nav graph |
| **M1** | Topics screen + Topic data + locked/unlocked rendering |
| **M2** | Level select + Game screen (port WordSearch logic) with EASY only |
| **M3** | Grid generator with 4-tier difficulty + reverse directions |
| **M4** | Save/resume, pause dialog, hint button, completion sheet |
| **M5** | Stars / XP / unlock gating + Profile screen |
| **M6** | Daily Challenge |
| **M7** | Achievements, animations polish, tutorial |
| **M8** | Settings + reset flows + unit/UI tests |
| **M9** | QA pass, icon + adaptive launcher, release build |

---

## 18. Files to Port Verbatim (with minor renames)

These existing parent-app files map cleanly into AlphaRush. Take the logic, rename packages and visual constants where needed, keep behavior identical:

| Source (HaateKhori) | Target (AlphaRush) | Change |
|---|---|---|
| `model/WordSearchModels.kt` | `model/*.kt` | Split into one file per type; extend `WordDirection` with reverse variants |
| `data/WordSearchData.kt` (highlightColors) | `constants/AppColors.kt` | Move into AppColors |
| `data/WordSearchStorage.kt` | `data/GameStateStorage.kt` + `data/ProgressStorage.kt` | Split responsibilities; keep JSON shape |
| `ui/screens/WordSearchGameScreen.kt` | `ui/screens/GameScreen.kt` | Add pause/hints/sheet; parameterize gridSize |
| `ui/screens/WordSearchTopicScreen.kt` | `ui/screens/TopicListScreen.kt` | Add locked overlay + progress text |
| `ui/components/WordSearchTutorial.kt` | `ui/components/AlphaRushTutorial.kt` | Rename only |

The grid-generation helpers (`generateWordSearchGrid`, `tryPlaceWord`, `canPlaceWord`, `placeWord`, `fillEmptyCells`, `getDirectionDeltas`) move to `game/GridGenerator.kt` as top-level functions. Wire them through a `Random(seed)` instance instead of the global `Random`.

---

## 19. Acceptance Criteria (v1)

1. App launches to splash and lands on Home within 1.5 s on a midrange device.
2. ≥ 20 topics implemented, ≥ 600 generated levels playable.
3. Level grid generation completes < 200 ms for EASY/MEDIUM and < 800 ms for HARD/EXPERT on a midrange device.
4. Drag selection works smoothly inside a vertically scrollable parent (pointer events not stolen by scroll).
5. Same level + seed always generates the same grid (verified by test).
6. Star/XP awards persist across cold restarts.
7. Topics lock/unlock correctly based on total stars.
8. Daily challenge rotates on local date change; streak persists.
9. Tutorial shows exactly once unless reset from Settings.
10. App is fully usable offline (airplane mode test).

---

## 20. Branding Assets To Be Provided

These are out of scope for code generation but needed before release:

- Adaptive launcher icon (foreground + background layers).
- Feature graphic 1024×500 (Play Store).
- 4 screenshots (Topics, Game, Level Complete, Daily Challenge).
- Privacy policy URL (required for Play even with no data collection).

---

## 21. Open Questions (defer until after v1)

- Add localized word lists (Bengali, Hindi)?
- Add audio pronunciation on word found (TTS)?
- Online leaderboards?
- Themed seasonal topics (Halloween, Eid, etc.)?

---

## 22. Backend API Specification (post-v1 / later release)

v1 ships fully offline with bundled content. A later release will introduce an optional backend for: dynamic content updates, cross-device progress sync, leaderboards, daily challenge distribution, and seasonal/themed topic packs.

The client must be **backend-optional**: if the API is unreachable or the user is signed out, fall back to bundled content and local-only progress. Implement a thin `RemoteContentRepository` behind the existing in-memory content providers — same data shapes, swap source at runtime.

### 22.1 Conventions

- **Base URL:** `https://api.alpharush.online/api/v1`
- **Protocol:** HTTPS only (reject HTTP).
- **Format:** JSON, UTF-8. `Content-Type: application/json; charset=utf-8`.
- **Auth:** Bearer JWT in `Authorization: Authorization: Bearer <token>` header. Anonymous device identity issued via `/auth/device` for users who do not sign in.
- **Versioning:** URL path (`/v1`). Breaking changes bump to `/v2`; client must send `X-Client-Version: <semver>` and `X-Platform: android` headers.
- **Idempotency:** All `POST` endpoints that mutate progress accept `Idempotency-Key: <uuid>` header. Server dedupes for 24 h.
- **Pagination:** `?page=<int>&size=<int>` (1-indexed, default size 50, max 200). Response envelope includes `pagination` block.
- **ETag / Caching:** All `GET` content endpoints return `ETag` and respect `If-None-Match` (returns `304 Not Modified`). Use to keep bundled assets warm.
- **Rate limit:** 60 req/min per device for read; 20 req/min for write. Headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`.
- **Time:** All timestamps ISO-8601 UTC (`2026-05-14T09:30:00Z`).
- **IDs:** Topic IDs match local string keys (`animals`, `fruits`, ...). Level IDs are integers 1–N within topic scope.

### 22.2 Standard Envelope

Success:

```json
{
  "success": true,
  "data": { ... },
  "meta": { "serverTime": "2026-05-14T09:30:00Z", "version": "1.0.0" }
}
```

Paginated:

```json
{
  "success": true,
  "data": [ ... ],
  "pagination": { "page": 1, "size": 50, "total": 660, "totalPages": 14 },
  "meta": { ... }
}
```

Error:

```json
{
  "success": false,
  "error": {
    "code": "INVALID_TOKEN",
    "message": "Auth token expired",
    "details": { "expiredAt": "2026-05-14T08:00:00Z" }
  }
}
```

### 22.3 Error Codes

| HTTP | Code | Meaning |
|---|---|---|
| 400 | `BAD_REQUEST` | Malformed payload |
| 400 | `VALIDATION_FAILED` | Field validation error (see `details.fields`) |
| 401 | `UNAUTHORIZED` | Missing token |
| 401 | `INVALID_TOKEN` | Token expired / malformed |
| 403 | `FORBIDDEN` | Auth ok but not allowed (e.g. locked topic) |
| 404 | `NOT_FOUND` | Resource missing |
| 409 | `CONFLICT` | State conflict (e.g. older progress) |
| 410 | `GONE` | Content version retired — client must refresh |
| 422 | `OUT_OF_DATE` | Client must upgrade (sends `minClientVersion`) |
| 429 | `RATE_LIMITED` | Slow down |
| 500 | `INTERNAL_ERROR` | Server bug |
| 503 | `MAINTENANCE` | Planned downtime, response includes `retryAfter` |

### 22.4 Authentication

#### `POST /auth/device`
Anonymous device-bound identity. Used on first launch.

Request:
```json
{
  "deviceId": "uuid-v4-generated-on-device",
  "platform": "android",
  "appVersion": "1.1.0",
  "locale": "en-US"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "userId": "usr_01HXYZ...",
    "accessToken": "<jwt>",
    "refreshToken": "<jwt>",
    "expiresIn": 3600
  }
}
```

#### `POST /auth/refresh`
Exchange refresh token for new access token. Body: `{ "refreshToken": "<jwt>" }`.

#### `POST /auth/link`
Optional: upgrade anonymous account to identified (email + OTP / Google sign-in). Body: `{ "provider": "google", "idToken": "<google-id-token>" }`. Server merges any existing identified account into device-bound one (or vice-versa) deterministically.

#### `DELETE /auth/account`
Right-to-be-forgotten. Erases server progress; client falls back to local.

### 22.5 Content Endpoints (public, may be unauthenticated)

#### `GET /content/manifest`
Returns content version + per-section ETags. Client compares against last-cached manifest to decide what to refresh.

Response:
```json
{
  "success": true,
  "data": {
    "contentVersion": "2026.05.14",
    "minClientVersion": "1.0.0",
    "sections": {
      "topics":   { "etag": "W/\"a1b2\"", "updatedAt": "2026-05-10T00:00:00Z" },
      "levels":   { "etag": "W/\"c3d4\"", "updatedAt": "2026-05-10T00:00:00Z" },
      "seasonal": { "etag": "W/\"e5f6\"", "updatedAt": "2026-05-14T00:00:00Z" }
    }
  }
}
```

#### `GET /content/topics`
List of all topics (server-side may exceed v1's 22).

Response `data`:
```json
[
  {
    "id": "animals",
    "name": "Animals",
    "icon": "🐾",
    "accentColor": "0xFFFFEB3B",
    "wordPool": ["DOG","CAT","LION", ...],
    "unlockStarsRequired": 0,
    "category": "core",
    "seasonalUntil": null
  }
]
```

`category` ∈ `core` | `seasonal` | `premium` (premium reserved for future; v1 ignores). `seasonalUntil` is an ISO date for time-boxed topic packs.

#### `GET /content/topics/{topicId}`
Single topic with full word pool + per-level overrides.

#### `GET /content/topics/{topicId}/levels`
All levels for a topic (paginated, default size 30).

Response item:
```json
{
  "id": 1,
  "topicId": "animals",
  "difficulty": "EASY",
  "targetWordCount": 3,
  "seed": 42,
  "wordOverrides": null
}
```

`wordOverrides`: optional `List<String>` to pin specific words for that level (otherwise client generates from pool + seed). Lets curators tune individual levels without client release.

#### `GET /content/bundle`
One-shot bulk download for offline caching. Returns topics + levels + hints + manifest in a single zipped JSON blob. Supports `?since=<contentVersion>` for delta.

Response (uncompressed):
```json
{
  "manifest": { ... },
  "topics":   [ ... ],
  "levels":   { "animals": [ ... ], "fruits": [ ... ] },
  "hints":    { "animals": { "LION": "King of the jungle" } }
}
```

### 22.6 Daily Challenge

#### `GET /daily/today`
Server-issued daily challenge (replaces v1's local seed). Ensures all players globally get the same challenge per UTC date — required for leaderboards.

Response:
```json
{
  "success": true,
  "data": {
    "date": "2026-05-14",
    "topicId": "animals",
    "difficulty": "MEDIUM",
    "seed": 20260514,
    "targetWordCount": 7,
    "expiresAt": "2026-05-15T00:00:00Z",
    "rewardMultiplier": 2
  }
}
```

#### `GET /daily/history?from=<date>&to=<date>`
Past 30 days of daily challenges + user's results. Max range 90 days.

#### `POST /daily/submit`
Submit daily challenge result.

Request:
```json
{
  "date": "2026-05-14",
  "stars": 3,
  "timeSeconds": 67,
  "hintsUsed": 0,
  "completedAt": "2026-05-14T09:31:07Z",
  "clientSeed": 20260514
}
```

Response includes updated streak + leaderboard rank.

### 22.7 Progress Sync

#### `GET /progress`
Full player snapshot — used on cold start / device restore.

Response:
```json
{
  "success": true,
  "data": {
    "userId": "usr_...",
    "totalStars": 142,
    "totalXp": 4820,
    "streakDays": 6,
    "lastPlayedEpochDay": 20231,
    "unlockedTopicIds": ["animals","fruits", "..."],
    "bestResults": {
      "animals:1": { "stars": 3, "timeSeconds": 42, "xpEarned": 36, "hintsUsed": 0, "completedAt": "2026-05-10T..." }
    },
    "badges": ["first_word","speedster"],
    "updatedAt": "2026-05-14T09:30:00Z"
  }
}
```

#### `POST /progress/level`
Submit single level result. Server merges using **best-wins** rule (max stars, then min time, then min hints). Returns merged record + any newly-unlocked topics/badges as side-effects.

Request:
```json
{
  "topicId": "animals",
  "levelId": 1,
  "stars": 3,
  "timeSeconds": 42,
  "hintsUsed": 0,
  "xpEarned": 36,
  "completedAt": "2026-05-14T09:30:00Z",
  "clientSeed": 12345
}
```

Response:
```json
{
  "success": true,
  "data": {
    "merged": { "topicId":"animals","levelId":1,"stars":3,"timeSeconds":42, ... },
    "totalStars": 142,
    "totalXp": 4820,
    "newlyUnlockedTopics": ["sports"],
    "newlyUnlockedBadges": ["topic_starter"]
  }
}
```

#### `POST /progress/batch`
Bulk-sync after offline play. Accepts an array of `LevelResult` (≤ 500 per call). Same best-wins merge per entry. Use after reconnect.

#### `POST /progress/reset`
Erases server-side progress (client-initiated). Requires body `{ "confirm": true }`.

### 22.8 Achievements / Badges

#### `GET /achievements`
Catalog of all achievement definitions (id, title, description, icon emoji, criteria summary). Server is source of truth so new badges can ship without app update.

#### `GET /achievements/me`
User's unlocked badges with timestamps. (Redundant with `/progress.badges` but cheaper.)

### 22.9 Leaderboards (optional v2 feature)

#### `GET /leaderboard/daily?date=<yyyy-mm-dd>&scope=global|country&page=&size=`
Top players for given daily challenge. Scoring: `stars * 1000 - timeSeconds - hintsUsed * 50`.

Response item:
```json
{ "rank": 1, "displayName": "AlphaFox", "avatar": "🦊", "score": 2933, "stars": 3, "timeSeconds": 67 }
```

#### `GET /leaderboard/topic/{topicId}?page=&size=`
All-time topic leaderboard (sum of stars, tie-break by total time).

#### `GET /leaderboard/me?board=<daily|topic|global>`
User's own ranking on a board, with ±5 neighbors.

### 22.10 Telemetry (opt-in)

#### `POST /telemetry/events`
Anonymous gameplay metrics. Only sent if user opted in via Settings. Events batched ≤ 50 per call.

Request:
```json
{
  "events": [
    {
      "name": "level_started",
      "ts": "2026-05-14T09:30:00Z",
      "props": { "topicId":"animals", "levelId":1, "difficulty":"EASY" }
    }
  ]
}
```

Recommended event names: `app_open`, `level_started`, `level_completed`, `level_abandoned`, `hint_used`, `daily_played`, `topic_unlocked`, `tutorial_finished`.

### 22.11 Config (remote feature flags)

#### `GET /config`
Server-controlled flags. Cache locally with 1 h TTL.

Response:
```json
{
  "success": true,
  "data": {
    "minClientVersion": "1.0.0",
    "forceUpdateBelow": "0.9.0",
    "leaderboardsEnabled": false,
    "seasonalTopicsEnabled": true,
    "maxOfflineQueueSize": 500,
    "telemetryEnabled": true
  }
}
```

### 22.12 Security

- TLS 1.2+ only. Certificate pinning recommended (pin to leaf or intermediate; rotate via remote config).
- JWT signed with RS256. Access token TTL 1 h, refresh 30 d.
- Never include PII in URLs (no email/name as path/query). Use `userId` opaque ULID.
- Input validation on every endpoint. Reject `timeSeconds < 0`, `stars ∉ [0,3]`, `levelId` outside topic range.
- Anti-cheat: server recomputes XP / stars from `timeSeconds + hintsUsed + difficulty`; client-sent values are advisory only. Reject submissions where `timeSeconds < difficulty.minPlausibleSec` (configurable, e.g. 5 s).
- Display names: profanity-filtered, max 20 chars, ASCII + emoji only.
- COPPA: app is kid-targeted — no email collection by default, no behavioral ads, no third-party trackers in client. Leaderboards use generated handles, not real names. Parental gate required to enable identified account linking.
- Privacy: data minimization — only store progress fields listed in §22.7. No device fingerprinting beyond `deviceId` provided by client.

### 22.13 Server Tech Stack (suggested)

| Concern | Choice |
|---|---|
| Language | Kotlin (Spring Boot 3.x) or Node 20 (NestJS) |
| Auth | JWT (RS256) via Spring Security / Passport |
| DB | PostgreSQL 16 (player + progress); Redis 7 (rate limit + leaderboards via sorted sets) |
| Storage | S3-compatible bucket for content bundles |
| CDN | CloudFront / Cloudflare in front of `/content/*` |
| Observability | OpenTelemetry → Grafana/Loki; Sentry for errors |
| Deployment | Docker → AWS ECS or Fly.io; blue/green |

### 22.14 Database Schema (Postgres, abbreviated)

```sql
CREATE TABLE users (
  user_id          TEXT PRIMARY KEY,           -- ULID
  device_id        TEXT UNIQUE,
  display_name     TEXT,
  avatar_emoji     TEXT,
  country_code     CHAR(2),
  created_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
  deleted_at       TIMESTAMPTZ
);

CREATE TABLE topics (
  id                     TEXT PRIMARY KEY,
  name                   TEXT NOT NULL,
  icon                   TEXT NOT NULL,
  accent_color           BIGINT NOT NULL,
  word_pool              JSONB NOT NULL,
  unlock_stars_required  INT NOT NULL DEFAULT 0,
  category               TEXT NOT NULL DEFAULT 'core',
  seasonal_until         DATE,
  updated_at             TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE levels (
  topic_id          TEXT REFERENCES topics(id),
  level_id          INT,
  difficulty        TEXT NOT NULL,            -- EASY|MEDIUM|HARD|EXPERT
  target_word_count INT NOT NULL,
  seed              BIGINT NOT NULL,
  word_overrides    JSONB,
  PRIMARY KEY (topic_id, level_id)
);

CREATE TABLE level_results (
  user_id       TEXT REFERENCES users(user_id),
  topic_id      TEXT,
  level_id      INT,
  stars         SMALLINT NOT NULL,
  time_seconds  INT NOT NULL,
  hints_used    SMALLINT NOT NULL,
  xp_earned     INT NOT NULL,
  completed_at  TIMESTAMPTZ NOT NULL,
  PRIMARY KEY (user_id, topic_id, level_id)
);

CREATE TABLE daily_challenges (
  date          DATE PRIMARY KEY,
  topic_id      TEXT NOT NULL,
  difficulty    TEXT NOT NULL,
  seed          BIGINT NOT NULL,
  word_count    INT NOT NULL,
  reward_mult   SMALLINT NOT NULL DEFAULT 2
);

CREATE TABLE daily_results (
  user_id       TEXT REFERENCES users(user_id),
  date          DATE REFERENCES daily_challenges(date),
  stars         SMALLINT NOT NULL,
  time_seconds  INT NOT NULL,
  hints_used    SMALLINT NOT NULL,
  score         INT NOT NULL,
  completed_at  TIMESTAMPTZ NOT NULL,
  PRIMARY KEY (user_id, date)
);

CREATE TABLE user_badges (
  user_id     TEXT REFERENCES users(user_id),
  badge_id    TEXT,
  unlocked_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (user_id, badge_id)
);

CREATE INDEX ON level_results (user_id);
CREATE INDEX ON daily_results (date, score DESC);
```

Leaderboards: maintain Redis sorted sets keyed `lb:daily:<yyyy-mm-dd>` and `lb:topic:<topicId>` with member = `userId`, score = computed score. Refresh on `/progress/level` and `/daily/submit`.

### 22.15 Client Integration Notes

- Add module package `online.alpharush.network` with `RetrofitClient`, `AlphaRushApi` (Retrofit interface), and DTOs mirroring §6 models.
- Keep `RemoteContentRepository` behind same interface as local content provider — flip via `BuildConfig.USE_REMOTE_CONTENT` or remote config.
- Persist offline queue (unsent `LevelResult`s) in `data/OfflineQueueStorage.kt`. Flush via WorkManager `OneTimeWorkRequest` with network constraint on app launch + completion.
- ETag cache: store per-section `etag` + last-fetched payload in `SharedPreferences("alpharush_content_cache")`. Send `If-None-Match` on every content GET.
- Migration path: on first launch of v1.1, read local v1 progress and POST to `/progress/batch` once auth is established. Mark migrated via `prefs.progress_migrated = true`.

### 22.16 Acceptance Criteria (backend release)

1. Anonymous device auth works on first launch, no user interaction.
2. Offline-first: airplane mode play still works; results queue + sync on reconnect.
3. Best-wins progress merge is commutative (order of `/progress/level` calls produces same final state).
4. Content delta (`/content/bundle?since=`) reduces payload by ≥ 80% vs full bundle on a no-change day.
5. Daily challenge identical for all players within same UTC date.
6. Leaderboard reads p95 < 150 ms at 10k concurrent users.
7. Anti-cheat rejects submissions with `timeSeconds` below plausible floor; flagged accounts logged.
8. COPPA: no PII collected without parental gate; account deletion removes all server data within 30 d.

---

## 23. Polish & Juice Layer

This section is the contract for making AlphaRush *feel* like Candy Crush, not just look like a word search. Every gameplay event must fire **at least three** of: visual reaction, audio cue, haptic, particle, screen flourish. Stack reactions on big events (combo, level win, badge unlock) and keep them on small ones (cell tap, word found).

### 23.1 Asset Inventory

Place under `app/src/main/`:

```
res/raw/
├── sfx_tap.ogg                  // cell touch down (16 kHz mono, ~80 ms)
├── sfx_select.ogg               // cell added to selection
├── sfx_found.ogg                // word found (default)
├── sfx_found_pitch.ogg          // optional alt for combo escalation (or pitch-shift sfx_found at runtime)
├── sfx_combo.ogg                // combo banner appears
├── sfx_miss.ogg                 // invalid selection released
├── sfx_hint.ogg                 // hint reveal pulse
├── sfx_pause.ogg                // pause open
├── sfx_star_pop.ogg             // star award per-star (play 3× staggered)
├── sfx_win.ogg                  // full level clear sting
├── sfx_lose.ogg                 // time-up sting
├── sfx_unlock.ogg               // topic/badge unlock
├── sfx_button.ogg               // generic UI button
├── bgm_menu.ogg                 // home / topic screens (60–90 s loop, low-energy)
├── bgm_game_easy.ogg            // game BGM EASY/MEDIUM
├── bgm_game_hard.ogg            // game BGM HARD/EXPERT (more tense)
└── bgm_daily.ogg                // daily challenge BGM

assets/lottie/
├── star_burst.json              // 1 s pop, used per star award
├── confetti.json                // 3 s fall + spin (replaces / augments Canvas confetti)
├── badge_unlock.json            // 2 s shine + scale
├── level_intro.json             // topic emoji bounce + sparkle backdrop
├── combo_x2.json                // combo banner v1
├── combo_x3.json                // combo banner v2 (more energetic)
├── combo_x5.json                // max-tier combo banner
├── streak_flame.json            // streak flame loop for ProfileScreen
└── loader.json                  // splash / loading spinner
```

Audio assets ship as `.ogg` (smaller than `.mp3`, no licensing concerns). SFX 16-bit mono ≤ 100 KB each, BGM 96 kbps stereo ≤ 1 MB.

### 23.2 Audio (`audio/SoundManager.kt`)

Single application-scoped `SoundManager` initialised in `Application.onCreate()` (or by the root composable via `remember` + `DisposableEffect` if avoiding a custom `Application`). Wraps `SoundPool`.

```kotlin
class SoundManager(private val context: Context) {

    private val pool: SoundPool = SoundPool.Builder()
        .setMaxStreams(8)
        .setAudioAttributes(
            AudioAttributes.Builder()
                .setUsage(AudioAttributes.USAGE_GAME)
                .setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION)
                .build()
        )
        .build()

    private val ids: MutableMap<SoundEvent, Int> = mutableMapOf()
    private var enabled: Boolean = true
    private var masterVolume: Float = 1f

    fun preloadAll() {
        SoundEvent.values().forEach { event ->
            val resId = event.resId ?: return@forEach
            ids[event] = pool.load(context, resId, 1)
        }
    }

    fun play(event: SoundEvent, rate: Float = 1f, volume: Float = 1f) {
        if (!enabled) return
        val id = ids[event] ?: return
        val v = (volume * masterVolume).coerceIn(0f, 1f)
        pool.play(id, v, v, /*priority*/ 1, /*loop*/ 0, rate.coerceIn(0.5f, 2f))
    }

    fun setEnabled(value: Boolean) { enabled = value }
    fun setMasterVolume(value: Float) { masterVolume = value.coerceIn(0f, 1f) }

    fun release() { pool.release() }
}

enum class SoundEvent(val resId: Int?) {
    TAP       (R.raw.sfx_tap),
    SELECT    (R.raw.sfx_select),
    FOUND     (R.raw.sfx_found),
    COMBO     (R.raw.sfx_combo),
    MISS      (R.raw.sfx_miss),
    HINT      (R.raw.sfx_hint),
    PAUSE     (R.raw.sfx_pause),
    STAR_POP  (R.raw.sfx_star_pop),
    WIN       (R.raw.sfx_win),
    LOSE      (R.raw.sfx_lose),
    UNLOCK    (R.raw.sfx_unlock),
    BUTTON    (R.raw.sfx_button)
}
```

Provide via a `CompositionLocal`:

```kotlin
val LocalSoundManager = staticCompositionLocalOf<SoundManager> {
    error("SoundManager not provided")
}
```

**Combo pitch escalation:** on each `FOUND` while combo active, play with `rate = 1f + (combo - 1) * 0.08f` (capped at `1.5f`). Higher combo → higher pitch.

**BGM:** `MusicManager` wraps a single `MediaPlayer`. Crossfade between tracks over 400 ms (`Animatable<Float>` for volume; pause+release outgoing player after fade-out). Pause on `Lifecycle.Event.ON_PAUSE`, resume on `ON_RESUME`. Duck to 30 % volume during win/star sting.

### 23.3 Haptics (`haptic/HapticManager.kt`)

```kotlin
class HapticManager(context: Context) {

    private val vibrator: Vibrator = if (Build.VERSION.SDK_INT >= 31) {
        context.getSystemService(VibratorManager::class.java).defaultVibrator
    } else {
        @Suppress("DEPRECATION")
        context.getSystemService(Vibrator::class.java)
    }

    private var enabled: Boolean = true
    fun setEnabled(value: Boolean) { enabled = value }

    fun light()   = vibrate(longArrayOf(0, 12), -1)
    fun tick()    = vibrate(longArrayOf(0, 8),  -1)
    fun success() = vibrate(longArrayOf(0, 20, 40, 30), -1)
    fun combo(level: Int) {
        val pattern = when (level.coerceIn(1, 5)) {
            1 -> longArrayOf(0, 15)
            2 -> longArrayOf(0, 15, 30, 25)
            3 -> longArrayOf(0, 20, 30, 30, 30, 35)
            4 -> longArrayOf(0, 25, 30, 35, 30, 40, 30, 45)
            else -> longArrayOf(0, 30, 25, 40, 25, 50, 25, 60, 25, 70)
        }
        vibrate(pattern, -1)
    }
    fun win()     = vibrate(longArrayOf(0, 40, 80, 40, 80, 80), -1)
    fun lose()    = vibrate(longArrayOf(0, 60, 60, 60), -1)

    private fun vibrate(pattern: LongArray, repeat: Int) {
        if (!enabled || !vibrator.hasVibrator()) return
        val effect = VibrationEffect.createWaveform(pattern, repeat)
        vibrator.vibrate(effect)
    }
}
```

Permission in `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.VIBRATE" />
```

Provide via `LocalHapticManager` `CompositionLocal`. Always check the user setting (`settings.haptics_enabled`) before calling.

### 23.4 Combo System (`game/ComboTracker.kt`)

A combo continues while the player finds words within `comboWindowMs` (default **3500 ms**) of the previous find. Each consecutive find within the window increments `combo`.

```kotlin
class ComboTracker(
    private val comboWindowMs: Long = 3500L,
    private val clock: () -> Long = { System.currentTimeMillis() }
) {
    private var lastFindAt: Long = 0L
    var combo: Int = 0
        private set
    var maxCombo: Int = 0
        private set

    /** Returns the new combo value. */
    fun onWordFound(): Int {
        val now = clock()
        combo = if (now - lastFindAt <= comboWindowMs) combo + 1 else 1
        lastFindAt = now
        if (combo > maxCombo) maxCombo = combo
        return combo
    }

    fun reset() { combo = 0; lastFindAt = 0L }
}
```

Combo XP bonus: `finalXp = baseXp + (comboAtFind - 1) * 3`. Track `maxCombo` per level and award **+10 XP if `maxCombo >= 4`**.

**Reactions per combo tier:**

| Combo | SFX rate | Haptic | Visual |
|---|---|---|---|
| 1 | 1.00 | `light` | word chip pulse, sparkle trail |
| 2 | 1.08 | `success` | + `combo_x2.json` banner |
| 3 | 1.16 | `combo(2)` | + screen-edge glow pulse |
| 4 | 1.24 | `combo(3)` | + `combo_x3.json` banner + 1.5× particle burst |
| 5+ | 1.32+ | `combo(5)` | + `combo_x5.json` + brief screen shake (amp 6 dp, 250 ms) |

### 23.5 Particle System (`effects/ParticleSystem.kt`)

Pooled Compose-Canvas particles. Pool size 256, reuse via free-list.

Particle struct:

```kotlin
data class Particle(
    var x: Float, var y: Float,
    var vx: Float, var vy: Float,
    var ax: Float = 0f, var ay: Float = 1200f, // gravity px/s²
    var life: Float = 1f,                       // remaining 1f → 0f
    var maxLife: Float = 1f,
    var size: Float,
    var color: Color,
    var rotation: Float = 0f,
    var spin: Float = 0f,
    var shape: ParticleShape = ParticleShape.CIRCLE,
    var active: Boolean = false
)

enum class ParticleShape { CIRCLE, SQUARE, STAR, RIBBON }
```

Drive with `withFrameNanos` inside a `LaunchedEffect`. Update once per frame, render via `Canvas { particles.forEach { drawParticle(it) } }`.

Presets:

- **`emitConfetti(origin, count = 60)`** — wide cone upward, gravity on, mixed `highlightColors`, RIBBON + SQUARE shapes. Used on level complete.
- **`emitWordBurst(cells, color)`** — 24 particles from each found cell along the selection line, low gravity, CIRCLE + STAR shapes, tinted with the word's highlight color.
- **`emitSparkleTrail(point, count = 4)`** — emitted every 33 ms while user is dragging; small short-lived STAR particles drifting upward.
- **`emitStarPop(point)`** — 30 STAR particles outward from the star slot on `LevelCompleteScreen`.

### 23.6 Screen Shake (`effects/ScreenShake.kt`)

```kotlin
class ScreenShakeController {
    val offset = Animatable(Offset.Zero, Offset.VectorConverter)

    suspend fun shake(amplitudeDp: Float, durationMs: Int) {
        val amp = amplitudeDp
        val steps = (durationMs / 16).coerceAtLeast(4)
        val rng = java.util.Random()
        repeat(steps) { i ->
            val decay = 1f - i.toFloat() / steps
            offset.snapTo(
                Offset(
                    (rng.nextFloat() * 2f - 1f) * amp * decay,
                    (rng.nextFloat() * 2f - 1f) * amp * decay
                )
            )
            delay(16)
        }
        offset.snapTo(Offset.Zero)
    }
}

fun Modifier.screenShake(controller: ScreenShakeController): Modifier =
    this.offset { IntOffset(controller.offset.value.x.toInt(), controller.offset.value.y.toInt()) }
```

Apply to the `GameScreen` root `Box`. Trigger on combo ≥ 5 (amp 6 dp / 250 ms), level complete (amp 4 dp / 200 ms), and time-up (amp 10 dp / 400 ms).

### 23.7 Lottie Hooks (`ui/components/*Lottie.kt`)

```kotlin
@Composable
fun StarBurstLottie(modifier: Modifier = Modifier, onEnd: () -> Unit = {}) {
    val composition by rememberLottieComposition(LottieCompositionSpec.Asset("lottie/star_burst.json"))
    val progress by animateLottieCompositionAsState(composition, iterations = 1)
    LottieAnimation(composition, progress, modifier)
    LaunchedEffect(progress) { if (progress >= 1f) onEnd() }
}
```

Mirror the pattern for `ConfettiLottie`, `BadgeUnlockLottie`, `LevelIntroLottie`, `ComboBannerLottie` (variant per tier), `StreakFlameLottie` (loops infinitely).

### 23.8 Event → Reaction Matrix

Single source of truth for which feedback fires on each gameplay event. Implement as a `GameEventBus` (a `MutableSharedFlow<GameEvent>`) that the `GameScreen` collects.

| Event | Visual | Audio | Haptic | Particles | Lottie | Screen |
|---|---|---|---|---|---|---|
| Cell touch down | cell scale 0.92×, 80 ms | `TAP` | `tick` | — | — | — |
| Cell added to selection | selection trail extends | `SELECT` (rate +0.02 per cell) | `light` every 3rd cell | sparkle trail | — | — |
| Invalid release | trail fades red 200 ms | `MISS` | `light` | — | — | — |
| Word found | chip strikethrough + scale 1.05×, cells flash highlight color, found-color trail persists | `FOUND` at combo rate | `success` (combo 1) or escalating | `emitWordBurst` along line | — (combo ≥ 2 fires banner) | — |
| Combo ≥ 2 | banner slides in top-center, scale-bounce | `COMBO` | per §23.4 | extra burst 1.5× | `combo_x{N}.json` | — |
| Combo ≥ 5 | banner + max colors | `COMBO` rate 1.32+ | `combo(5)` | burst 2× + edge glow | `combo_x5.json` | shake 6 dp / 250 ms |
| Hint used | target cell pulses `FunYellow` 1500 ms | `HINT` | `light` | small sparkle at cell | — | — |
| Pause opened | dim 60 %, sheet rises | `PAUSE` | — | — | — | — |
| Time warning (≥ time budget) | timer text pulses `FunRed` | none | `tick` every 5 s while over | — | — | subtle vignette pulse |
| Time up (fail) | grid greys, sad emoji | `LOSE` | `lose` | — | — | shake 10 dp / 400 ms |
| Level complete | bottom sheet rises, dim 70 % | `WIN` then `STAR_POP` × stars (200 ms apart) | `success` then per-star `tick` | `emitConfetti` from top edge | `confetti.json` + `star_burst.json` (per star) | shake 4 dp / 200 ms |
| Topic unlocked | toast-card slides from top, 2 s | `UNLOCK` | `success` | sparkle around card | `badge_unlock.json` | — |
| Badge earned | profile-style card overlay 2.5 s | `UNLOCK` | `success` | — | `badge_unlock.json` | — |
| Daily streak +1 | flame emoji bounce on home | `STAR_POP` | `tick` | — | `streak_flame.json` (briefly intensifies) | — |
| Level intro (open game) | topic emoji scales 0→1.2→1.0, name fades in | `BUTTON` | — | — | `level_intro.json` (skippable on tap) | — |

### 23.9 Meta-Game Add-Ons (Candy-Crush-style)

Add on top of the existing progression in §7 / §8 / §11. All persisted in DataStore.

- **World Map LevelSelect:** replace zig-zag grid with a custom layout that draws a hand-drawn path connecting `LevelDot`s (Compose `Path` between dots, dashed-stroke advance animation as new levels unlock). Parallax background scrolls 0.3× the foreground scroll. Topic accent tints the path.
- **Lives system (optional, off by default for kid-app):** 5 hearts, regenerate one per 20 min. Show heart counter on Home + LevelSelect. Failing a level (time-up) consumes a heart. Toggle in `SettingsScreen` ("Difficulty pressure: Off / Hearts").
- **Daily Reward Chain:** 7-day rotating gift (Day 1: 10 XP, Day 2: 1 hint, Day 3: 20 XP, Day 4: 1 booster, Day 5: 30 XP, Day 6: 2 hints, Day 7: 1 topic-unlock token). Modal on first launch each day. Streak persists; missed day resets to Day 1.
- **Boosters** (earned, not bought — kid-app, no IAP):
  - **Reveal Letter** — highlights first letter of one random unfound word for 2.5 s.
  - **Freeze Time** — pauses timer for 10 s, blue tint on timer pill.
  - **Magnifier** — slows time decay by 50 % for the rest of the level.
  Inventory shown in `GameScreen` bottom row alongside hint.
- **Combo XP** (see §23.4).
- **Achievements** extended (additions to §11):
  - `combo_5` — Hit a combo of 5+ words.
  - `flawless_three` — Three back-to-back 3-star clears in one session.
  - `flame_keeper` — 14-day daily streak.
  - `world_traveller` — Play in 5 different topics in one day.

### 23.10 Performance Budget

Polish must not regress the spec's perf criteria (§19):

- Particle system: cap active particles at 192. Above that, refuse new emissions.
- Lottie: never render more than 2 concurrent compositions; recycle via `LottieRetainMode = Disabled`.
- SoundPool: max 8 streams. Ignore plays over cap.
- BGM: max 1 active `MediaPlayer`. Recycle on screen change.
- Frame budget: keep `GameScreen` recompositions to ≤ 4 ms on midrange (use `derivedStateOf` for chip strikethrough, hoist particle state into separate composable).
- Asset weight: total `res/raw` + `assets/lottie` ≤ 4 MB combined. Compress Lottie JSON via `lottie-android`'s minifier.

Profiling checklist:

- [ ] Layout Inspector shows no GameScreen overdraw > 3×.
- [ ] Macrobenchmark: word-find frame p99 < 33 ms (30 fps floor) on Pixel 4a.
- [ ] APK size delta from polish stack ≤ 6 MB.

### 23.11 Persistence Migration (SharedPreferences → DataStore)

```kotlin
val Context.alphaRushDataStore by preferencesDataStore(
    name = "alpharush_prefs",
    produceMigrations = { ctx ->
        listOf(SharedPreferencesMigration(ctx, "alpharush_prefs"))
    }
)
```

DataStore keys mirror §12.1 verbatim (same names) but typed via `preferencesKey`. Storage classes (`ProgressStorage`, `SettingsStorage`, `GameStateStorage`, `DailyChallengeStorage`) expose `Flow<...>` for reactive UI. Writes use `dataStore.edit { ... }` inside coroutines. The JSON blob shape (game state, best results) is unchanged.

### 23.12 Settings Additions

Extend `SettingsScreen` (§9.9):

- **Music** — slider 0–100 % (default 60 %).
- **Sound effects** — slider 0–100 % (default 100 %).
- **Haptic feedback** — on/off (default on).
- **Reduce motion** — on/off. When on: disable screen shake, halve particle counts, swap Lottie animations for static end-frames.
- **Hearts system** — Off / On (default Off for v1).
- **Reset tutorial flag** — same.
- **Reset all progress** — same.

Persist all values in DataStore. Honor **Reduce motion** in `ScreenShakeController`, `ParticleSystem`, and Lottie wrappers.

### 23.13 Acceptance Criteria (Polish v1)

Adds to §19:

11. Every entry in §23.8 fires at least the listed visual + audio + haptic on first matching event.
12. Master mute toggle + per-channel volume sliders work in real time (no app restart).
13. Reduce-motion toggle measurably drops dropped-frame count by ≥ 30 % under combo storms (verified with `Choreographer.FrameCallback`).
14. Lottie + Coil + DataStore add ≤ 6 MB to release APK (R8 enabled).
15. SoundPool preloads all 12 SFX in < 200 ms cold-start on Pixel 4a; play latency < 60 ms.
16. No audio glitch (gap > 100 ms) when crossfading BGM on navigation between Home → Topics → Game.
17. Combo banner + screen shake never overlap a star-award sequence (queue events; sheet animations win priority).

---

**End of spec.** Anything ambiguous after reading this should be resolved by mirroring the existing `WordSearch*.kt` files in the HaateKhori repo, since they are the visual and behavioral source of truth.
