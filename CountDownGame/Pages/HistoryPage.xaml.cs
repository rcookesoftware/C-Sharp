using Microsoft.Maui.Controls;
using CountDownGame.ViewModels;
using System.Threading.Tasks;

namespace CountDownGame.Pages;

public partial class HistoryPage : ContentPage
{
    readonly HistoryViewModel _vm = new();

    public HistoryPage()
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

