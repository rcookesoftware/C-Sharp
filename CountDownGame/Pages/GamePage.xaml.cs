using Microsoft.Maui.Controls;
using CountDownGame.ViewModels;

namespace CountDownGame.Pages;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel(); // simple for now; we can swap to DI later
    }
}

