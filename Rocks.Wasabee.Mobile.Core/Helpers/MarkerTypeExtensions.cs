using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class MarkerTypeExtensions
    {
        public static string ToFriendlyString(this MarkerType markerType)
        {
            return markerType switch
            {
                MarkerType.DestroyPortal => "Destroy portal",
                MarkerType.UseVirus => "Use virus",
                MarkerType.CapturePortal => "Capture portal",
                MarkerType.FarmPortal => "Farm keys",
                MarkerType.LetDecay => "Let decay",
                MarkerType.MeetAgent => "Meet Agent",
                MarkerType.Other => "Other",
                MarkerType.RechargePortal => "Recharge portal",
                MarkerType.UpgradePortal => "Upgrade portal",
                MarkerType.CreateLink => "Create link",
                MarkerType.Exclude => "Exclude Marker",
                MarkerType.GetKey => "Get Key",
                MarkerType.GoToPortal => "Go to portal",
                _ => throw new ArgumentOutOfRangeException(markerType.ToString())
            };
        }
    }
}