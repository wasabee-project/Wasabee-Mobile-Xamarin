using System;
using System.Collections.Generic;
using System.Drawing;
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
            if (descriptor.Id.Equals("wasabee_player_marker"))
            {
                return CreateMarker("wasabee/markers/player.png");
            }

            if (descriptor.Id.Equals("wasabee_player_marker_self"))
            {
                return CreateMarker("wasabee/markers/player_self.png");
            }

            if (descriptor.Id.Equals("wasabee_player_marker_gray"))
            {
                return CreateMarker("wasabee/markers/player_gray.png");
            }
            
            if (descriptor.Id.Contains('|'))
            {
                var fileName = "wasabee/markers/";

                var descriptors = descriptor.Id.Split('|');
                fileName += descriptors[0] switch
                {
                    "DestroyPortalAlert" => "destroy",
                    "UseVirusPortalAlert" => "virus",
                    "CapturePortalMarker" => "capture",
                    "FarmPortalMarker" => "farm",
                    "LetDecayPortalAlert" => "decay",
                    "MeetAgentPortalMarker" => "meetagent",
                    "OtherPortalAlert" => "other",
                    "RechargePortalAlert" => "recharge",
                    "UpgradePortalAlert" => "upgrade",
                    "CreateLinkAlert" => "link",
                    "ExcludeMarker" => "exclude",
                    "GetKeyPortalMarker" => "key",
                    "GotoPortalMarker" => "goto",
                    _ => throw new ArgumentOutOfRangeException(descriptors[0])
                } + $"/{descriptors[1]}.png";

                return CreateMarker(fileName);
            }
            
            if (descriptor.Id.Contains("#"))
                return CreatePin("wasabee/pins/pin_green.png");
            
            return CreatePin($"wasabee/pins/{descriptor.Id}.png");
        }
        
        private static UIImage CreatePin(string bundleFilePath) => CreateImageFromBundleName(bundleFilePath, 30, 60);
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

        // resize the image to be contained within a maximum width and height, keeping aspect ratio
        private static UIImage ResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            if (sourceImage is null)
                return null;

            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Max(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

            if (maxResizeFactor > 1)
                return sourceImage;

            var width = (float) (maxResizeFactor * sourceSize.Width);
            var height = (float) (maxResizeFactor * sourceSize.Height);

            UIGraphics.BeginImageContext(new SizeF(width, height));

            sourceImage.Draw(new RectangleF(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();

            UIGraphics.EndImageContext();

            return resultImage;
        }
    }
}