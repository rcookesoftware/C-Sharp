using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CountDownGame.Models;
using CountDownGame.Services;

namespace CountDownGame.ViewModels;

public class HistoryViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void Raise([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private readonly GameStorageService _storage = new();

    public ObservableCollection<GameResult> Games { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand ClearCommand { get; }

    public HistoryViewModel()
    {
        RefreshCommand = new Command(async () => await LoadAsync());
        ClearCommand = new Command(async () => await ClearAsync());
    }

    public async Task LoadAsync()
    {
        Games.Clear();
        var all = await _storage.LoadAllAsync();
        foreach (var g in all) Games.Add(g);
    }

    public async Task ClearAsync()
    {
        await _storage.ClearAsync();
        await LoadAsync();
    }
}

