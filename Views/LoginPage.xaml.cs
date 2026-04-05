using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class LoginPage : ContentPage
{
    LoginViewModel _viewModel;
    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;
        //BindingContext = new LoginViewModel();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel?.OnAppearing(); // Call the ViewModel method
    }
}