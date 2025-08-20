using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CountDownGame.Models;
using CountDownGame.Services;

namespace CountDownGame.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly SettingsService _svc = new();
    GameSettings _model = new();

    public string Theme
    {
        get => _model.Theme;
        set { if (_model.Theme != value) { _model.Theme = value; Raise(); } }
    }

    public int MaxRounds
    {
        get => _model.MaxRounds;
        set { if (_model.MaxRounds != value) { _model.MaxRounds = value; Raise(); } }
    }

    public int RoundSeconds
    {
        get => _model.RoundSeconds;
        set { if (_model.RoundSeconds != value) { _model.RoundSeconds = value; Raise(); } }
    }

    public ICommand SaveCommand { get; }
    public ICommand ResetDefaultsCommand { get; }

    public SettingsViewModel()
    {
        SaveCommand = new Command(async () => await SaveAsync());
        ResetDefaultsCommand = new Command(ResetDefaults);
    }

    public async Task LoadAsync()
    {
        _model = await _svc.LoadAsync();
        Raise(nameof(Theme));
        Raise(nameof(MaxRounds));
        Raise(nameof(RoundSeconds));
    }

    async Task SaveAsync() => await _svc.SaveAsync(_model);

    void ResetDefaults()
    {
        _model = new GameSettings(); // resets to defaults
        Raise(nameof(Theme));
        Raise(nameof(MaxRounds));
        Raise(nameof(RoundSeconds));
    }
}

