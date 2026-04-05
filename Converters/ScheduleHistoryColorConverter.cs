using System.Globalization;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Converters
{
    public class ScheduleHistoryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {           
            if (value is not ScheduleHistoryDto schedule)
                return Colors.Gray;
        
            if (schedule.Name == "Pull-out" || schedule.Name == "Pull-in")
                return Colors.White; // The timeline for Pull-out in the image is white.

            if (schedule.EventType == ScheduleEventType.Pickup && schedule.TripType == "Appointment")
                return Color.FromArgb("#2E7D32"); // Green

            if (schedule.EventType == ScheduleEventType.Dropoff && schedule.TripType == "Appointment")
                return Color.FromArgb("#C62828"); // Red

            if (schedule.EventType == ScheduleEventType.Pickup && schedule.TripType == "Return")
                return Color.FromArgb("#1565C0"); // Blue

            if (schedule.EventType == ScheduleEventType.Dropoff && schedule.TripType == "Return")
                return Color.FromArgb("#6A1B9A"); // Purple

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
