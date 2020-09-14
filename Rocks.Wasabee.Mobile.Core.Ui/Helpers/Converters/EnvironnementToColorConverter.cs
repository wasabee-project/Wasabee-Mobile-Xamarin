using System;
using System.Globalization;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Helpers.Converters
{
    public class EnvironnementToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value?.ToString())) return Color.Red;

            switch (value.ToString())
            {
                case "dev":
                    return (Color)Application.Current.Resources["PrimaryBlue"];
                case "release":
                    return (Color)Application.Current.Resources["PrimaryGreen"];
                default:
                    return (Color)Application.Current.Resources["PrimaryRed"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}