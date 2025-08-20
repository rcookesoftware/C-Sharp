using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Specialized;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;
using CountDownGame.Models;

namespace CountDownGame.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // --- Players & settings ---
    public Player Player1 { get; } = new() { Name = "" };
    public Player Player2 { get; } = new() { Name = "" };
    public string Player1Name { get => Player1.Name; set { if (Player1.Name != value) { Player1.Name = value; Raise(); } } }
    public string Player2Name { get => Player2.Name; set { if (Player2.Name != value) { Player2.Name = value; Raise(); } } }
    public int Player1Score { get => Player1.Score; set { if (Player1.Score != value) { Player1.Score = value; Raise(); } } }
    public int Player2Score { get => Player2.Score; set { if (Player2.Score != value) { Player2.Score = value; Raise(); } } }

    public GameSettings Settings { get; } = new(); // defaults: 6 rounds, 30s

    // --- Round state ---
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

    // --- Timer & phases ---
    IDispatcherTimer? _timer;
    int _timeLeft;
    public int TimeLeft { get => _timeLeft; private set { _timeLeft = value; Raise(); Raise(nameof(TimerDisplay)); } }
    public string TimerDisplay => IsRoundActive ? $"{TimeLeft}s" : (IsEntryPhase ? "Enter words" : "Ready");

    bool _isRoundActive;
    public bool IsRoundActive
    {
        get => _isRoundActive;
        private set { if (_isRoundActive != value) { _isRoundActive = value; Raise(); Raise(nameof(StartEnabled)); Raise(nameof(CanPickLetters)); Raise(nameof(CanClearLetters)); } }
    }

    bool _isEntryPhase;
    public bool IsEntryPhase
    {
        get => _isEntryPhase;
        private set { if (_isEntryPhase != value) { _isEntryPhase = value; Raise(); Raise(nameof(StartEnabled)); Raise(nameof(CanPickLetters)); } }
    }

    public bool StartEnabled => Letters.Count == 9 && !IsRoundActive && !IsEntryPhase;
    public bool CanPickLetters => !IsRoundActive && !IsEntryPhase && Letters.Count < 9;
    public bool CanClearLetters => !IsRoundActive;

    // --- Word inputs & status ---
    string _p1Word = string.Empty;
    public string Player1WordInput { get => _p1Word; set { if (_p1Word != value) { _p1Word = value; Raise(); } } }

    string _p2Word = string.Empty;
    public string Player2WordInput { get => _p2Word; set { if (_p2Word != value) { _p2Word = value; Raise(); } } }

    string _status = string.Empty;
    public string StatusMessage { get => _status; set { if (_status != value) { _status = value; Raise(); } } }

    // --- History for later persistence ---
    public ObservableCollection<RoundResult> RoundHistory { get; } = new();

    // --- Commands ---
    public ICommand AddVowelCommand { get; }
    public ICommand AddConsonantCommand { get; }
    public ICommand ClearLettersCommand { get; }
    public ICommand StartRoundCommand { get; }
    public ICommand ScoreRoundCommand { get; }

    // --- Real letter bags (from Step 4) ---
    char[] _vowels = Array.Empty<char>();
    int _vowelIndex = 0;
    char[] _consonants = Array.Empty<char>();
    int _consIndex = 0;

    readonly Random _rng = new();

    public GameViewModel()
    {
        AddVowelCommand = new Command(AddVowel);
        AddConsonantCommand = new Command(AddConsonant);
        ClearLettersCommand = new Command(ClearLetters);
        StartRoundCommand = new Command(StartRound);
        ScoreRoundCommand = new Command(ScoreRound);

        Letters.CollectionChanged += OnLettersChanged;

        ResetBags(); // build + shuffle bags once at startup
    }

    void OnLettersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Raise(nameof(LettersCountDisplay));
        Raise(nameof(StartEnabled));
        Raise(nameof(CanPickLetters));
    }

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

    // ===== Letters =====
    void ResetBags()
    {
        _vowels = BuildVowelBag();
        _vowelIndex = 0;
        _consonants = BuildConsonantBag();
        _consIndex = 0;
    }

    static char[] BuildVowelBag()
    {
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

        // Prepare phase
        IsEntryPhase = false;
        IsRoundActive = true;
        TimeLeft = Settings.RoundSeconds;
        StatusMessage = "Round started — good luck!";

        EnsureTimer();
        _timer?.Stop();
        _timer?.Start();
    }

    void ScoreRound()
    {
        if (!IsEntryPhase) return;

        // Normalize inputs
        string w1 = (Player1WordInput ?? string.Empty).Trim().ToUpperInvariant();
        string w2 = (Player2WordInput ?? string.Empty).Trim().ToUpperInvariant();

        bool w1LettersOk = UsesOnlyGivenLetters(w1, Letters);
        bool w2LettersOk = UsesOnlyGivenLetters(w2, Letters);

        // TODO (Step 6): add dictionary check; for now assume any alphabetic string is potentially valid.
        bool w1Alpha = IsAlphabetic(w1);
        bool w2Alpha = IsAlphabetic(w2);

        int len1 = (w1LettersOk && w1Alpha) ? w1.Length : 0;
        int len2 = (w2LettersOk && w2Alpha) ? w2.Length : 0;

        int p1Pts = 0, p2Pts = 0;
        if (len1 == 0 && len2 == 0)
        {
            // nobody scores
        }
        else if (len1 == len2)
        {
            p1Pts = len1;
            p2Pts = len2;
        }
        else if (len1 > len2)
        {
            p1Pts = len1;
        }
        else
        {
            p2Pts = len2;
        }

        Player1Score += p1Pts;
        Player2Score += p2Pts;

        // Save round summary for history page later
        RoundHistory.Add(new RoundResult
        {
            Letters = Letters.ToList(),
            Player1Word = w1,
            Player2Word = w2,
            Player1Points = p1Pts,
            Player2Points = p2Pts
        });

        // Status text
        StatusMessage = $"Round {CurrentRound}: P1 '{w1}' (+{p1Pts}) vs P2 '{w2}' (+{p2Pts})";

        // Prepare next round
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
            string winner =
                Player1Score == Player2Score ? "Draw!" :
                (Player1Score > Player2Score ? $"{p1Name} wins!" : $"{p2Name} wins!");
            StatusMessage += $"  |  Game over — {winner}";
            // (We’ll save to disk on the History page in a later step.)
        }
        else
        {
            StatusMessage += "  |  Pick new letters for the next round.";
        }
    }

    static bool IsAlphabetic(string s) => s.All(c => c >= 'A' && c <= 'Z');

    static bool UsesOnlyGivenLetters(string word, IEnumerable<char> letters)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;

        var avail = new int[26];
        foreach (var c in letters)
        {
            if (c >= 'A' && c <= 'Z') avail[c - 'A']++;
        }

        foreach (var c in word)
        {
            if (c < 'A' || c > 'Z') return false;
            int idx = c - 'A';
            if (avail[idx] == 0) return false;
            avail[idx]--;
        }
        return true;
    }
}
