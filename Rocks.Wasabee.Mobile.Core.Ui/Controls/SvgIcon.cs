using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Rocks.Wasabee.Mobile.Core.Ui.Controls
{
    public class SvgIcon : Frame
    {
        public static readonly BindableProperty ResourceProperty = BindableProperty.Create(
            nameof(Resource), 
            typeof(string), 
            typeof(SvgIcon), 
            default(string), 
            propertyChanged: RedrawCanvas);
 
        public string Resource
        {
            get => (string)GetValue(ResourceProperty);
            set => SetValue(ResourceProperty, value);
        }

        private readonly SKCanvasView _canvasView = new();
 
        public SvgIcon()
        {
            Padding = new Thickness(0);
 
            // Thanks to TheMax for pointing out that on mobile, the icon will have a shadow by default.
            // Also it has a white background, which we might not want.
            HasShadow = false;
            BackgroundColor = Color.Transparent;
 
            Content = _canvasView;
            _canvasView.PaintSurface += CanvasViewOnPaintSurface;
        }

        private void CanvasViewOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;
            canvas.Clear();
 
            if (string.IsNullOrEmpty(Resource))
                return;

            using var stream = GetType().Assembly.GetManifestResourceStream(Resource);
            var svg = new SKSvg();
            svg.Load(stream);

            var info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);
 
            var bounds = svg.ViewBox;
            var xRatio = info.Width / bounds.Width;
            var yRatio = info.Height / bounds.Height;
 
            var ratio = Math.Min(xRatio, yRatio);
 
            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);
            canvas.DrawPicture(svg.Picture);
        }

        private static void RedrawCanvas(BindableObject bindable, object oldvalue, object newvalue)
        {
            var svgIcon = bindable as SvgIcon;
            svgIcon?._canvasView.InvalidateSurface();
        }
    }
}