using Mercurio.Driver.ViewModels;
using Mercurio.Driver.Views;

namespace Mercurio.Driver
{
    public partial class AppShell : Shell
    {       
        public AppShell()
        {
            InitializeComponent();
            BindingContext = new AppShellViewModel();

            //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            //Routing.RegisterRoute(nameof(SchedulePage), typeof(SchedulePage));
            Routing.RegisterRoute(nameof(TodaySchedulePage), typeof(TodaySchedulePage));
            Routing.RegisterRoute(nameof(Views.PullOutDetailPage), typeof(Views.PullOutDetailPage));
            //Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(EventDetailPage), typeof(EventDetailPage));
            Routing.RegisterRoute(nameof(SignaturePage), typeof(SignaturePage));

            Routing.RegisterRoute(nameof(FutureSchedulePage), typeof(FutureSchedulePage));
            Routing.RegisterRoute(nameof(FutureDetailPage), typeof(FutureDetailPage));
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            Routing.RegisterRoute(nameof(ContactPage), typeof(ContactPage));

            //Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
            //Routing.RegisterRoute("HomePage", typeof(Views.HomePage));   
        }       
       
    }
}
