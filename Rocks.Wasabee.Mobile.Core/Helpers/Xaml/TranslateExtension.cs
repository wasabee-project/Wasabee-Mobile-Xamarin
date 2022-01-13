using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rocks.Wasabee.Mobile.Core.Helpers.Xaml
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        const string ResourceId = "Rocks.Wasabee.Mobile.Core.Resources.I18n.Strings";

        public string Text { get; set; } = string.Empty;

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return string.Empty;
            
            return GetValueInternal(Text);
        }

        public static string GetValue(string key)
        {
            return GetValueInternal(key);
        }

        private static string GetValueInternal(string key)
        {
            ResourceManager resourceManager = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

            var result = resourceManager.GetString(key, CultureInfo.CurrentUICulture);
            
            if (string.IsNullOrWhiteSpace(result))
                result = $"[{key}] not found";

            return result;
        }
    }
}