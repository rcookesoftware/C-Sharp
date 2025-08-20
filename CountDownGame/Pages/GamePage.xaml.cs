using Microsoft.Maui.Controls;
using CountDownGame.ViewModels;
using CountDownGame.Services;

namespace CountDownGame.Pages;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel(new DictionaryService());
    }
}

