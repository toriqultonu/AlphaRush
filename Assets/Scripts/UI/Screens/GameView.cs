using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameView : MonoBehaviour {
    [SerializeField] GameGridView gridView;
    [SerializeField] WordListView wordList;
    [SerializeField] TimerView timer;
    [SerializeField] TMP_Text headerTopic, progressLine;
    [SerializeField] Button hintBtn, restartBtn, pauseBtn, backBtn;
    [SerializeField] ComboBannerView comboBanner;
    [SerializeField] GameObject layoutPortrait, layoutLandscape;
    [SerializeField] PauseDialog pauseDialog;
    [SerializeField] ResumeDialog resumeDialog;
    [SerializeField] TutorialOverlay tutorial;

    // Set by DailyChallengeView right before Open() so Complete() can credit the streak.
    public static bool NextOpenIsDaily;

    string topicId; int levelId;
    Topic topic; Level level;
    List<PlacedWord> placedWords;
    List<FoundWord> foundWords = new();
    char[,] grid;
    int elapsedSec, hintsUsed, colorIndex;
    bool paused, complete, isDailyRun;
    ComboTracker combo = new(AppConfig.ComboWindowMs);
    Coroutine timerLoop;
    long? pendingShuffleSeed; // set by Restart — re-places words with a fresh layout

    void OnEnable() {
        if (hintBtn != null) hintBtn.onClick.AddListener(UseHint);
        if (restartBtn != null) restartBtn.onClick.AddListener(Restart);
        if (pauseBtn != null) pauseBtn.onClick.AddListener(Pause);
        if (backBtn != null) backBtn.onClick.AddListener(SaveAndQuit);
        if (pauseDialog != null) {
            pauseDialog.OnResume  = ClosePauseDialog;
            pauseDialog.OnRestart = () => { ClosePauseDialog(); Restart(); };
            pauseDialog.OnQuit    = () => { ClosePauseDialog(); SaveAndQuit(); };
        }
    }

    void OnDisable() {
        if (hintBtn != null) hintBtn.onClick.RemoveListener(UseHint);
        if (restartBtn != null) restartBtn.onClick.RemoveListener(Restart);
        if (pauseBtn != null) pauseBtn.onClick.RemoveListener(Pause);
        if (backBtn != null) backBtn.onClick.RemoveListener(SaveAndQuit);
    }

    public async void Open(string topicId, int levelId) {
        this.topicId = topicId;
        this.levelId = levelId;
        isDailyRun = NextOpenIsDaily;
        NextOpenIsDaily = false;

        var repo = ServiceLocator.Content;
        topic = await repo.GetTopicAsync(topicId);
        level = await repo.GetLevelAsync(topicId, levelId);
        var words = await repo.GetWordsForLevelAsync(level);

        if (headerTopic != null) headerTopic.text = topic?.name ?? topicId;

        var saved = ServiceLocator.GameState?.Load(topicId, levelId);
        if (saved != null && resumeDialog != null) {
            // Let the player choose before anything starts ticking.
            resumeDialog.OnResume = () => {
                resumeDialog.gameObject.SetActive(false);
                LoadFromSaved(saved);
                BeginRound();
            };
            resumeDialog.OnRestart = () => {
                resumeDialog.gameObject.SetActive(false);
                ServiceLocator.GameState?.Delete(topicId, levelId);
                pendingShuffleSeed = NewShuffleSeed();
                GenerateFreshGrid(words);
                BeginRound();
            };
            resumeDialog.gameObject.SetActive(true);
            return;
        }

        if (saved != null) LoadFromSaved(saved);
        else GenerateFreshGrid(words);
        BeginRound();
    }

    void BeginRound() {
        gridView.OnSelectionComplete = OnSelectionEnd;
        gridView.Build(grid);

        wordList?.SetWords(ExtractPlacedWords());
        foreach (var fw in foundWords)
            wordList?.MarkFound(fw.word, UnpackColor(fw.colorPacked));

        UpdateProgress();

        complete = false; paused = false;
        if (timerLoop != null) StopCoroutine(timerLoop);
        timerLoop = StartCoroutine(TickTimer());

        // First-launch tutorial (spec §16.3 — fires once).
        var settings = ServiceLocator.Settings?.Load();
        if (settings != null && !settings.tutorialShown && tutorial != null) {
            paused = true;
            tutorial.OnFinished = () => {
                settings.tutorialShown = true;
                ServiceLocator.Settings?.Save(settings);
                paused = false;
            };
            tutorial.Show();
        }
    }

    void GenerateFreshGrid(List<string> words) {
        // First play of a level uses the deterministic seed (acceptance #4);
        // Restart shuffles with a one-shot random seed and resets the clock.
        long seed = pendingShuffleSeed ?? level.seed;
        pendingShuffleSeed = null;
        var res = GridGenerator.Generate(level.difficulty.GridSize(), words, level.difficulty, seed);
        grid = res.Grid;
        placedWords = res.Placed;
        foundWords = new List<FoundWord>();
        elapsedSec = 0; hintsUsed = 0; colorIndex = 0;
    }

    static long NewShuffleSeed() =>
        System.DateTime.Now.Ticks ^ (System.Environment.TickCount << 16);

    void LoadFromSaved(SavedGameState s) {
        int size = s.gridRows.Length;
        grid = new char[size, size];
        for (int r = 0; r < size; r++)
            for (int c = 0; c < size; c++)
                grid[r, c] = s.gridRows[r][c];

        placedWords = s.placedWords;
        foundWords = new List<FoundWord>();
        // Saved blob keeps found word strings only — reassign palette colors deterministically by replay order.
        int reIdx = 0;
        foreach (var w in s.foundWords) {
            var pw = placedWords.Find(p => p.word == w);
            if (pw == null) continue;
            var color = AppColors.HighlightColors[reIdx++ % AppColors.HighlightColors.Length];
            foundWords.Add(new FoundWord { word = pw.word, colorPacked = PackColor(color) });
        }
        elapsedSec = s.elapsedSeconds;
        hintsUsed = s.hintsUsed;
        colorIndex = s.colorIndex;
    }

    List<string> ExtractPlacedWords() {
        var list = new List<string>(placedWords.Count);
        foreach (var pw in placedWords) list.Add(pw.word);
        return list;
    }

    void OnSelectionEnd(List<CellSelection> cells) {
        if (paused || complete) return;
        string word = BuildString(cells);
        string reversed = Reverse(word);
        foreach (var pw in placedWords) {
            if (foundWords.Exists(f => f.word == pw.word)) continue;
            if (pw.word == word || pw.word == reversed) { MarkFound(pw, cells); return; }
        }
        ServiceLocator.Sound?.Play(SoundEvent.MISS);
        HapticManager.Light();
    }

    void MarkFound(PlacedWord pw, List<CellSelection> cells) {
        var color = AppColors.HighlightColors[colorIndex++ % AppColors.HighlightColors.Length];
        foundWords.Add(new FoundWord {
            word = pw.word,
            startRow = cells[0].row, startCol = cells[0].col,
            endRow   = cells[^1].row, endCol   = cells[^1].col,
            colorPacked = PackColor(color)
        });
        foreach (var c in cells) gridView.Tile(c.row, c.col).PlayFound(color);
        int comboLvl = combo.OnWordFound();
        ServiceLocator.Sound?.Play(SoundEvent.FOUND, pitch: 1f + (comboLvl - 1) * 0.08f);
        HapticManager.Success();
        if (comboLvl >= 2) {
            comboBanner?.Show(comboLvl);
            HapticManager.Combo(comboLvl);
        }
        wordList?.MarkFound(pw.word, color);
        UpdateProgress();
        if (foundWords.Count == placedWords.Count) Complete();
    }

    void UpdateProgress() {
        if (progressLine == null) return;
        progressLine.text = $"FOUND: {foundWords.Count} / {placedWords.Count}";
    }

    // Counts UP with no limit — the game can't be lost to the clock. Elapsed
    // time only decides stars on completion (fast = 3★, slower = fewer).
    IEnumerator TickTimer() {
        var wait = new WaitForSeconds(1f);
        int budget = level.difficulty.TimeBonusSec();
        timer?.SetTime(elapsedSec, budget);
        while (!complete) {
            yield return wait;
            if (paused || complete) continue;
            elapsedSec++;
            timer?.SetTime(elapsedSec, budget);
        }
    }

    // Backing out to Android home / app switch: freeze the clock and persist
    // the board so a returning player continues exactly where they left off.
    void OnApplicationPause(bool pausedByOS) {
        if (!pausedByOS || complete || grid == null || placedWords == null) return;
        if (!gameObject.activeInHierarchy) return;
        ServiceLocator.GameState?.Save(BuildSaveBlob());
    }

    public void UseHint() {
        if (paused || complete || placedWords == null) return;
        var remaining = placedWords.FindAll(p => !foundWords.Exists(f => f.word == p.word));
        if (remaining.Count == 0) return;

        var target = remaining[Random.Range(0, remaining.Count)];
        gridView.Tile(target.startRow, target.startCol).PlayHintPulse();
        hintsUsed++;
        ServiceLocator.Sound?.Play(SoundEvent.HINT);
        HapticManager.Light();
    }

    public void Pause() {
        if (complete) return;
        paused = true;
        ServiceLocator.Sound?.Play(SoundEvent.PAUSE);
        HapticManager.Tick();
        if (pauseDialog != null) pauseDialog.gameObject.SetActive(true);
    }

    void ClosePauseDialog() {
        if (pauseDialog != null) pauseDialog.gameObject.SetActive(false);
        paused = false;
    }

    // Restart = shuffle: same words, brand-new random layout, timer back to 0.
    public void Restart() {
        ServiceLocator.GameState?.Delete(topicId, levelId);
        if (timerLoop != null) StopCoroutine(timerLoop);
        pendingShuffleSeed = NewShuffleSeed();
        Open(topicId, levelId);
    }

    public void SaveAndQuit() {
        if (timerLoop != null) StopCoroutine(timerLoop);

        if (complete) {
            ServiceLocator.GameState?.Delete(topicId, levelId);
        } else if (grid != null && placedWords != null) {
            ServiceLocator.GameState?.Save(BuildSaveBlob());
        }

        ServiceLocator.Router?.Show(isDailyRun ? Routes.Home : Routes.LevelSelect);
    }

    SavedGameState BuildSaveBlob() {
        int size = grid.GetLength(0);
        var rows = new string[size];
        for (int r = 0; r < size; r++) {
            var arr = new char[size];
            for (int c = 0; c < size; c++) arr[c] = grid[r, c];
            rows[r] = new string(arr);
        }
        var foundNames = new List<string>(foundWords.Count);
        foreach (var f in foundWords) foundNames.Add(f.word);
        return new SavedGameState {
            topicId = topicId,
            levelId = levelId,
            gridRows = rows,
            placedWords = placedWords,
            foundWords = foundNames,
            elapsedSeconds = elapsedSec,
            hintsUsed = hintsUsed,
            colorIndex = colorIndex
        };
    }

    void Complete() {
        complete = true;
        if (timerLoop != null) StopCoroutine(timerLoop);

        int stars = ScoreCalculator.ComputeStars(elapsedSec, level.difficulty, hintsUsed);
        int xp    = ScoreCalculator.ComputeXp(stars, level.difficulty, placedWords.Count);

        var result = new LevelResult {
            topicId      = topicId,
            levelId      = levelId,
            stars        = stars,
            timeSeconds  = elapsedSec,
            xpEarned     = xp,
            hintsUsed    = hintsUsed,
            completedAt  = System.DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        // High-score check BEFORE recording — beat the stored best on stars,
        // or match stars with a faster time, and the sheet celebrates it.
        bool isNewBest = true;
        var progress = ServiceLocator.Progress?.Load();
        if (progress?.bestResults != null &&
            progress.bestResults.TryGetValue($"{topicId}:{levelId}", out var prev) && prev != null) {
            isNewBest = stars > prev.stars ||
                        (stars == prev.stars && elapsedSec < prev.timeSeconds);
        }

        ServiceLocator.Progress?.RecordLevelResult(result);
        ServiceLocator.GameState?.Delete(topicId, levelId);

        if (isDailyRun) {
            ServiceLocator.Daily?.MarkCompleted(stars);
            isDailyRun = false;
        }

        ServiceLocator.Sound?.Play(SoundEvent.WIN);
        HapticManager.Win();

        var sheet = FindAnyObjectByType<LevelCompleteView>(FindObjectsInactive.Include);
        ServiceLocator.Router?.Show(Routes.LevelComplete);
        sheet?.Open(result, isNewBest);
    }

    // ─── helpers ───────────────────────────────────────────────────────────

    string BuildString(List<CellSelection> cells) {
        var sb = new System.Text.StringBuilder(cells.Count);
        foreach (var c in cells) sb.Append(grid[c.row, c.col]);
        return sb.ToString();
    }

    static string Reverse(string s) {
        var arr = s.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }

    static long PackColor(Color c) {
        long a = (long)(Mathf.Clamp01(c.a) * 255f);
        long r = (long)(Mathf.Clamp01(c.r) * 255f);
        long g = (long)(Mathf.Clamp01(c.g) * 255f);
        long b = (long)(Mathf.Clamp01(c.b) * 255f);
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    static Color UnpackColor(long packed) {
        float a = ((packed >> 24) & 0xFF) / 255f;
        float r = ((packed >> 16) & 0xFF) / 255f;
        float g = ((packed >> 8)  & 0xFF) / 255f;
        float b = (packed         & 0xFF) / 255f;
        return new Color(r, g, b, a);
    }
}
