using Microsoft.Maui.Controls;
using CountDownGame.ViewModels;

namespace CountDownGame.Pages;

public partial class SettingsPage : ContentPage
{
    readonly SettingsViewModel _vm = new();

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}

