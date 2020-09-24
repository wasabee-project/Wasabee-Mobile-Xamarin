using System;
using System.Globalization;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Helpers.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ?
                (Color)Application.Current.Resources["PrimaryGreen"] :
                (Color)Application.Current.Resources["PrimaryGray"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}