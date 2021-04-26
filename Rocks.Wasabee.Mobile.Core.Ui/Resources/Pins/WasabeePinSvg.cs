using Xamarin.Forms;

namespace Rocks.Wasabee.Mobile.Core.Ui.Resources.Pins
{
    public class WasabeePinSvg
    {
        private Color _color;

        public WasabeePinSvg(string color)
        {
            _color = Color.FromHex(color);
        }

        public string RawData => $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<!DOCTYPE svg PUBLIC ""-//W3C//DTD SVG 1.1//EN"" ""http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd"">
<svg width=""100%"" height=""100%"" viewBox=""0 0 250 300"" version=""1.1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xml:space=""preserve"" xmlns:serif=""http://www.serif.com/"" style=""fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;fill:rgb({_color.R*255:F0},{_color.G*255:F0},{_color.B*255:F0});"">
    <g>
        <path d=""M125,300L105,270L105,70L145,70L145,270Z"" style=""fill:rgb(176,176,176);""/>
        <path d=""M125,300L105,270L105,70L125,70Z"" style=""fill:rgb(192,192,192);""/>
        <circle cx=""125"" cy=""62"" r=""62"" />
        <circle cx=""125"" cy=""62"" r=""50"" style=""fill:rgb(255,255,255); fill-opacity: .1""/>
        <path d=""M88,60a37,37,0,0,1,37,-37"" style=""stroke: white; stroke-opacity: .4; fill: none; stroke-linecap: round; stroke-width: 12;""/>
    </g>
</svg>
";
    }
}