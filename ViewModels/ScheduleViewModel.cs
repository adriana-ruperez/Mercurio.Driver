using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Views; 
using System.Diagnostics;
using System.Web; 
using Microsoft.Maui.Storage;

namespace Mercurio.Driver.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _runLogin;

        //[ObservableProperty]
        //private string _vehicleLogin;

        [ObservableProperty]
        private bool _isBusy;

        public ScheduleViewModel()
        {
            RunLogin = Preferences.Get("runLogin", "");
        }

        [RelayCommand]
        private async Task GoToTodaySchedule()
        {
            if (IsBusy) return; // Avoid multiple clicks
            RunLogin = NormalizeRunLogin(RunLogin);

            if (string.IsNullOrWhiteSpace(RunLogin))
            {
                await Shell.Current.DisplayAlert("Input Required", "Please enter a Run Login.", "OK");
                return;
            }

            try
            {
                IsBusy = true; 

                Preferences.Set("runLogin", RunLogin);

                var navigationParameter = new Dictionary<string, object>
                {
                    { "runLogin", RunLogin }
                };

                await Shell.Current.GoToAsync(nameof(TodaySchedulePage), navigationParameter);
            }
            catch (Exception ex)
            {               
                Debug.WriteLine($"Error while browsing: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "The schedule page could not be opened.", "OK");
            }
            finally
            {
                IsBusy = false; 
            }      
        }

        [RelayCommand]
        private async Task GoToFutureSchedule()
        {
            if (IsBusy) return;
            RunLogin = NormalizeRunLogin(RunLogin);

            if (string.IsNullOrWhiteSpace(RunLogin))
            {
                await Shell.Current.DisplayAlert("Input Required", "Please enter a Run Login.", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                Preferences.Set("runLogin", RunLogin);
                await Shell.Current.GoToAsync(nameof(FutureSchedulePage), new Dictionary<string, object>
                {
                    { "runLogin", RunLogin }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to future schedule: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string NormalizeRunLogin(string input)
        {
            if (input == null) return "";

            var s = input.Trim();
            s = s.Replace('\u00A0', ' ').Trim();   // NBSP -> espacio normal
            s = s.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();

            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s;
        }
    }
}