using System;
using System.Globalization;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Helpers.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (string.IsNullOrEmpty(parameterString))
                return string.Empty;

            if (parameterString.Contains("|"))
            {
                var parameters = parameterString.Split('|');
                return (bool)value ? parameters[0] : parameters[1];
            }
            return parameterString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}