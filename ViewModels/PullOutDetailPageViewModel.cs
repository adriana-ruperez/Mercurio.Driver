using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using System.Diagnostics;
using Mercurio.Driver.Converters;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;

namespace Mercurio.Driver.ViewModels
{
    // We use QueryProperty to receive the ScheduleDto object during navigation
    [QueryProperty(nameof(Event), "EventDetail")]
    [QueryProperty(nameof(IsFirstEvent), "IsFirstEvent")]
    [QueryProperty(nameof(PendingEventsCount), "PendingEventsCount")]
    [QueryProperty(nameof(SignatureSaved), "SignatureSaved")] // ✅ added (return from SignaturePage)
    public partial class PullOutDetailPageViewModel : ObservableObject, IDisposable, IQueryAttributable
    {
        private readonly IScheduleService _scheduleService;
        private readonly IGpsService _gpsService;
        private readonly IMapService _mapService;

        [ObservableProperty]
        private int pendingEventsCount;

        [ObservableProperty]
        private bool signatureSaved; // ✅ added

        private bool _awaitingSignature; // ✅ added (internal flow flag)

        [ObservableProperty]
        private ScheduleDto _event;

        [ObservableProperty]
        private Color _eventColor;

        private readonly ScheduleColorConverter _colorConverter = new();

        [ObservableProperty]
        private bool _isOdometerEntered;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OdometerOrPerformActionCommand))]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotTracking))]
        private bool isTracking;

        public bool IsNotTracking => !IsTracking;

        [ObservableProperty]
        private bool _isFirstEvent;

        // Controls the visibility of the Odometer/Perform action row
        [ObservableProperty]
        private bool _isPrimaryActionVisible;

        // Properties for dynamic texts in the UI
        [ObservableProperty]
        private string _pageTitle;

        [ObservableProperty]
        private string _mapActionText;


        [ObservableProperty]
        private bool _isSignatureEntered;

        [ObservableProperty]
        private bool _showOdometerAction;

        [ObservableProperty]
        private bool _showSignatureAction;

        [ObservableProperty]
        private bool _showPerformAction;


        public PullOutDetailPageViewModel(IScheduleService scheduleService, IGpsService gpsService, IMapService mapService)
        {
            _scheduleService = scheduleService;
            _gpsService = gpsService;
            _mapService = mapService;

            // Subscribe to service status changes
            if (_gpsService != null)
            {
                _gpsService.IsTrackingChanged += OnGpsTrackingChanged;
                // Synchronize initial state
                IsTracking = _gpsService.IsTracking;
            }
        }

        partial void OnIsFirstEventChanged(bool value) => UpdateUIState();
        //partial void OnEventChanged(ScheduleDto value) => UpdateUIState();

        private void UpdateUIState()
        {
            if (Event == null) return;

            // TÍTULOS Y COLORES
            PageTitle = Event.Name;
            MapActionText = $"Maps - {Event.Name} Address";
            EventColor = (Color)_colorConverter.Convert(Event, typeof(Color), null, System.Globalization.CultureInfo.CurrentCulture);

            // LÓGICA DE VISIBILIDAD CRÍTICA:
            // Se muestra si es Pull-out (que siempre suele ser el primero)
            // O si es Pull-in Y es el primero de la lista (porque ya se completó el resto)
            IsPrimaryActionVisible = Event.Name == "Pull-out" || (Event.Name == "Pull-in" && IsFirstEvent);

            // Refrescar los estados internos de los botones (Odometer/Signature/Perform)
            _ = RefreshStates();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("SignatureSaved", out var saved) && (bool)saved)
            {

                if (Event != null)
                {

                    Event.PassengerSignature = new byte[] { 1 };
                }
                _ = RefreshStates();
            }
        }

        /*public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("SignatureSaved", out var saved) && (bool)saved)
            {
                // Forzamos la recarga del objeto o simplemente marcamos como firmado
                IsSignatureEntered = true;
                _ = RefreshStates();
            }
        }*/


        /*public async Task ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("SignatureSaved"))
            {
                IsSignatureEntered = true;
                await RefreshStates();
            }
        }*/

        private async Task RefreshStates()
        {
            if (Event == null) return;

            IsOdometerEntered = Event.Odometer != null && Event.Odometer > -1;
            // We simulate the signature verification by looking if it already exists in the object
            IsSignatureEntered = !string.IsNullOrEmpty(Event.PassengerSignature?.ToString());

            if (Event.Name == "Pull-in")
            {
                // Pull-in sequence: Odometer -> Signature -> Perform
                ShowOdometerAction = !IsOdometerEntered;
                ShowSignatureAction = IsOdometerEntered && !IsSignatureEntered;
                ShowPerformAction = IsOdometerEntered && IsSignatureEntered;
            }
            else // Pull-out
            {
                // Normal sequence: Odometer -> Perform
                ShowOdometerAction = !IsOdometerEntered;
                ShowSignatureAction = false;
                ShowPerformAction = IsOdometerEntered;
            }

            // Reporting changes to ensure UI IsVisible is triggered
            OnPropertyChanged(nameof(ShowOdometerAction));
            OnPropertyChanged(nameof(ShowSignatureAction));
            OnPropertyChanged(nameof(ShowPerformAction));
        }
        [RelayCommand]
        private async Task GoToSignature()
        {
            if (IsBusy) return;

            await Shell.Current.GoToAsync(nameof(SignaturePage), new Dictionary<string, object>
        {
            { "ScheduleId", Event.Id },
            { "IsDriverSignature", true } // This activates the legal text
        });
        }

        [RelayCommand(CanExecute = nameof(CanExecuteAction))]
        private async Task OdometerOrPerformAction()
        {
            if (IsOdometerEntered)
            {
                // Logic for when the user presses "Perform"
                await PerformAction();
            }
            else
            {
                // Logic for when the user presses "Odometer"
                await EnterOdometer();
            }
        }

        private bool CanExecuteAction() => !IsBusy;

        private async Task<bool> CheckAndRequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // On iOS, if the user denies permission, they cannot be asked again.
                await Shell.Current.DisplayAlert("Permission Required", "Location permission was denied. Please activate it in the application settings.", "OK");
                return false;
            }

            // Request permission from the user
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status == PermissionStatus.Granted;
        }

        private void OnGpsTrackingChanged(bool isTracking)
        {
            // The service notifies us that its status has changed
            // We update the property in the UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsTracking = isTracking;
            });
        }

        public void Dispose()
        {
            // Unsubscribe from the event to avoid memory leaks
            if (_gpsService != null)
            {
                _gpsService.IsTrackingChanged -= OnGpsTrackingChanged;
            }
        }

        private async Task EnterOdometer()
        {
            // To avoid double clicks.
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var result = await Shell.Current.DisplayPromptAsync(
                    "Odometer",
                    "Enter Odometer Reading.",
                    "OK",
                    "Cancel",
                    keyboard: Keyboard.Numeric
                );

                if (string.IsNullOrWhiteSpace(result)) return;

                if (long.TryParse(result, out long odometerValue) && odometerValue > -1)
                {
                    Event.Odometer = odometerValue;

                    bool success = await _scheduleService.UpdateScheduleAsync(Event);

                    if (success)
                    {
                        // IsOdometerEntered = true;
                        await RefreshStates();
                        await Shell.Current.DisplayAlert("Success", "Odometer reading has been saved.", "OK");
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Could not save the odometer reading. Please try again.", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Invalid Input", "Please enter a valid number for the odometer.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PerformAction()
        {
            if (IsBusy) return;

            IsBusy = true;

            try
            {
                // ✅ Pull-out flow: unchanged (permission + perform)
                if (Event.Name == "Pull-out")
                {
                    var hasPermission = await CheckAndRequestLocationPermissionAsync();
                    if (!hasPermission)
                    {
                        // The user did not give permission, we cannot continue.
                        await Shell.Current.DisplayAlert("Permission Required", "Tracking cannot be started without location permission.", "OK");
                        return; // We leave the method
                    }

                    await CompletePerformAsync();
                    return;
                }

                // ✅ Pull-in flow: require odometer + signature before completing perform
                if (Event.Name == "Pull-in")
                {
                    // 1) Odometer first (reuses existing logic)
                    if (!IsOdometerEntered)
                    {
                        await EnterOdometer();
                        if (!IsOdometerEntered) return; // cancelled/invalid -> stop here
                    }

                    // 2) Signature next (navigate to SignaturePage if not saved yet)
                    if (!SignatureSaved)
                    {
                        _awaitingSignature = true;

                        await Shell.Current.GoToAsync(nameof(SignaturePage), new Dictionary<string, object>
                        {
                            { "ScheduleId", Event.Id }
                        });

                        return; // flow continues when SignatureSaved returns
                    }

                    // 3) Complete perform (stop tracking + update schedule + go back)
                    await CompletePerformAsync();
                    return;
                }

                // Fallback: keep original behavior for any other unexpected event name
                await CompletePerformAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error performing action: {ex.Message}");
                Event.Performed = false;
                await Shell.Current.DisplayAlert("Error", "An unexpected error occurred.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ✅ Added: same logic you already had inside PerformAction, extracted to avoid duplication
        private async Task CompletePerformAsync()
        {
            TimeSpan? oldEta = Event.ETA;
            Event.Perform = DateTime.Now.TimeOfDay;
            Event.Performed = true;
            Event.ETA = Event.Perform;

            bool success = await _scheduleService.UpdateScheduleAsync(Event);

            if (success)
            {
                if (Event.Name == "Pull-out")
                {
                    _gpsService.StartTracking(Event.VehicleRouteId);
                }
                else if (Event.Name == "Pull-in")
                {
                    _gpsService.StopTracking();
                }
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                Event.Perform = null;
                Event.Performed = false;
                Event.ETA = oldEta;
                await Shell.Current.DisplayAlert("Error", "The action could not be performed. Please check your connection and try again.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoToOdometer()
        {
            // Shows the text entry dialog
            var result = await Shell.Current.DisplayPromptAsync(
                "Odometer",
                "Enter Odometer Reading.",
                "OK",
                "Cancel",
                keyboard: Keyboard.Numeric // This displays a numeric keypad to the user
            );

            // The user pressed "Cancel" or left the field empty
            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            // We validate that the input is a number
            if (long.TryParse(result, out long odometerValue))
            {
                // The object in the ViewModel is updated
                Event.Odometer = odometerValue;

                // Service is called to save changes to the API
                bool success = await _scheduleService.UpdateScheduleAsync(Event);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Odometer reading has been saved.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Could not save the odometer reading. Please try again.", "OK");
                }
            }
            else
            {
                // The user entered something that is not a valid number
                await Shell.Current.DisplayAlert("Invalid Input", "Please enter a valid number for the odometer.", "OK");
            }

            // TODO: Implementar navegación a la página del Odómetro
            /*await Shell.Current.DisplayAlert("Función no implementada", "La navegación al odómetro aún no está disponible.", "OK");
            Debug.WriteLine("Navegando a la página del Odómetro...");*/
        }

        [RelayCommand]
        private async Task GoToMaps()
        {
            if (IsBusy || Event is null) return;

            IsBusy = true;
            try
            {
                await _mapService.LaunchNavigationAsync(Event.ScheduleLatitude, Event.ScheduleLongitude, Event.Address);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MapsCommand: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "An unexpected error occurred while trying to open maps.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CopyAddress()
        {
            if (Event != null && !string.IsNullOrWhiteSpace(Event.Address))
            {
                await Clipboard.SetTextAsync(Event.Address);
                await Shell.Current.DisplayAlert("Copied", "The address has been copied to the clipboard.", "OK");
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            // If there is an operation in progress, we do not allow returning to avoid inconsistent states.
            if (IsBusy)
            {
                return;
            }

            // We use ".." to navigate to the previous page in the navigation stack.
            await Shell.Current.GoToAsync("..");
        }

        // This method is automatically fired when the 'Event' property receives a value
        partial void OnEventChanged(ScheduleDto value)
        {
            // ✅ reset signature flow state when a new event arrives
            SignatureSaved = false;
            _awaitingSignature = false;

            if (value != null)
            {
                PageTitle = value.Name; // "Pull-out" o "Pull-in"
                MapActionText = $"Maps - {value.Name} Address";

                // Determine if the main action (Odometer/Perform) should be visible.
                //    - A "Pull-out" is always the first event, so it is always actionable.
                //    - A "Pull-in" is only actionable if it is the only event left in the list
                //      (which means IsFirstEvent will be true for it).
                IsPrimaryActionVisible =
                    value.Name == "Pull-out"
                    || (value.Name == "Pull-in" && PendingEventsCount == 1);

                EventColor = (Color)_colorConverter.Convert(value, typeof(Color), null, System.Globalization.CultureInfo.CurrentCulture);
                IsOdometerEntered = value.Odometer != null && value.Odometer > -1;
                _ = RefreshStates();
            }
            else
            {
                EventColor = Colors.Gray;
                IsOdometerEntered = false;
            }
        }

        partial void OnPendingEventsCountChanged(int value)
        {
            if (Event == null) return;

            IsPrimaryActionVisible =
                Event.Name == "Pull-out"
                || (Event.Name == "Pull-in" && value == 1);
        }

        // ✅ Added: when returning from SignaturePage with SignatureSaved=true, finish the perform flow
        partial void OnSignatureSavedChanged(bool value)
        {
            if (!value) return;
            if (!_awaitingSignature) return;

            _awaitingSignature = false;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (IsBusy) return;

                IsBusy = true;
                try
                {
                    await CompletePerformAsync();
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        public void StartGpsTracking()
        {
            if (_gpsService.IsTracking) return;

            _gpsService.StartTracking(Event.VehicleRouteId);
            IsTracking = _gpsService.IsTracking;
        }

        public void StopGpsTracking()
        {
            if (!_gpsService.IsTracking) return;

            _gpsService.StopTracking();
            IsTracking = _gpsService.IsTracking;
        }
    }
}
