
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Services;
using Mercurio.Driver.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Mercurio.Driver.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IRunService _runService;
        private readonly IScheduleService _scheduleService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _driverName;

        [ObservableProperty]
        private string _runName;

        [ObservableProperty]
        private string _vehicleName;

        [ObservableProperty]
        private ScheduleDto _nextEvent;

        [ObservableProperty]
        private int _remainingEventsCount;

        [ObservableProperty]
        private string _runLogin; 

        [ObservableProperty]
        private bool _hasData; // To show/hide views

        public DashboardViewModel(IRunService runService, IScheduleService scheduleService)
        {
            _runService = runService;
            _scheduleService = scheduleService;
            
        }

        [RelayCommand]
        private async Task InitializeAsync()
        {
            IsBusy = true;
            HasData = false;
            try
            {
                //var userId = Preferences.Get("UserId", 0);
                var userIdStr = Preferences.Get("UserId", "0");
                if (!int.TryParse(userIdStr, out int userId) || userId == 0)
                {
                    // If it cannot be converted or is 0, we do not continue.
                    return;
                }
                //userId = 6; // para probar
                if (userId == 0) return;

                // Get the active route
                var activeRun = await _runService.GetActiveRunByDriverIdAsync(userId);

                if (activeRun != null)
                {                    
                    RunLogin = activeRun.SmartphoneLogin;
                    Preferences.Set("runLogin", RunLogin);
                   
                    var pendingEvents = await _scheduleService.GetPendingSchedulesByRunAsync(activeRun.SmartphoneLogin, DateTime.Today);
                   
                    DriverName = activeRun.Driver?.FullName ?? "N/A"; 
                    RunName = activeRun.Name;
                    VehicleName = activeRun.Vehicle?.Name ?? "N/A"; 

                    NextEvent = pendingEvents.FirstOrDefault();
                    RemainingEventsCount = pendingEvents.Count();
                    HasData = true;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToSchedule()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(RunLogin)) return;

            IsBusy = true;
            try
            {
                await Shell.Current.GoToAsync(nameof(TodaySchedulePage), new Dictionary<string, object>
                {
                    { "runLogin", RunLogin }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}