using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CountDownGame.Models;

namespace CountDownGame.ViewModels;

public class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public Player Player1 { get; } = new() { Name = "" };
    public Player Player2 { get; } = new() { Name = "" };

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

    readonly Random _rng = new();

    public GameViewModel()
    {
        AddVowelCommand = new Command(AddVowel);
        AddConsonantCommand = new Command(AddConsonant);
        ClearLettersCommand = new Command(ClearLetters);
    }

    void AddVowel()
    {
        if (Letters.Count >= 9) return;
        // TEMP stub so you can see the UI working; real distribution next step per brief. 
        char[] sampleVowels = { 'A', 'E', 'I', 'O', 'U', 'E', 'A' };
        Letters.Add(sampleVowels[_rng.Next(sampleVowels.Length)]);
        Raise(nameof(LettersCountDisplay));
    }

    void AddConsonant()
    {
        if (Letters.Count >= 9) return;
        // TEMP stub; we’ll replace with Countdown consonant bag next step.
        char[] sampleCons = { 'R', 'S', 'T', 'L', 'N', 'D' };
        Letters.Add(sampleCons[_rng.Next(sampleCons.Length)]);
        Raise(nameof(LettersCountDisplay));
    }

    void ClearLetters()
    {
        Letters.Clear();
        Raise(nameof(LettersCountDisplay));
    }
}
