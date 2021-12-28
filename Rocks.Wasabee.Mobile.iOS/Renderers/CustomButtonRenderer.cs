using Rocks.Wasabee.Mobile.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Button), typeof(CustomButtonRenderer))]
namespace Rocks.Wasabee.Mobile.iOS.Renderers
{
    public class CustomButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Element != null)
            {
                Element.Padding = new Thickness(10, 5);
                Element.Text = Element.Text.ToUpperInvariant();
                Element.FontAttributes = FontAttributes.Bold;
            }
        }
    }
}