using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(RunLogin), "runLogin")]
    public partial class FutureScheduleViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;

        [ObservableProperty]
        private ObservableCollection<ScheduleDto> _events;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _runLogin;

        [ObservableProperty]
        private bool _hasEvents;

        [ObservableProperty]
        private bool _showNoEventsMessage;

        public FutureScheduleViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
            Events = new ObservableCollection<ScheduleDto>();
        }

        private void UpdateUiState()
        {
            HasEvents = Events.Any();
            ShowNoEventsMessage = !HasEvents && !IsBusy;
        }

        [RelayCommand]
        private async Task LoadEventsAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(RunLogin))
            {
                RunLogin = Preferences.Get("runLogin", "");
                if (string.IsNullOrWhiteSpace(RunLogin))
                    return;
            }

            try
            {
                IsBusy = true;
                UpdateUiState();
                
                var futureEvents = await _scheduleService.GetFutureSchedulesByRunAsync(RunLogin);

                Events.Clear();
                foreach (var ev in futureEvents)
                {
                    Events.Add(ev);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading future events: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not load the future schedule.", "OK");
            }
            finally
            {
                IsBusy = false;
                UpdateUiState();
            }
        }

        [RelayCommand]
        private async Task SelectEvent(ScheduleDto selectedEvent)
        {
            if (selectedEvent == null) return;
           
            await Shell.Current.GoToAsync(nameof(FutureDetailPage), new Dictionary<string, object>
            {
                { "EventDetail", selectedEvent }
            });
        }

        partial void OnRunLoginChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Preferences.Set("runLogin", value);
                _ = LoadEventsAsync();
            }
        }
    }
}