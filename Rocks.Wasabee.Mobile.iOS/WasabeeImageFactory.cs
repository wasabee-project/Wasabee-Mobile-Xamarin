using Rocks.Wasabee.Mobile.Core.Ui.Resources.Pins;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using UIKit;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.iOS.Factories;

namespace Rocks.Wasabee.Mobile.iOS
{
    public class WasabeeImageFactory : IImageFactory
    {
        private static readonly Dictionary<string, UIImage> ImagesCache = new Dictionary<string, UIImage>();

        public UIImage ToUIImage(BitmapDescriptor descriptor)
        {
            switch (descriptor.Id)
            {
                case "wasabee_player_marker":
                    return CreateMarker("wasabee/markers/player.png");
                case "wasabee_player_marker_self":
                    return CreateMarker("wasabee/markers/player_self.png");
                case "wasabee_player_marker_gray":
                    return CreateMarker("wasabee/markers/player_gray.png");
            }

            if (descriptor.Id.Contains('|'))
            {
                var fileName = "wasabee/markers/";

                var descriptors = descriptor.Id.Split('|');
                fileName += descriptors[0] switch
                {
                    "DestroyPortal" => "destroy",
                    "UseVirus" => "virus",
                    "CapturePortal" => "capture",
                    "FarmPortal" => "farm",
                    "LetDecay" => "decay",
                    "MeetAgent" => "meetagent",
                    "Other" => "other",
                    "RechargePortal" => "recharge",
                    "UpgradePortal" => "upgrade",
                    "CreateLink" => "link",
                    "Exclude" => "exclude",
                    "GetKey" => "key",
                    "GoToPortal" => "goto",
                    _ => throw new ArgumentOutOfRangeException(descriptors[0])
                } + $"/{descriptors[1].ToLowerInvariant()}.png";

                return CreateMarker(fileName);
            }

            var pinColor = descriptor.Id.StartsWith("pin_") ?
                descriptor.Id.Substring(4) :
                "#DD3D45"; // default goes to RED

            return CreatePin(pinColor);
        }

        private static UIImage CreatePin(string color) => CreateImageFromWasabeePinSvg(color, 50, 60);
        private static UIImage CreateMarker(string bundleFilePath) => CreateImageFromBundleName(bundleFilePath, 35, 60);
        private static UIImage CreateImageFromBundleName(string bundleFilePath, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(bundleFilePath))
                return Google.Maps.Marker.MarkerImage(UIColor.Red);

            if (ImagesCache.ContainsKey(bundleFilePath))
                return ImagesCache[bundleFilePath];

            var image = ResizeImage(UIImage.FromBundle(bundleFilePath), width, height);
            if (image is null)
                return Google.Maps.Marker.MarkerImage(UIColor.Red);

            ImagesCache.Add(bundleFilePath, image);

            return image;
        }

        private static UIImage CreateImageFromWasabeePinSvg(string color, int width, int height)
        {
            if (ImagesCache.ContainsKey(color))
                return ImagesCache[color];

            SKPicture picture;
            var pin = new WasabeePinSvg(color);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pin.RawData)))
                picture = new SkiaSharp.Extended.Svg.SKSvg().Load(stream);

            var image = SKImage.FromPicture(picture, new SKSizeI((int)picture.CullRect.Size.Width, (int)picture.CullRect.Size.Height)).ToUIImage();
            var resizedImage = ResizeImage(image, width, height);
            if (resizedImage is null)
                return Google.Maps.Marker.MarkerImage(UIColor.Red);

            ImagesCache.Add(color, resizedImage);
            return resizedImage;
        }

        // resize the image to be contained within a maximum width and height, keeping aspect ratio
        private static UIImage ResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            if (sourceImage is null)
                return null;

            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

            if (maxResizeFactor > 1)
                return sourceImage;

            var width = (float)(maxResizeFactor * sourceSize.Width);
            var height = (float)(maxResizeFactor * sourceSize.Height);

            UIGraphics.BeginImageContext(new SizeF(width, height));

            sourceImage.Draw(new RectangleF(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();

            UIGraphics.EndImageContext();

            return resultImage;
        }
    }
}