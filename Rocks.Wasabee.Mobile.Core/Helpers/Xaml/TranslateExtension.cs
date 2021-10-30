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

            ResourceManager resourceManager = new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

            var result = resourceManager.GetString(Text, CultureInfo.CurrentCulture);
            
            if (string.IsNullOrWhiteSpace(result))
                result = $"[{Text}] not found";
            
            return result;
        }
    }
}