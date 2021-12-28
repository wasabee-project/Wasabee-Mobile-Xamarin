using Rocks.Wasabee.Mobile.Core.Ui.Controls;
using Rocks.Wasabee.Mobile.iOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessEditor), typeof(BorderlessEditorRenderer))]
namespace Rocks.Wasabee.Mobile.iOS.Renderers
{
    public class BorderlessEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.Layer.BorderWidth = 0;
            }
        }
    }
}