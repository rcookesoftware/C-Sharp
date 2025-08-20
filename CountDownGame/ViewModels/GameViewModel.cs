using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls;
using CountDownGame.Models;
using CountDownGame.Services;

namespace CountDownGame.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // --- Services ---
    private readonly DictionaryService _dictService;
    private readonly GameStorageService _storage;
    private readonly SettingsService _settingsSvc = new();

    // --- Constructor ---
    public GameViewModel(DictionaryService? dictService = null, GameStorageService? storage = null)
    {
        _dictService = dictService ?? new DictionaryService();
        _storage = storage ?? new GameStorageService();

        AddVowelCommand = new Command(AddVowel);
        AddConsonantCommand = new Command(AddConsonant);
        ClearLettersCommand = new Command(ClearLetters);
        StartRoundCommand = new Command(StartRound);
        ScoreRoundCommand = new Command(async () => await ScoreRoundAsync());

        Letters.CollectionChanged += OnLettersChanged;

        ResetBags();

        // Load saved settings (theme is applied inside the service)
        var latest = _settingsSvc.Load();
        Settings.MaxRounds = latest.MaxRounds;
        Settings.RoundSeconds = latest.RoundSeconds;
        Raise(nameof(RoundDisplay));

        // Download/load dictionary in background
        _ = LoadDictionaryAsync();
    }

    // --- Players & Settings ---
    public Player Player1 { get; } = new();
    public Player Player2 { get; } = new();

    public string Player1Name
    {
        get => Player1.Name;
        set { if (Player1.Name != value) { Player1.Name = value; Raise(); } }
    }

    public string Player2Name
    {
        get => Player2.Name;
        set { if (Player2.Name != value) { Player2.Name = value; Raise(); } }
    }

    public int Player1Score
    {
        get => Player1.Score;
        set { if (Player1.Score != value) { Player1.Score = value; Raise(); } }
    }

    public int Player2Score
    {
        get => Player2.Score;
        set { if (Player2.Score != value) { Player2.Score = value; Raise(); } }
    }

    public GameSettings Settings { get; } = new(); // defaults (overridden by saved values above)

    // --- Round State ---
    int _currentRound = 1;
    public int CurrentRound
    {
        get => _currentRound;
        set { if (_currentRound != value) { _currentRound = value; Raise(); Raise(nameof(RoundDisplay)); } }
    }

    public string RoundDisplay => $"Round {CurrentRound} / {Settings.MaxRounds}";

    // --- Letters ---
    public ObservableCollection<char> Letters { get; } = new();
    public string LettersCountDisplay => $"{Letters.Count} / 9 letters";

    // --- Timer / Phases ---
    IDispatcherTimer? _timer;
    int _timeLeft;
    public int TimeLeft
    {
        get => _timeLeft;
        private set { _timeLeft = value; Raise(); Raise(nameof(TimerDisplay)); }
    }

    public string TimerDisplay => IsRoundActive ? $"{TimeLeft}s"
                               : (IsEntryPhase ? "Enter words" : "Ready");

    bool _isRoundActive;
    public bool IsRoundActive
    {
        get => _isRoundActive;
        private set
        {
            if (_isRoundActive != value)
            {
                _isRoundActive = value;
                Raise();
                Raise(nameof(StartEnabled));
                Raise(nameof(CanPickLetters));
                Raise(nameof(CanClearLetters));
            }
        }
    }

    bool _isEntryPhase;
    public bool IsEntryPhase
    {
        get => _isEntryPhase;
        private set
        {
            if (_isEntryPhase != value)
            {
                _isEntryPhase = value;
                Raise();
                Raise(nameof(StartEnabled));
                Raise(nameof(CanPickLetters));
            }
        }
    }

    public bool StartEnabled => Letters.Count == 9 && !IsRoundActive && !IsEntryPhase;
    public bool CanPickLetters => !IsRoundActive && !IsEntryPhase && Letters.Count < 9;
    public bool CanClearLetters => !IsRoundActive;

    // --- Word Inputs / Status ---
    string _p1Word = string.Empty;
    public string Player1WordInput { get => _p1Word; set { if (_p1Word != value) { _p1Word = value; Raise(); } } }

    string _p2Word = string.Empty;
    public string Player2WordInput { get => _p2Word; set { if (_p2Word != value) { _p2Word = value; Raise(); } } }

    string _status = string.Empty;
    public string StatusMessage { get => _status; set { if (_status != value) { _status = value; Raise(); } } }

    // --- Dictionary State ---
    HashSet<string>? _dictionary;
    bool _dictReady;
    public bool IsDictionaryReady { get => _dictReady; private set { if (_dictReady != value) { _dictReady = value; Raise(); } } }

    string _dictStatus = "Loading dictionary…";
    public string DictionaryStatus { get => _dictStatus; private set { if (_dictStatus != value) { _dictStatus = value; Raise(); } } }

    // --- History (this game’s rounds) ---
    public ObservableCollection<RoundResult> RoundHistory { get; } = new();

    // --- Commands ---
    public ICommand AddVowelCommand { get; }
    public ICommand AddConsonantCommand { get; }
    public ICommand ClearLettersCommand { get; }
    public ICommand StartRoundCommand { get; }
    public ICommand ScoreRoundCommand { get; }

    // --- Letter Bags ---
    char[] _vowels = Array.Empty<char>(); int _vowelIndex = 0;
    char[] _consonants = Array.Empty<char>(); int _consIndex = 0;

    // ===== Dictionary =====
    async Task LoadDictionaryAsync()
    {
        try
        {
            DictionaryStatus = "Downloading dictionary (first run may take a moment)…";
            _dictionary = await _dictService.LoadOrDownloadAsync();
            IsDictionaryReady = true;
            DictionaryStatus = $"Dictionary loaded ({_dictionary.Count:N0} words)";
        }
        catch (Exception ex)
        {
            IsDictionaryReady = false;
            DictionaryStatus = $"Dictionary unavailable — scoring by length only. ({ex.GetType().Name})";
        }
    }

    // ===== Letter collection change =====
    void OnLettersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Raise(nameof(LettersCountDisplay));
        Raise(nameof(StartEnabled));
        Raise(nameof(CanPickLetters));
    }

    // ===== Timer =====
    void EnsureTimer()
    {
        if (_timer != null) return;
        _timer = Application.Current?.Dispatcher.CreateTimer();
        if (_timer == null) return;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, __) => TickTimer();
    }

    void TickTimer()
    {
        if (TimeLeft > 0)
        {
            TimeLeft--;
            if (TimeLeft == 0)
            {
                _timer?.Stop();
                IsRoundActive = false;
                IsEntryPhase = true;
                StatusMessage = "⏳ Time! Enter your words.";
            }
        }
    }

    // ===== Bags =====
    void ResetBags()
    {
        _vowels = BuildVowelBag(); _vowelIndex = 0;
        _consonants = BuildConsonantBag(); _consIndex = 0;
    }

    static char[] BuildVowelBag()
    {
        // A×15, E×21, I×13, O×13, U×5  (Total 67)
        var list = new List<char>(67);
        list.AddRange(Enumerable.Repeat('A', 15));
        list.AddRange(Enumerable.Repeat('E', 21));
        list.AddRange(Enumerable.Repeat('I', 13));
        list.AddRange(Enumerable.Repeat('O', 13));
        list.AddRange(Enumerable.Repeat('U', 5));
        var arr = list.ToArray();
        Random.Shared.Shuffle(arr);
        return arr;
    }

    static char[] BuildConsonantBag()
    {
        // B2 C3 D6 F2 G3 H2 J1 K1 L5 M4 N8 P4 Q1 R9 S9 T9 V1 W1 X1 Y1 Z1  (Total 74)
        var list = new List<char>(74);
        list.AddRange(Enumerable.Repeat('B', 2));
        list.AddRange(Enumerable.Repeat('C', 3));
        list.AddRange(Enumerable.Repeat('D', 6));
        list.AddRange(Enumerable.Repeat('F', 2));
        list.AddRange(Enumerable.Repeat('G', 3));
        list.AddRange(Enumerable.Repeat('H', 2));
        list.AddRange(Enumerable.Repeat('J', 1));
        list.AddRange(Enumerable.Repeat('K', 1));
        list.AddRange(Enumerable.Repeat('L', 5));
        list.AddRange(Enumerable.Repeat('M', 4));
        list.AddRange(Enumerable.Repeat('N', 8));
        list.AddRange(Enumerable.Repeat('P', 4));
        list.AddRange(Enumerable.Repeat('Q', 1));
        list.AddRange(Enumerable.Repeat('R', 9));
        list.AddRange(Enumerable.Repeat('S', 9));
        list.AddRange(Enumerable.Repeat('T', 9));
        list.AddRange(Enumerable.Repeat('V', 1));
        list.AddRange(Enumerable.Repeat('W', 1));
        list.AddRange(Enumerable.Repeat('X', 1));
        list.AddRange(Enumerable.Repeat('Y', 1));
        list.AddRange(Enumerable.Repeat('Z', 1));
        var arr = list.ToArray();
        Random.Shared.Shuffle(arr);
        return arr;
    }

    void AddVowel()
    {
        if (!CanPickLetters) return;
        if (_vowelIndex >= _vowels.Length) { _vowels = BuildVowelBag(); _vowelIndex = 0; }
        Letters.Add(_vowels[_vowelIndex++]);
    }

    void AddConsonant()
    {
        if (!CanPickLetters) return;
        if (_consIndex >= _consonants.Length) { _consonants = BuildConsonantBag(); _consIndex = 0; }
        Letters.Add(_consonants[_consIndex++]);
    }

    void ClearLetters()
    {
        if (!CanClearLetters) return;
        Letters.Clear();
        ResetBags();
        StatusMessage = string.Empty;
    }

    // ===== Round control =====
    void StartRound()
    {
        if (!StartEnabled) return;

        // Re-read latest settings before each round (small file; fast)
        var latest = _settingsSvc.Load();
        Settings.MaxRounds = latest.MaxRounds;
        Settings.RoundSeconds = latest.RoundSeconds;
        Raise(nameof(RoundDisplay));

        IsEntryPhase = false;
        IsRoundActive = true;
        TimeLeft = Settings.RoundSeconds;
        StatusMessage = IsDictionaryReady ? "Round started — good luck!"
                                          : "Round started — dictionary still loading (scoring by length only).";
        EnsureTimer();
        _timer?.Stop();
        _timer?.Start();
    }

    async Task ScoreRoundAsync()
    {
        if (!IsEntryPhase) return;

        string w1 = (Player1WordInput ?? string.Empty).Trim().ToUpperInvariant();
        string w2 = (Player2WordInput ?? string.Empty).Trim().ToUpperInvariant();

        bool w1LettersOk = UsesOnlyGivenLetters(w1, Letters);
        bool w2LettersOk = UsesOnlyGivenLetters(w2, Letters);
        bool w1Alpha = IsAlphabetic(w1);
        bool w2Alpha = IsAlphabetic(w2);

        bool w1DictOk = !IsDictionaryReady || (_dictionary?.Contains(w1) == true);
        bool w2DictOk = !IsDictionaryReady || (_dictionary?.Contains(w2) == true);

        int len1 = (w1LettersOk && w1Alpha && w1DictOk) ? w1.Length : 0;
        int len2 = (w2LettersOk && w2Alpha && w2DictOk) ? w2.Length : 0;

        int p1Pts = 0, p2Pts = 0;
        if (len1 == len2) { p1Pts = len1; p2Pts = len2; }
        else if (len1 > len2) { p1Pts = len1; } else { p2Pts = len2; }

        Player1Score += p1Pts;
        Player2Score += p2Pts;

        RoundHistory.Add(new RoundResult
        {
            Letters = Letters.ToList(),
            Player1Word = w1,
            Player2Word = w2,
            Player1Points = p1Pts,
            Player2Points = p2Pts
        });

        StatusMessage = $"Round {CurrentRound}: P1 '{w1}' (+{p1Pts}) vs P2 '{w2}' (+{p2Pts})";
        if (IsDictionaryReady && (!w1DictOk || !w2DictOk))
            StatusMessage += "  |  (Invalid word removed by dictionary)";

        CurrentRound++;
        IsEntryPhase = false;
        Player1WordInput = string.Empty;
        Player2WordInput = string.Empty;
        Letters.Clear();
        ResetBags();

        if (CurrentRound > Settings.MaxRounds)
        {
            string p1Name = string.IsNullOrWhiteSpace(Player1Name) ? "Player 1" : Player1Name;
            string p2Name = string.IsNullOrWhiteSpace(Player2Name) ? "Player 2" : Player2Name;
            string winner = Player1Score == Player2Score ? "Draw!"
                             : (Player1Score > Player2Score ? $"{p1Name} wins!" : $"{p2Name} wins!");
            StatusMessage += $"  |  Game over — {winner}";

            // Save the finished game to history
            var result = new GameResult
            {
                PlayedAt = DateTime.Now,
                Player1Name = p1Name,
                Player1Score = Player1Score,
                Player2Name = p2Name,
                Player2Score = Player2Score,
                Rounds = RoundHistory.ToList()
            };
            try { await _storage.AddAsync(result); }
            catch { }
        }
        else
        {
            StatusMessage += "  |  Pick new letters for the next round.";
        }
    }

    // ===== Helpers =====
    static bool IsAlphabetic(string s) => s.All(c => c is >= 'A' and <= 'Z');

    static bool UsesOnlyGivenLetters(string word, IEnumerable<char> letters)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;

        var avail = new int[26];
        foreach (var c in letters)
            if (c is >= 'A' and <= 'Z') avail[c - 'A']++;

        foreach (var c in word)
        {
            if (c is < 'A' or > 'Z') return false;
            int idx = c - 'A';
            if (avail[idx] == 0) return false;
            avail[idx]--;
        }
        return true;
    }
}
