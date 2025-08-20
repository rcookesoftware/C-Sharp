using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using CountDownGame.Models;

namespace CountDownGame.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public Player Player1 { get; } = new() { Name = "" };
    public Player Player2 { get; } = new() { Name = "" };

    public string Player1Name { get => Player1.Name; set { if (Player1.Name != value) { Player1.Name = value; Raise(); } } }
    public string Player2Name { get => Player2.Name; set { if (Player2.Name != value) { Player2.Name = value; Raise(); } } }
    public int Player1Score { get => Player1.Score; set { if (Player1.Score != value) { Player1.Score = value; Raise(); } } }
    public int Player2Score { get => Player2.Score; set { if (Player2.Score != value) { Player2.Score = value; Raise(); } } }

    public GameSettings Settings { get; } = new();

    int _currentRound = 1;
    public int CurrentRound
    {
        get => _currentRound;
        set { if (_currentRound != value) { _currentRound = value; Raise(); Raise(nameof(RoundDisplay)); } }
    }

    public string RoundDisplay => $"Round {CurrentRound} / {Settings.MaxRounds} · {Settings.RoundSeconds}s";

    public ObservableCollection<char> Letters { get; } = new();
    public string LettersCountDisplay => $"{Letters.Count} / 9 letters";

    public ICommand AddVowelCommand { get; }
    public ICommand AddConsonantCommand { get; }
    public ICommand ClearLettersCommand { get; }

    // --- Real Countdown bags (built & shuffled at start/reset) ---
    char[] _vowels = Array.Empty<char>();
    int _vowelIndex = 0;

    char[] _consonants = Array.Empty<char>();
    int _consIndex = 0;

    public GameViewModel()
    {
        AddVowelCommand = new Command(AddVowel);
        AddConsonantCommand = new Command(AddConsonant);
        ClearLettersCommand = new Command(ClearLetters);

        ResetBags(); // build + shuffle the bags once at startup
    }

    void ResetBags()
    {
        _vowels = BuildVowelBag();
        _vowelIndex = 0;
        _consonants = BuildConsonantBag();
        _consIndex = 0;
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
        Random.Shared.Shuffle(arr); // .NET 8 shuffle
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
        Random.Shared.Shuffle(arr); // .NET 8 shuffle
        return arr;
    }

    void AddVowel()
    {
        if (Letters.Count >= 9) return;

        // If we’ve consumed the bag, reshuffle a fresh one.
        if (_vowelIndex >= _vowels.Length)
        {
            _vowels = BuildVowelBag();
            _vowelIndex = 0;
        }

        Letters.Add(_vowels[_vowelIndex++]);
        Raise(nameof(LettersCountDisplay));
    }

    void AddConsonant()
    {
        if (Letters.Count >= 9) return;

        if (_consIndex >= _consonants.Length)
        {
            _consonants = BuildConsonantBag();
            _consIndex = 0;
        }

        Letters.Add(_consonants[_consIndex++]);
        Raise(nameof(LettersCountDisplay));
    }

    void ClearLetters()
    {
        Letters.Clear();
        Raise(nameof(LettersCountDisplay));

        // Optional: refresh the bags for a fresh round feel
        ResetBags();
    }
}
