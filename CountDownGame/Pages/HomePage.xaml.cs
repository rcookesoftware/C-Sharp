using Microsoft.Maui.Controls;

namespace CountDownGame.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnStartGameClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//game");

    private async void OnViewHistoryClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//history");
}


