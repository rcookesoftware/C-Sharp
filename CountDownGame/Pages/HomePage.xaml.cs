using Microsoft.Maui.Controls;

namespace CountDownGame.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnStartGameTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//game");

    private async void OnViewHistoryTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//history");

    private async void OnSettingsTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//settings");
}

