using Mercurio.Driver.ViewModels;

namespace Mercurio.Driver.Views;

public partial class TodaySchedulePage : ContentPage
{
    public TodaySchedulePage(TodayScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TodayScheduleViewModel vm && vm.LoadEventsCommand.CanExecute(null))
        {
            vm.LoadEventsCommand.Execute(null);
        }
    }

    private async void OnBackOrMenuClicked(object sender, EventArgs e)
    {
        // Si hay back stack, vuelve. Si no, abre el menú.
        var navCount = Shell.Current?.Navigation?.NavigationStack?.Count ?? 0;
        if (navCount > 1)
            await Shell.Current.GoToAsync("..");
        else
            Shell.Current.FlyoutIsPresented = true;
    }

    private void OnMenuClicked(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = true;
    }
}
