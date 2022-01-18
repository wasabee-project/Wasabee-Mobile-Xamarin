using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Helpers.Converters
{
    public class AgentVerificationBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ?
                TranslateExtension.GetValue("Global_Verified") :
                TranslateExtension.GetValue("Global_Unverified");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}